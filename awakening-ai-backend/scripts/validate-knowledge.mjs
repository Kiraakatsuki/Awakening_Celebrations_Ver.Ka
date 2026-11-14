import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';

const root = new URL('../knowledge/', import.meta.url);
const unity = new URL('../../', import.meta.url);
const read = (path, base = root) => readFileSync(new URL(path, base), 'utf8');
const json = (path) => JSON.parse(read(path));
const kinds = ['sources', 'overview', 'timeline', 'editions', 'defiles', 'performers', 'images', 'contributors', 'evaluation'];
const { sources, overview, timeline, editions, defiles, performers, images, contributors, evaluation } =
  Object.fromEntries(kinds.map((kind) => {
    const rows = json('data/' + kind + '.json');
    assert(Array.isArray(rows) && rows.length, kind + ' must be a non-empty collection');
    return [kind, rows];
  }));
const heal = json('data/heal2026.json');
const contextSchema = json('schema/context.schema.json');
const locales = ['en', 'zh-TW'];
const nonempty = (value) => typeof value === 'string' && !!value.trim();
const unique = (values, label) => assert.equal(new Set(values).size, values.length, 'Duplicate ' + label);
const byId = (rows) => new Map(rows.map((record) => [record.id, record]));
const sourcedBy = byId(sources);
const editionById = byId(editions);
const defileById = byId(defiles);
const performerById = byId(performers);
const imageById = byId(images);
const facts = [...overview, ...timeline, ...editions, heal, ...heal.program, ...heal.distinctions,
  ...defiles, ...contributors, ...performers, ...images];
const factById = byId(facts);

function bilingual(value, id, predicate = nonempty) {
  assert(value && typeof value === 'object', id + ' needs bilingual content');
  assert.deepEqual(Object.keys(value).sort(), locales, id + ' invalid locales');
  for (const locale of locales) assert(predicate(value[locale]), id + ' invalid ' + locale);
}

function citations(record) {
  const { id, sources: refs } = record;
  assert(Array.isArray(refs) && refs.length, id + ' missing evidence');
  unique(refs, id + ' source');
  for (const ref of refs) assert(sourcedBy.has(ref), id + ' unknown source ' + ref);
}

function date(value, id) {
  assert.match(value, /^\d{4}-\d{2}-\d{2}$/, id + ' invalid date format');
  assert.equal(new Date(value).toISOString().slice(0, 10), value, id + ' invalid calendar date');
}

const records = [...sources, ...facts, ...evaluation];
unique(records.map((row) => row.id), 'canonical ID');
for (const row of records) assert.match(row.id, /^[A-Z0-9]+(?:-[A-Z0-9]+)*$/);
for (const source of sources) {
  const { id, kind, url, accessed, date: pub, dateBasis } = source;
  assert(['organizer', 'government', 'reporting', 'research', 'repository'].includes(kind), id);
  assert(['id', 'en'].includes(source.language) && [source.title, source.publisher, source.evidence, source.access].every(nonempty), id);
  date(accessed, id);
  if (pub !== null) {
    assert.match(pub, /^\d{4}(?:-(?:0[1-9]|1[0-2])(?:-\d{2})?)?$/, id);
    if (pub.length === 10) date(pub, id);
    assert(pub <= accessed, id + ' publication after research');
    assert(['published', 'article_dateline', 'issue_date', 'page_last_updated'].includes(dateBasis), id);
  } else {
    assert.equal(dateBasis, kind === 'repository' ? 'repository_revision' : 'undated', id);
  }
  if (kind === 'repository') {
    assert.equal(url, null);
    assert.match(source.revision, /^[0-9a-f]{40}$/);
    assert(source.paths.length);
    for (const path of source.paths) assert(read(path, unity).length, path);
  } else assert.match(url, /^https:\/\/[^/]+(?:\/|$)/);
}
unique(sources.filter((row) => row.url).map((row) => row.url), 'source URL');
for (const record of facts) {
  citations(record);
  if (Object.hasOwn(record, 'editionId') && record.editionId !== null)
    assert(editionById.has(record.editionId), record.id + ' unknown edition');
}

