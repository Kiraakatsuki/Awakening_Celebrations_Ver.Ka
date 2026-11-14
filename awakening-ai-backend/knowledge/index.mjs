import { readFileSync } from 'node:fs';

export const locales = ['en', 'zh-TW'];
const dataDirectory = new URL('./data/', import.meta.url);
const read = (directory, name) => JSON.parse(readFileSync(new URL(name + '.json', directory), 'utf8'));
const normalized = (text) => text.normalize('NFKC').toLowerCase().replace(/[‐‑–—]/g, '-');
const string = (value) => typeof value === 'string' && value.trim().length > 0;
const words = (text) => normalized(text).match(/[a-z0-9]+|[\p{Script=Han}]+/gu) ?? [];
const stopWords = new Set('a an and are about can did do does for how i in is it me of on or that the this to us was were what when which who why with you your'.split(' '));
const terms = (text) => words(text).flatMap((word) =>
  /\p{Script=Han}/u.test(word) ? Array.from({ length: Math.max(0, word.length - 1) }, (_, i) => word.slice(i, i + 2)) :
    stopWords.has(word) ? [] : [word]);

function match(text, phrase) {
  phrase = normalized(phrase);
  let at = text.indexOf(phrase);
  while (at >= 0) {
    if ((!/^[a-z0-9]/.test(phrase) || !/[a-z0-9]/.test(text[at - 1] ?? '')) &&
        (!/[a-z0-9]$/.test(phrase) || !/[a-z0-9]/.test(text[at + phrase.length] ?? ''))) return true;
    at = text.indexOf(phrase, at + phrase.length);
  }
  return false;
}

