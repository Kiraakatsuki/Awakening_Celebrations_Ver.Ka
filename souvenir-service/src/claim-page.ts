import { nonce } from "./security";

export function claimPage(): Response {
  const scriptNonce = nonce();
  const markup = `<!doctype html>
<html lang="en">
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>Awakening Celebrations · Digital souvenir</title>
<style nonce="${scriptNonce}">
  body { margin: 2rem auto; padding: 0 1rem; max-width: 36rem; background: #111820; color: #f4eee0;
    font: 1rem/1.6 Georgia, serif; }
  img { display: block; max-width: 100%; height: auto; }
  button { margin-top: 1rem; padding: .65rem 1rem; font: inherit; }
</style>
<main>
<h1 id="title">Digital souvenir</h1>
<p id="status" role="status">Opening your souvenir…</p>
<img id="preview" alt="" hidden>
<button id="download" hidden>Download image</button>
</main>
<script nonce="${scriptNonce}">
  const token = location.hash.slice(1);
  history.replaceState(null, "", "/claim");
  const title = document.getElementById("title");
  const status = document.getElementById("status");
  const preview = document.getElementById("preview");
  const download = document.getElementById("download");
  async function checked(response) {
    if (!response.ok) throw new Error(response.status === 410 ? "This souvenir link has expired or been revoked." :
      "This souvenir is unavailable right now.");
    return response;
  }
  async function resolve() {
    const response = await checked(await fetch("/api/claims/resolve", {
      method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ token }),
    }));
    return response.json();
  }
  async function image(path, grant) {
    const response = await checked(await fetch(path, {
      headers: { "X-Claim-Token": token, Authorization: "Bearer " + grant.authorization },
    }));
    return URL.createObjectURL(await response.blob());
  }
  (async () => {
    const claim = await resolve();
    title.textContent = claim.souvenir.title.en;
    if (!claim.image) throw new Error("This souvenir has no image yet.");
    preview.src = await image("/api/claims/preview", claim.image);
    preview.alt = claim.souvenir.title.en;
    preview.hidden = false;
    status.textContent = "";
    download.hidden = false;
    download.onclick = async () => {
      download.disabled = true;
      try {
        const { download: grant } = await resolve();
        if (!grant) throw new Error("This souvenir has no image yet.");
        const url = await image("/api/claims/download", grant);
        const link = document.createElement("a");
        link.href = url;
        link.download = "souvenir.webp";
        link.click();
        setTimeout(() => URL.revokeObjectURL(url), 1000);
      } catch (error) { status.textContent = error.message; }
      finally { download.disabled = false; }
    };
  })().catch(error => { status.textContent = error.message; });
</script>
</html>`;
  return new Response(markup, { headers: {
    "Content-Type": "text/html; charset=utf-8",
    "Cache-Control": "no-store",
    "Referrer-Policy": "no-referrer",
    "X-Content-Type-Options": "nosniff",
    "Content-Security-Policy": `default-src 'none'; script-src 'nonce-${scriptNonce}'; style-src 'nonce-${scriptNonce}'; img-src blob:; connect-src 'self'; base-uri 'none'; frame-ancestors 'none'; form-action 'none'`,
  } });
}