for (const record of [...overview, ...timeline]) {
  assert(['verified-fact', 'reported-history', 'reported-outcome', 'exhibition-fact'].includes(record.provenance), record.id);
  assert(record.year === null || (Number.isInteger(record.year) && record.year >= 1900 && record.year <= 2026), record.id);
  bilingual(record.text, record.id);
}
const editionNumbers = editions.filter((row) => row.format === 'numbered').map((row) => row.edition);
assert.deepEqual(editionNumbers, Array.from({ length: 24 }, (_, n) => n + 1));
assert.deepEqual([...new Set(editions.map((row) => row.year))], Array.from({ length: 24 }, (_, n) => n + 2003));
unique(editions.filter((row) => row.theme).map((row) => row.theme.toLowerCase()), 'umbrella theme');
for (const row of editions) {
  const numbered = row.format === 'numbered';
  assert(numbered ? row.id === 'ED-JFC-' + String(row.edition).padStart(2, '0') :
    row.format === 'related-virtual' && row.edition === null && row.year === 2020, row.id);
  const expected = row.theme === null ? (numbered ? 'umbrella-not-established' : 'program-only') : 'sourced';
  assert.equal(row.themeStatus, expected, row.id);
  assert(row.theme === null || (numbered && nonempty(row.theme)), row.id);
  assert(row.subtitle === null || nonempty(row.subtitle), row.id);
  assert(['historical-corroboration', 'contemporary-evidence'].includes(row.confidence), row.id);
  assert(Array.isArray(row.aliases) && row.aliases.every(nonempty), row.id);
  assert(Array.isArray(row.defileNames) && row.defileNames.every(nonempty), row.id);
  unique(row.defileNames, row.id + ' defile');
  assert(row.defileNames.length ? ['published-list', 'examples'].includes(row.defileCoverage) :
    row.defileCoverage === 'unresolved', row.id);
  bilingual(row.summary, row.id);
}

assert.equal(heal.editionId, 'ED-JFC-24');
assert.equal(heal.title, editionById.get(heal.editionId).theme);
assert.equal(heal.expansion, editionById.get(heal.editionId).subtitle);
const names = ['Gaia', 'Noise', 'Pandemic', 'Sanctuary', 'White Guardian', 'Conscience', 'Re-Birth', 'Roots'];
assert.deepEqual(heal.narrativeOrder.map((id) => defileById.get(id)?.name), names);
assert.deepEqual(editionById.get(heal.editionId).defileNames, names);
assert.deepEqual(defiles.map((row) => row.id), heal.narrativeOrder);
assert.equal(heal.scheduledDefileOrder.provenance, 'announced');
assert.deepEqual([...heal.scheduledDefileOrder.ids].sort(), [...heal.narrativeOrder].sort());
citations({ ...heal.scheduledDefileOrder, id: heal.id + '/scheduledDefileOrder' });
date(heal.startDate, heal.id);
date(heal.endDate, heal.id);
assert.deepEqual(heal.program.map((row) => row.date), ['2026-07-24', '2026-07-25', '2026-07-26']);
assert.equal(heal.startDate, heal.program[0].date);
assert.equal(heal.endDate, heal.program.at(-1).date);
bilingual(heal.narrative.text, heal.id);
citations({ ...heal.narrative, id: heal.id + '/narrative' });
assert.equal(heal.narrative.provenance, 'official-theme');
for (const item of heal.program) {
  assert(['observed', 'mixed', 'announced'].includes(item.status) && item.programs.every(nonempty), item.id);
  bilingual(item.text, item.id);
}
for (const item of heal.distinctions) {
  assert(['context-dependent', 'announced'].includes(item.status), item.id);
  bilingual(item.text, item.id);
}
for (const item of defiles) {
  assert.equal(item.editionId, heal.editionId);
  assert(Array.isArray(item.aliases) && item.aliases.every(nonempty), item.id);
  if (item.relatedTerms) assert(Array.isArray(item.relatedTerms) && item.relatedTerms.every(nonempty), item.id);
  bilingual(item.display, item.id);
  bilingual(item.openQuestions, item.id);
  assert(item.officialTheme.provenance === 'official-theme' &&
    item.relationshipToGaia.provenance === 'curatorial-reading' &&
    ['reported-outcome', 'official-theme'].includes(item.festivalPresentation.provenance), item.id);
  for (const section of ['officialTheme', 'festivalPresentation', 'relationshipToGaia']) {
    citations({ ...item[section], id: item.id + '/' + section });
    bilingual(item[section].text, item.id);
  }
  unique(item.relatedDefileIds, item.id + ' related defile');
  assert(item.relatedDefileIds.every((id) => defileById.has(id) && id !== item.id), item.id);
  assert(item.confirmedCredits.every((id) => byId(contributors).has(id)), item.id);
}