export function loadKnowledge(directory = dataDirectory) {
  const groups = Object.fromEntries(['sources', 'overview', 'timeline', 'editions', 'defiles', 'performers', 'images', 'contributors']
    .map((name) => {
      const rows = read(directory, name);
      if (!Array.isArray(rows) || !rows.length) throw new Error('Invalid knowledge collection');
      return [name, rows];
    }));
  const heal = read(directory, 'heal2026');
  const records = [...Object.values(groups).flat(), heal, ...heal.program, ...heal.distinctions];
  const ids = new Set();
  const byId = (rows) => new Map(rows.map((row) => [row.id, row]));
  const sources = byId(groups.sources);
  function verifyRefs(refs) {
    if (!Array.isArray(refs) || refs.length === 0 || refs.some((id) => !sources.has(id))) throw new Error('Invalid knowledge provenance');
    return refs;
  }
  for (const row of records) {
    if (!/^[A-Z0-9-]+$/.test(row.id) || ids.has(row.id)) throw new Error('Invalid or duplicate knowledge ID');
    ids.add(row.id);
    if (!sources.has(row.id)) verifyRefs(row.sources);
  }
  for (const source of sources.values()) {
    if (!string(source.title) || !string(source.publisher) || (source.kind === 'repository' ?
      source.url !== null : typeof source.url !== 'string' || new URL(source.url).protocol !== 'https:'))
      throw new Error('Invalid knowledge source');
  }
  const performers = byId(groups.performers);
  const images = byId(groups.images);
  const defiles = byId(groups.defiles);
  const editions = byId(groups.editions);
  if (performers.size !== 8 || defiles.size !== 8 || [...performers.values()].filter((row) => row.featured).length !== 6 ||
      heal.narrativeOrder.length !== 8 || new Set(heal.narrativeOrder).size !== 8 ||
      heal.narrativeOrder.some((id) => !defiles.has(id)) || !editions.has(heal.editionId)) throw new Error('Invalid HEAL inventory');
  for (const row of performers.values()) {
    if (!defiles.has(row.defileId) || row.defileId !== 'DEF-' + row.id ||
        row.imageIds.some((id) => images.get(id)?.performerId !== row.id)) throw new Error('Invalid performer registry');
  }
  for (const row of images.values()) {
    const parent = performers.get(row.performerId);
    if (!parent?.featured || !parent.imageIds.includes(row.id) || !row.id.startsWith(parent.id + '-IMG'))
      throw new Error('Invalid image registry');
  }
  const passages = [];
  const provenanceKinds = new Set(['verified-fact', 'reported-history', 'reported-outcome', 'exhibition-fact',
    'historical-edition', 'official-theme', 'context-dependent', 'announced', 'curatorial-reading', 'unresolved',
    'approved-exhibition', 'attributed-contributor']);
  function add(record, section, text, provenance, refs = record.sources, tags = []) {
    if (!provenanceKinds.has(provenance) || !text ||
        locales.some((locale) => !string(text[locale]) || text[locale].length > 1024))
      throw new Error('Invalid knowledge passage');
    passages.push({
      recordId: record.id, section, key: record.id + '/' + section, provenance,
      sourceIds: verifyRefs(refs), text,
      tokens: Object.fromEntries(locales.map((locale) => [locale, new Set(terms(text[locale] + ' ' + tags.join(' ')))])),
    });
  }
  for (const row of [...groups.overview, ...groups.timeline]) add(row, 'summary', row.text, row.provenance);
  for (const row of groups.editions) {
    const heading = row.theme ? [row.theme, row.subtitle].filter(Boolean).join(' · ') :
      row.format === 'related-virtual' ? 'World Kids Carnival' : null;
    const list = row.defileNames.length ? ' ' + row.defileNames.join(', ') : '';
    add(row, 'edition', {
      en: row.year + ' · ' + (heading ?? 'Umbrella title unconfirmed') + '. ' + row.summary.en +
        (list ? ' Documented defiles or examples:' + list + '.' : ''),
      'zh-TW': row.year + ' 年 · ' + (heading ?? '總主題名稱尚未確認') + '。' + row.summary['zh-TW'] +
        (list ? ' 有資料記載的分隊或例子：' + list + '。' : ''),
    }, 'historical-edition', row.sources, [...row.aliases, 'theme edition year', '主題 屆次 年份']);
  }
  add(heal, 'facts', {
    en: 'HEAL means Humanity, Earth, and Life. JFC’s 24th edition ran July 24–26, 2026 in Jember, East Java, Indonesia. Its eight official defiles are ' +
      heal.narrativeOrder.map((id) => defiles.get(id).name).join(', ') + '.',
    'zh-TW': 'HEAL 意指 Humanity, Earth, and Life（人類、地球與生命）。JFC 第 24 屆於 2026 年 7 月 24 至 26 日在印尼東爪哇 Jember 舉行。八支官方分隊為 ' +
      heal.narrativeOrder.map((id) => defiles.get(id).name).join('、') + '。',
  }, 'verified-fact', heal.sources, ['theme', 'count', '主題', '幾支', '八支']);
  add(heal, 'narrative', heal.narrative.text, heal.narrative.provenance, heal.narrative.sources);
  for (const item of heal.program) add(item, 'program', item.text, 'reported-outcome', item.sources, item.programs);
  for (const item of heal.distinctions) add(item, 'distinction', item.text, item.status);
  for (const row of defiles.values()) {
    for (const section of ['officialTheme', 'festivalPresentation', 'relationshipToGaia'])
      add(row, section, row[section].text, row[section].provenance, row[section].sources, [row.name, ...row.aliases]);
    add(row, 'unverifiedCredits', row.openQuestions, 'unresolved', row.sources);
  }
  for (const row of performers.values()) add(row, 'exhibition', row.notes, 'exhibition-fact');
  for (const row of images.values()) {
    if (row.approvedDescription) add(row, 'approvedDescription', row.approvedDescription.text, 'approved-exhibition', row.approvedDescription.sources);
    else add(row, 'binding', {
      en: 'Registered exhibition artwork for ' + performers.get(row.performerId).names.en +
        '. The depicted identity, costume designer, materials and image rights have not been verified in this record.',
      'zh-TW': performers.get(row.performerId).names['zh-TW'] + ' 的登錄展圖。此記錄尚未核實圖中人物、服裝設計者、材料或圖像權利。',
    }, 'exhibition-fact');
  }
  for (const row of groups.contributors) add(row, 'role', row.summary, 'attributed-contributor');
  return { sources, performers, defiles, editions, images, passages, contributors: groups.contributors };
}

