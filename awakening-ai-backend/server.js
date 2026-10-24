import express from 'express';
import helmet from 'helmet';
import { rateLimit } from 'express-rate-limit';
import OpenAI from 'openai';

const app = express();
const port = Number.parseInt(process.env.PORT ?? '3000', 10);
const model = process.env.OPENAI_MODEL || 'gpt-4.1-mini';
const maxMessageLength = 4000;

app.disable('x-powered-by');
// Render terminates TLS at one trusted reverse proxy and forwards the client IP.
app.set('trust proxy', 1);
app.use(helmet());
app.use(express.json({ limit: '16kb', strict: true }));

const chatLimiter = rateLimit({
  windowMs: 60_000,
  limit: 20,
  standardHeaders: 'draft-8',
  legacyHeaders: false,
  message: { error: 'Too many requests. Please try again shortly.' },
});

app.get('/health', (_request, response) => {
  response.json({ status: 'ok' });
});

app.post('/chat', chatLimiter, async (request, response, next) => {
  const { message } = request.body ?? {};
  if (typeof message !== 'string') {
    return response.status(400).json({ error: 'Message must be a string.' });
  }

  const normalizedMessage = message.trim();
  if (!normalizedMessage) {
    return response.status(400).json({ error: 'Message cannot be empty.' });
  }
  if (normalizedMessage.length > maxMessageLength) {
    return response.status(400).json({ error: `Message must be ${maxMessageLength} characters or fewer.` });
  }
  if (!process.env.OPENAI_API_KEY) {
    return response.status(503).json({ error: 'Chat service is not configured.' });
  }

  try {
    const client = new OpenAI({
      apiKey: process.env.OPENAI_API_KEY,
      timeout: 30_000,
      maxRetries: 1,
    });
    const result = await client.responses.create({ model, input: normalizedMessage });
    if (typeof result.output_text !== 'string' || !result.output_text.trim()) {
      console.warn('OpenAI returned no text output.');
      return response.status(502).json({ error: 'The assistant returned no reply. Please try again.' });
    }
    return response.json({ reply: result.output_text.trim() });
  } catch (error) {
    // Keep diagnostic details server-side without recording user messages or credentials.
    console.error('OpenAI request failed:', error?.name ?? 'UnknownError', error?.status ?? 'no-status');
    if (error?.name === 'APIConnectionTimeoutError' || error?.name === 'TimeoutError') {
      return response.status(504).json({ error: 'The assistant request timed out. Please try again.' });
    }
    if (error?.status === 400 || error?.status === 404 || error?.status === 422) {
      return response.status(502).json({ error: 'The assistant request could not be completed.' });
    }
    return response.status(502).json({ error: 'The assistant service is temporarily unavailable.' });
  }
});

// Express body parser errors (including invalid JSON and oversized requests) need a safe JSON response.
app.use((error, _request, response, _next) => {
  if (error?.type === 'entity.too.large') {
    return response.status(413).json({ error: 'Request body is too large.' });
  }
  if (error instanceof SyntaxError && 'body' in error) {
    return response.status(400).json({ error: 'Request body must be valid JSON.' });
  }
  console.error('Unexpected request error:', error?.name ?? 'UnknownError');
  return response.status(500).json({ error: 'An unexpected server error occurred.' });
});

app.listen(port, '0.0.0.0', () => {
  console.log(`Awakening AI backend listening on 0.0.0.0:${port}`);
});
