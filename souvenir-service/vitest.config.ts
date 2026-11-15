import { cloudflareTest, readD1Migrations } from "@cloudflare/vitest-plugin";
import { defineConfig } from "vitest/config";

export default defineConfig({
  plugins: [
    cloudflareTest(async () => ({
      wrangler: { configPath: "./wrangler.jsonc" },
      miniflare: {
        bindings: {
          LOCAL_TEST_AUTH: "enabled",
          LOCAL_KIOSK_KEY: "synthetic-local-kiosk-test-key-0001",
          TOKEN_KEY: "synthetic-token-derivation-test-key-0001",
          TEST_MIGRATIONS: await readD1Migrations("./migrations"),
        },
      },
    })),
  ],
});