const routeFile = read('Assets/LivingGlass/Core/LivingGlassRoutes.cs', unity);
const unityIds = [...routeFile.match(/Performers = \{([^}]+)\}/)[1].matchAll(/"([^"]+)"/g)].map((m) => m[1]);
const bindings = read('Assets/LivingGlass/Assets/LivingGlassAssets.asset', unity);
const stageBindings = Object.fromEntries(['portraits', 'activation', 'living'].map((field) => {
  const entries = bindings.match(new RegExp('^  ' + field + ':\\r?\\n((?:  - [^\\r\\n]+\\r?\\n)+)', 'm'))[1];
  return [field, [...entries.matchAll(/guid: ([0-9a-f]{32})/g)].map((m) => m[1])];
}));
const scene = read('Assets/Scenes/AwakeningLivingGlass.unity', unity);
const assetGuid = read('Assets/LivingGlass/Assets/LivingGlassAssets.asset.meta', unity).match(/^guid: ([0-9a-f]{32})/m)[1];
assert(scene.includes('guid: ' + assetGuid), 'Living Glass scene is missing its asset catalog');
assert.equal(performers.length, 8);
assert.deepEqual(performers.filter((row) => !row.featured).map((row) => row.id).sort(), ['JFC26-NOISE', 'JFC26-ROOTS']);
unique(performers.map((row) => row.defileId), 'performer defile');
assert.deepEqual(performers.filter((row) => row.featured).sort((a, b) => a.unity.index - b.unity.index)
  .map((row) => row.unity.performerId), unityIds);
for (const row of performers) {
  assert.equal(row.defileId, 'DEF-' + row.id);
  assert(defileById.has(row.defileId) && row.idAuthority === 'application', row.id);
  bilingual(row.names, row.id);
  bilingual(row.notes, row.id);
  if (!row.featured) {
    assert.equal(row.unity, null, row.id);
    assert.deepEqual(row.imageIds, [], row.id);
    continue;
  }
  const { performerId, index, chapterCode, assistantRoute, focusRoute, reflectionRoute, galleryButton, assistantButton } = row.unity;
  assert.equal(chapterCode, 'PER-00' + (index + 1), row.id);
  assert.equal(assistantRoute, 'Assistant-' + performerId, row.id);
  assert.equal(focusRoute, performerId + '---FOCUS-VIEW---' + chapterCode, row.id);
  assert.equal(reflectionRoute, performerId + '---POST-VIDEO-AI---' + chapterCode, row.id);
  assert.equal(galleryButton, 'presence-' + index, row.id);
  assert.equal(assistantButton, 'assistant-' + index, row.id);
  assert.deepEqual(row.imageIds, images.filter((image) => image.performerId === row.id).map((image) => image.id), row.id);
}
assert.equal(images.length, 18);
assert.equal(images.filter((row) => row.selectable).length, 6);
unique(images.map((row) => row.unity.guid), 'image GUID');
for (const row of images) {
  const parent = performerById.get(row.performerId);
  assert(parent?.featured && parent.imageIds.includes(row.id), row.id);
  const { assetField, assetPath, guid, index, width, height } = row.unity;
  assert(matchAssetField(row, assetField) && index === parent.unity.index, row.id);
  assert.equal(stageBindings[assetField]?.[index], guid, row.id + ' serialized binding');
  assert.equal(read(assetPath + '.meta', unity).match(/^guid: ([0-9a-f]{32})/m)?.[1], guid, row.id + ' asset metadata');
  const png = readFileSync(new URL(assetPath, unity));
  assert.equal(png.subarray(1, 4).toString(), 'PNG', row.id);
  assert.equal(png.readUInt32BE(16), width, row.id);
  assert.equal(png.readUInt32BE(20), height, row.id);
  bilingual(row.label, row.id);
  bilingual(row.observation.text, row.id);
  assert.equal(row.observation.provenance, 'visual-observation');
  citations({ ...row.observation, id: row.id + '/observation' });
  assert.equal(row.verification.mapping, 'verified-repository', row.id);
  for (const key of ['identity', 'rights']) assert(['unresolved', 'verified'].includes(row.verification[key]), row.id);
  if (row.verification.identity === 'unresolved') assert([row.costumeName, row.depictedPerformer, row.designer].every((v) => v === null), row.id);
  if (row.verification.rights === 'unresolved') assert.equal(row.rights, null, row.id);
  if (row.approvedDescription !== null) {
    bilingual(row.approvedDescription.text, row.id);
    citations({ ...row.approvedDescription, id: row.id + '/approvedDescription' });
    assert.equal(row.approvedDescription.provenance, 'approved-exhibition', row.id);
  }
}