const referring = /(?:\b(?:this|that|it|these|those|previous one|following year|next year|tell me more|continue)\b|這張|這件|這套|這個|那個|它|隔年|下一年|多說|再多|再說|前一)/iu;
const imageQuestion = /(?:\b(?:costume|image|portrait|picture|outfit|fabric|material)\b|服裝|圖像|圖片|這張圖|肖像|材質|材料|服飾)/iu;
const identityQuestion = /(?:\b(?:designed|designer|made|created|wore|wearing|identity|material|fabric|plastic|rights|copyright|who played|who portrayed|who is (?:in|this))\b|誰設計|誰做|誰創作|誰穿|誰演|是誰|材料|材質|塑膠|版權|著作權|權利)/iu;
const broadHistory = /history|origins|歷史|起源/iu;
export function retrieve(knowledge, { message, locale, context, previous, limit = 6 }) {
  const q = normalized(message);
  const tokens = [...new Set(terms(q))];
  const performers = [...knowledge.performers.values()];
  const explicit = performers.filter((row) => {
    const defile = knowledge.defiles.get(row.defileId);
    return [row.id, defile.id, row.names.en, row.names['zh-TW'], ...defile.aliases, ...(defile.relatedTerms ?? [])]
      .some((alias) => match(q, alias));
  });
  const namedPeople = knowledge.contributors.filter((row) => match(q, row.name));
  const explicitImages = [...knowledge.images.values()].filter((row) => match(q, row.id));
  const years = new Set([...q.matchAll(/\b(?:19|20)\d\d\b/gu)].map(([year]) => Number(year)));
  const relativeYear = !years.size && previous?.year && /following year|next year|隔年|下一年/iu.test(q);
  if (relativeYear) years.add(previous.year + 1);
  else if (!years.size && previous?.year && referring.test(q) && explicit.length === 0) years.add(previous.year);
  const editions = [...knowledge.editions.values()].filter((row) => years.has(row.year) ||
    [row.id, row.theme, row.subtitle, ...row.aliases].some((name) => name && match(q, name)));
  const subjects = explicit.length ? explicit.map((row) => row.id) :
    explicitImages.length ? [...new Set(explicitImages.map((row) => row.performerId))] :
      referring.test(q) && previous?.performerIds.length ? previous.performerIds :
        context.performerId ? [context.performerId] : [];
  const targetImage = explicitImages.length === 1 ? explicitImages[0] : context.imageId ? knowledge.images.get(context.imageId) :
    !context.unknownImage && !context.performerId && referring.test(q) && previous?.imageId ? knowledge.images.get(previous.imageId) : null;
  const focusImage = targetImage && (explicitImages.length || !explicit.length || explicit.some((row) => row.id === targetImage.performerId))
    ? targetImage : null;
  const creditIntent = identityQuestion.test(q);
  const followup = referring.test(q);
  const imageIntent = imageQuestion.test(q) || (creditIntent && followup && Boolean(focusImage));
  const scores = new Map();
  function boost(id, section, amount) { scores.set(id + '/' + section, amount + (scores.get(id + '/' + section) ?? 0)); }
  for (const performerId of subjects) {
    const row = knowledge.performers.get(performerId);
    boost(row.defileId, 'officialTheme', explicit.length ? 34 : 18);
    boost(row.defileId, 'relationshipToGaia', /connect|related|relationship|symboliz|關係|連|象徵/iu.test(q) ? 26 : 8);
    boost(row.defileId, 'festivalPresentation', 6);
    if (creditIntent && !namedPeople.length && (imageIntent || /who played|who portrayed|誰演/iu.test(q))) boost(row.defileId, 'unverifiedCredits', 30);
    if (!row.featured || /app|exhibit|select|展覽|可選|體驗/iu.test(q)) boost(row.id, 'exhibition', 14);
  }
  for (const item of editions) {
    boost(item.id, 'edition', 36);
    boost('HIST-' + item.year, 'summary', 11);
  }
  if (relativeYear) {
    for (const item of knowledge.editions.values())
      if (item.year === previous.year) boost(item.id, 'edition', 24);
  }
  for (const row of namedPeople) boost(row.id, 'role', 44);
  if (focusImage && (imageIntent || explicitImages.length)) boost(focusImage.id,
    focusImage.approvedDescription ? 'approvedDescription' : 'binding', 55);
  if (/\b(?:founder|founded|history|origins|began|first jfc)\b|創辦|歷史|起源|首屆/iu.test(q)) {
    boost('JFC-OVERVIEW', 'summary', 28);
    boost('PERSON-DYNAND-FARIZ', 'role', 22);
    if (broadHistory.test(q)) {
      for (const id of ['HIST-2001', 'HIST-2003', 'HIST-2022']) boost(id, 'summary', 20);
    }
    if (/first jfc|earliest|首屆/iu.test(q)) boost('ED-JFC-01', 'edition', 40);
  }
  if (/\bheal\b|\b2026\b|八支|幾支|地球與生命/iu.test(q)) {
    boost('JFC-HEAL-2026', 'facts', 36);
    boost('JFC-HEAL-2026', 'narrative', 25);
  }
  if (explicit.length > 1 && /connect|related|relationship|關係|連/iu.test(q)) boost('JFC-HEAL-2026', 'narrative', 27);
  if (/only six|six of|the other two|noise.*roots|只能|六支|六位/iu.test(q)) {
    boost('JFC-EXHIBITION-SCOPE', 'summary', 56);
    boost('JFC26-NOISE', 'exhibition', 28);
    boost('JFC26-ROOTS', 'exhibition', 28);
  }
  if (/grand carnival|dewi bumi|street parade|主遊行|先出場|隊列/iu.test(q)) {
    boost('HEAL-DAY-3', 'program', 23);
    if (/first|order|先|順序/iu.test(q)) boost('HEAL-ORDER', 'distinction', 31);
  }
  if (years.has(2020)) boost('HIST-2020', 'summary', 32);
  for (const passage of knowledge.passages) {
    if (match(q, passage.recordId)) boost(passage.recordId, passage.section, 48);
  }
  const localized = knowledge.passages.map((row) => ({
    row, value: (scores.get(row.key) ?? 0) + Math.min(12, tokens.filter((token) => row.tokens[locale].has(token)).length * 2),
  })).filter(({ value }) => value >= 4).sort((a, b) => b.value - a.value || a.row.key.localeCompare(b.row.key))
    .slice(0, limit).map(({ row }) => ({
      key: row.key, recordId: row.recordId, section: row.section, locale, sourceIds: row.sourceIds,
      provenance: row.provenance, text: row.text[locale],
    }));
  return {
    passages: localized, imageIntent, creditIntent, followup, focusImage, namedPeople,
    earlyThemeUnresolved: /umbrella|main theme|official theme|總主題|官方主題/iu.test(q) &&
      (/first jfc|earliest|首屆/iu.test(q) || (editions.length > 0 && editions.every((row) => row.themeStatus === 'umbrella-not-established'))),
    ambiguous: explicit.length === 0 && followup && !imageIntent && subjects.length > 1,
    subject: { performerIds: subjects.slice(0, 8), year: editions.length ? editions[0].year : null,
      imageId: imageIntent && focusImage ? focusImage.id : null },
  };
}
