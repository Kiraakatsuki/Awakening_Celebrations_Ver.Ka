const encoder = new TextEncoder();
const tokenPattern = /^[A-Za-z0-9_-]{43}$/;

function base64url(bytes: Uint8Array): string {
  return btoa(String.fromCharCode(...bytes)).replaceAll("+", "-").replaceAll("/", "_").replace(/=+$/, "");
}

export function nonce(): string {
  return base64url(crypto.getRandomValues(new Uint8Array(32)));
}

export async function digest(value: string): Promise<string> {
  const bytes = new Uint8Array(await crypto.subtle.digest("SHA-256", encoder.encode(value)));
  return Array.from(bytes, byte => byte.toString(16).padStart(2, "0")).join("");
}

export async function derive(key: string, purpose: "visit" | "qr" | "image", value: string): Promise<string> {
  const signingKey = await crypto.subtle.importKey(
    "raw", encoder.encode(key), { name: "HMAC", hash: "SHA-256" }, false, ["sign"],
  );
  const bytes = await crypto.subtle.sign("HMAC", signingKey, encoder.encode(`souvenir/v1/${purpose}:${value}`));
  return base64url(new Uint8Array(bytes));
}

export function isToken(value: string): boolean {
  return tokenPattern.test(value);
}

export function matches(provided: string, expected: string): boolean {
  const left = encoder.encode(provided);
  const right = encoder.encode(expected);
  return left.byteLength === right.byteLength && crypto.subtle.timingSafeEqual(left, right);
}

export function randomIndex(size: number): number {
  if (!Number.isSafeInteger(size) || size < 1 || size > 2 ** 32) throw new RangeError("Invalid selection size");
  const accepted = 2 ** 32 - (2 ** 32 % size);
  const sample = new Uint32Array(1);
  do { crypto.getRandomValues(sample); } while (sample[0] >= accepted);
  return sample[0] % size;
}