function matchAssetField(row, field) {
  const roles = { portraits: [1, 'selectable-portrait'], activation: [2, 'activation-art'], living: [3, 'living-still'] };
  if (!roles[field]) return false;
  const [number, role] = roles[field];
  return row.role === role && row.id === row.performerId + '-IMG0' + number && row.selectable === (number === 1);
}

for (const row of contributors) {
  assert(nonempty(row.name) && ['founder', 'organizer', 'ambassador', 'designer', 'performer'].includes(row.role), row.id);
  assert.equal(row.scope, row.editionId === null ? 'organization' : 'festival-event');
  bilingual(row.summary, row.id);
}

function classifyContext(ctx) {
  if (ctx === undefined) return 'none';
  const { properties } = contextSchema;
  if (!ctx || typeof ctx !== 'object' || Array.isArray(ctx) ||
    Object.keys(ctx).some((key) => !Object.hasOwn(properties, key)) || !properties.source.enum.includes(ctx.source)) return 'invalid';
  for (const key of ['performerId', 'imageId'])
    if (ctx[key] != null && (typeof ctx[key] !== 'string' || !new RegExp(properties[key].pattern).test(ctx[key]))) return 'invalid';
  const performer = performerById.get(ctx.performerId);
  const image = imageById.get(ctx.imageId);
  if (ctx.performerId != null && !performer) return 'invalid';
  if (ctx.source === 'performer-page' && !performer?.featured) return 'invalid';
  if (ctx.imageId != null && !image) return 'unknown-image';
  if (image && performer && image.performerId !== performer.id) return 'invalid';
  return 'valid';
}

for (const row of evaluation) {
  bilingual(row.question, row.id);
  bilingual(row.expected.answer, row.id);
  if (row.priorTurn) bilingual(row.priorTurn, row.id);
  const { behavior, recordIds, mustMention, mentionAny } = row.expected;
  assert(['answer', 'attributed-interpretation', 'uncertain', 'clarify', 'invalid-context'].includes(behavior), row.id);
  unique(recordIds, row.id + ' cited record');
  assert(recordIds.every((id) => factById.has(id)), row.id + ' dangling expected citation');
  if (behavior !== 'clarify') assert(recordIds.length, row.id + ' missing expected citation');
  const state = classifyContext(row.context);
  assert.equal(state === 'invalid', behavior === 'invalid-context', row.id);
  assert.equal(state === 'unknown-image', behavior === 'clarify', row.id);
  bilingual(mustMention, row.id, (terms) => Array.isArray(terms) && terms.every(nonempty));
  if (mentionAny) bilingual(mentionAny, row.id, (terms) => Array.isArray(terms) && terms.length && terms.every(nonempty));
  for (const locale of locales) {
    const answer = row.expected.answer[locale].toLowerCase();
    assert(mustMention[locale].every((term) => answer.includes(term.toLowerCase())), row.id + ' term mismatch');
    if (mentionAny) assert(mentionAny[locale].some((term) => answer.includes(term.toLowerCase())), row.id + ' alternatives mismatch');
  }
}

console.log('PASS Stage 5.1 knowledge, source, evaluation and Unity image contracts');
console.log(JSON.stringify({
  numberedEditions: editionNumbers.length,
  sourcedUmbrellaThemes: editions.filter((row) => row.themeStatus === 'sourced').length,
  earlyUmbrellasUnestablished: editions.filter((row) => row.themeStatus === 'umbrella-not-established').length,
  relatedVirtualPrograms: editions.filter((row) => row.format === 'related-virtual').length,
  sources: sources.length, publicSources: sources.filter((row) => row.url).length,
  defiles: defiles.length, featured: performers.filter((row) => row.featured).length,
  imageBindings: images.length, selectableImages: images.filter((row) => row.selectable).length,
  unresolvedImageIdentities: images.filter((row) => row.verification.identity === 'unresolved').length,
  contributorRecords: contributors.length, bilingualQuestionPairs: evaluation.length
}, null, 2));
