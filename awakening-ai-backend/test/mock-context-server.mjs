import { readFileSync } from 'node:fs';
import { createServer } from 'node:https';
import { createApp } from '../server.js';

const [certPath, keyPath, port = '8743'] = process.argv.slice(2);
if (!certPath || !keyPath) {
  console.error('Usage: node test/mock-context-server.mjs <local certificate PEM> <local private key PEM> [port]');
  process.exit(1);
}

const app = createApp({
  hasProvider: () => true,
  respond: ({ instructions, input }) => {
    if (!instructions) return 'Local legacy transport fixture.';
    const prompt = JSON.parse(input);
    const evidence = prompt.evidence[0];
    return JSON.stringify({
      reply: prompt.locale === 'zh-TW' ? '本機測試回覆。' : 'Local contextual transport fixture.',
      evidenceKeys: [evidence.key],
    });
  },
});
createServer({ cert: readFileSync(certPath), key: readFileSync(keyPath) }, app)
  .listen(Number(port), '127.0.0.1', () => console.log('Fake provider listening on HTTPS loopback port ' + port));
