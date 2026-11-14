# Stage 5.2 — Context mapping and continuity

Unity still uses its ordinal bindings and existing route strings. The new protocol adapter serializes canonical Stage 5.1 IDs without replacing those navigation keys.

| Existing Unity performer | Canonical ID | Common chooser portrait |
| --- | --- | --- |
| GAIA | `JFC26-GAIA` | `JFC26-GAIA-IMG01` |
| PANDEMIC | `JFC26-PANDEMIC` | `JFC26-PANDEMIC-IMG01` |
| RE-BIRTH | `JFC26-REBIRTH` | `JFC26-REBIRTH-IMG01` |
| WHITE-GUARDIAN | `JFC26-WGUARDIAN` | `JFC26-WGUARDIAN-IMG01` |
| CONSCIENCE | `JFC26-CONSCIENCE` | `JFC26-CONSCIENCE-IMG01` |
| SANCTUARY | `JFC26-SANCTUARY` | `JFC26-SANCTUARY-IMG01` |

Dedicated full-screen assistant and reflection routes pass the current performer with `source=performer-page`. The six existing common chooser buttons pass that portrait and parent with `source=common-ai`. A development-only **Ask about JFC** action opens the same chat layout without selected IDs. The general chat uses the neutral accent and the existing composer. No image data is uploaded. Noise and Roots remain valid festival subjects without Unity journeys. A question that names a different defile takes retrieval priority.

## Session boundary

- The server issues 256-bit random bearer tokens, encodes them as 43 base64url characters and stores their SHA-256 digest as the lookup key. Client-chosen tokens cannot create sessions.
- The store holds up to 256 sessions with a fixed 15-minute lifetime, swept on issuance and lookup. At capacity it declines new allocations until entries expire or reset. It stores four recent question excerpts of at most 512 UTF-16 units each, plus small canonical subject state. Model replies are not kept in this store. The retained text limit is 524,288 UTF-16 units across a full store, excluding object/metadata overhead.
- One request at a time can update a session. Reset deletes state and signals cancellation to the provider adapter. Expired or reset results do not commit continuity. A disconnected request signals cancellation too. A process restart loses state and the client can retry after HTTP 410. There is no durable transcript storage, server transcript-reading endpoint, or token in a URL.
- A locale change retains the same token and subject while the selection scope is unchanged. Scope changes discard the previous question window for the next successful turn. An explicit defile reference also takes precedence within the current scope. Ambiguous multi-defile follow-ups ask for clarification.
- Unity keeps the token only in memory, clears it at the existing Home visitor-reset boundary and controller disable, and sends a best-effort scoped reset. The server TTL is the backstop when shutdown or a transport failure prevents delivery. This experiment retains same-scope server continuity across overlay rebuilds while the current visit stays active. The existing visual transcript lifecycle is unchanged. Existing coroutine/sequence checks reject responses after navigation, cancellation, locale change, or overlay replacement.

Selection IDs resolve only public exhibition knowledge. They convey no identity assertion, media download capability or souvenir permission.
