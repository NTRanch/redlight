# C# / Unity — library & tooling research

**Engine / language:** Unity 6.3 LTS (current LTS, supported to December 2027) · C#
**Role in the program:** W5 *Redlight* (Lux — recorded-studio scene; torori reactive voice fired by stage events; Lens Toy attention economy) and W12 *Second Wave* (Prima — Continuation Rig re-expressed in Unity, cross-checking the Rust rig semantics of W4/W11). Unity is also tackle's editor backend, so every finding here doubles as tackle surface knowledge.
**Delivery shape:** Unity Web build packed *toward* a single self-contained `index.html` (loader + `.wasm` + `.data` inlined, base64). **Named seam:** packed size. Unity's own web-optimization guidance puts an empty project's compressed build in the ~12–15 MB range before any game content; a VN with voice and Live2D/Spine rigs will be far above that. If the packed file breaches itch.io's per-file limits, the week's preflight — never the build night — decides the fallback in the program doc: the WebGL build ships as the jam's downloadable twin and the page build is a reduced-core shell.
**Engine licence (2026 state):** the Runtime Fee was cancelled outright on 12 September 2024; Unity is seat-based again. Personal is free under US$200K trailing revenue/funding; Pro is US$2,310/seat/year after the 5% increase of 12 January 2026; Enterprise is mandatory above US$25M. Sources: https://www.mycplus.com/game-development/game-engines/unity-game-engine/ · https://appradar.com/blog/mobile-game-engines-development-platforms

---

## Parts matrix

### (a) 2D rendering & sprite animation — BUILT-IN, sufficient

- **Unity 2D tooling (built-in packages, free):** Sprite Renderer + **Sprite Atlas** (packing; the main texture-memory lever on web), **2D Animation package** (sprite skinning + 2D IK), **PSD Importer** (layered PSD → rigged sprite, the direct bridge from the estate's layered-art pipeline), **2D Tilemap** (not needed for a VN stage, available), **Timeline** for authored scene beats. No third-party purchase is required for frame/sprite-layer animation at jam scale.
- **Verdict:** use built-ins. The character presentation layer for these weeks is skeletal (row b); sprite-sheet animation is the fallback for effects and props.

### (b) Skeletal / puppet animation — TWO REAL ROUTES, both verified current

- **Live2D Cubism 5 SDK for Unity — R6_1, released 2026-10-29** (manual updated 2026-10-29: https://docs.live2d.com/en/cubism-sdk-manual/top/). This is the estate's primary puppet format. Load-bearing version note: **from R5 the Unity SDK renders through URP only** (R4_1 and earlier are the Built-in Render Pipeline line and are frozen out of Cubism 5.3+ model features); motion is managed via Mecanim or the SDK's Playables-based `CubismMotionController`, and R6 changes mask handling (36 masks by default, high-definition mask optional) — comparison table: https://docs.live2d.com/en/cubism-sdk-manual/cubismsdk-compare/. **Licence:** SDK use is free for general users / small-scale enterprises (annual sales under ¥10M); at or above that, a Publication License Agreement is required per title (https://help.live2d.com/en/sdk/sdk_001/; running-royalty plan: https://www.live2d.com/en/sdk/license/running_plan01/). The Cubism *Editor* is a separate freemium licence. Maturity: vendor-supported, actively released (R6 2026-10-20, R6_1 2026-10-29).
- **Spine (Esoteric Software) — spine-unity runtime.** The alternative rig format; official runtime, source-available under the Spine Runtimes licence (runtime use requires a Spine editor licence). Editor pricing is one-time: Essential US$69 (list $99), Professional US$299–379 depending on sale/list; businesses over US$500K annual revenue must hold Enterprise (annual). Price survey (checked against vendor listings, September 2026): https://blog.makko.ai/sprite-sheet-tools/. Maturity: the industry default for 2D skeletal in Unity; strongest if a rig must be shared with the Godot week (same vendor sells spine-godot, see the Godot chapter).
- **Custom / NONE:** neither package is needed for the *face-state* layer itself — see the from-scratch register; both are parameter-driven, which is exactly what the face system needs (Cubism parameters / Spine bones+skins+animation state).

### (c) Audio + reactive / voice-event systems — BUILT-IN FOR JAM, MIDDLEWARE EXISTS

- **Built-in (free):** `AudioSource`/`AudioListener` + **Audio Mixer** (groups, snapshots, ducking via side-chain-style send levels) covers a one-scene voice-reactive build: stage events call clips on mixer groups; snapshot transitions fake "room tone changes." No random-access latency problem at this scale if clips are loaded `AudioClipLoadType.CompressedInMemory` / streamed deliberately.
- **FMOD for Unity (FMOD Studio, vendor integration):** event/parameter model is the natural home for a *coverage* system — one `applause` event with intensity parameter, voice lines as programmer instruments keyed by line ID. FMOD's indie licence is free under its revenue threshold (historically US$200K budget/revenue class terms — re-check the current licence before the full game, not the jam). Maturity: industry standard, Unity integration maintained by FMOD.
- **Wwise Unity Integration (Audiokinetic):** equally capable (RTPCs ≈ FMOD parameters); **free Indie licence for projects with total production budget under US$250K, no asset limit** (https://blog.audiokinetic.com/free-wwise-indie-license/). Heavier authoring toolchain than FMOD for a one-week build.
- **Verdict:** jam weeks use built-in audio + a custom event bus (see register); the full game evaluates FMOD first (its parameter model maps 1:1 onto the reactive-voice design, and the same vendor has a maintained Godot GDExtension, which keeps one audio authoring tool across engines).

### (d) Dialogue / VN frameworks — YARN SPINNER (primary), NANINOVEL (purpose-built), INK (narrative logic), FUNGUS (legacy)

- **Yarn Spinner for Unity — stable 2.4.x; v3 in beta (2026).** MIT-licensed, free for commercial use (paid Asset Store/itch copies are support-the-dev channels). Core changelog shows 3.2.2 (2026-08-07); the Unity v3 beta requires Unity 2022.3+ and moves dialogue views to async/await (https://www.yarnspinner.dev/blog/yarn-spinner-3-beta-1; core: https://raw.githubusercontent.com/YarnSpinnerTool/YarnSpinner/main/README.md). Screenplay-like syntax writers can own; commands map cleanly onto stage events (`<<applause>>`, `<<lens praise 0.2>>`). **Pick for W5/W12:** pin 2.4.x for the jam (stable), evaluate v3 for the full game.
- **Naninovel — v1.21 (Q1 2026), commercial.** The purpose-built VN engine for Unity: script language, rollback, auto/skip, backlog, layered character staging, save slots — the whole VN furniture set, supported on Unity 6.0 and 6.3 LTS specifically (https://naninovel.com/releases/1.21). Paid licence per seat via Asset Store/GitHub distribution. **Pick if a Unity week were a full VN**; over-scope for a one-scene jam entry, and its character model would fight the estate's rig-first staging.
- **Ink (inkle) — ink-unity-integration, MIT.** Superb branching-narrative *logic* (knots, variables, external functions), no presentation layer at all (https://raw.githubusercontent.com/inkle/ink/master/README.md). Right tool if the writing outgrows Yarn; wrong tool as the only dialogue system for a staged scene.
- **Fungus (snozbot/fungus) — free/open-source, effectively legacy.** Visual flowchart VN toolkit; still downloadable, but release cadence has been slow for years and its bundled TextMeshPro-era internals are a liability on Unity 6. **Do not start new work on it**; listed because older VN tutorials still recommend it.
- **Custom layer regardless of pick:** none of these knows what a *stage event* is in torori terms — the dialogue runner emits estate events; the voice system consumes them (register item 6).

### (e) State machines & save systems — CUSTOM CORE, engine furniture around it

- **State machines:** Animator/Mecanim state machines are for *animation*, and abusing them as game logic is the classic Unity VN bug farm. For scene/flow state, use a small custom C# state machine (the estate's arousal/face cores are pure C# classes with explicit transition tables — comptime-table envy from the Zig week is satisfied by `static readonly` tables + source-generated validation, not by a package). Unity's own **State Machine Behaviours** and **PlayableBehaviour** are presentation hooks only. No third-party FSM package earns its place at this scope.
- **Saves:** Unity has **no built-in save system** beyond `PlayerPrefs` (which is settings storage, not saves — and on WebGL it lands in IndexedDB with size limits). The jam/full-game answer is custom: serialise the pure-data core (arousal state, ledger, face state, script position) to JSON (`JsonUtility` for flat data, or Newtonsoft.Json — Unity ships it as a package dependency — for nested data) into `Application.persistentDataPath` (IndexedDB-backed on web). **Easy Save 3** (Moodkie, paid Asset Store) is the credible buy-it alternative — encryption, cloud hooks — but it would own a format the estate wants to own itself, since the same save schema must round-trip through the Rust/Godot implementations. **Verdict: custom, deliberately.**

### (f) UI — UI TOOLKIT for menus, uGUI/TMP for in-scene

- **UI Toolkit** (built-in): USS/UXML styling is the closest Unity gets to the estate's CSS-shaped thinking; right choice for menus, settings, the 18+ gate screen, and the jam's How-to-Play/Settings screens.
- **uGUI + TextMeshPro** (built-in): still the pragmatic choice for in-scene dialogue boxes and name plates (TMP's SDF text is the readability floor for VN text at phone sizes). Note TMP adds ~0.7 MB to a web build (measured in https://docs.crazygames.com/resources/optimization-tips/) — acceptable, budgeted.
- **Verdict:** hybrid, both built-in; no UI package purchase is justified.

### (g) Asset pipeline & build-size reduction for web — THE WEEK'S REAL BOSS

Unity's official web optimisation manual (https://docs.unity.com/en-us/engine/6000.6/manual/platform-specific/webgl/building-distribution/web-optimization-mobile) names the levers; in estate priority order:

1. **Brotli compression** (Player Settings → Web → Compression Format). Beats gzip; requires HTTPS serving (itch.io qualifies). Long build times are the cost.
2. **Code optimisation = Disk Size with LTO** + managed stripping level as high as the build tolerates; .NET Standard API compatibility profile.
3. **Addressables** — defer everything not in the first scene; group tightly (fewer bundles = smaller total), compress bundles. For the *single-file* jam shape Addressables are in tension with the goal (they exist to split delivery); the jam build therefore inlines a *reduced* Addressables catalogue or none, and the full game uses them properly with remote catalogues.
4. **Texture discipline:** sprite atlases, max-size caps per asset class, Crunch/ASTC-where-supported; voice audio Vorbis at VN-appropriate quality (voice ≈ 64–96 kbps mono is generous); no uncompressed anything.
5. **URP tax:** Cubism R5+ forces URP (§b), and URP adds ~3.6 MB to a web build (CrazyGames measurement above). That is a *known, accepted* cost of current Cubism — the alternative is freezing on the BiRP-era R4_1 SDK and losing Cubism 5.3+ models. Pin the choice at preflight; do not discover it at build time.
6. **Single-file packing itself** is custom tooling (register/preflight): a post-build script that base64-inlines `.wasm`/`.data`/framework into the loader HTML adds ~33% on top of the compressed size — measure the packed file, not the build folder.

### (h) Testing / CI — UNITY TEST FRAMEWORK + GAMECI

- **Unity Test Framework** (built-in package): NUnit-based EditMode tests are where the pure C# cores (arousal engine, face-state evaluator, ledger) get their golden-trace tests — these run without a scene and are the Unity half of the cross-engine conformance story. PlayMode tests cover staging integration.
- **GameCI** (community, open source): GitHub Actions runners for Unity builds/tests, including Web targets; the standard way to get a Unity week into CI without a self-hosted editor machine. (Unity licences in CI need the GameCI activation flow — a known setup cost, paid once.)
- **Verdict:** UTF EditMode suite on the data cores is non-negotiable; it is what makes "Continuation Rig semantics in Unity" a *checkable* claim instead of a vibe (register item 5).

---

## Write-from-scratch register

Nothing below exists as a package in any catalogue. Each item names why, and the engine feature it rides on.

1. **Face-state system (F-codes: expression / gaze / blush as data).** *Why from scratch:* F-codes are estate canon — a scored, measured vocabulary (§1.7 full-disc gaze definition, D-4 channel measurements). No vendor sells "F-03 at edge." *Rides on:* Cubism parameter writes (per-frame `CubismParameter` values) or Spine bone/skin control, driven by a pure C# evaluator that takes (face-state, time) → parameter set. The evaluator is engine-agnostic data + math; only the last-mile parameter writer is Unity-specific. Blush is a parameter/opacity channel, gaze is two bounded axes — both are exactly what Cubism parameters already model.
2. **Arousal engine (exactly-once culmination).** *Why:* the engine's contract (tiers T1–T4 at canon durations, exactly-once culmination, F-03/F-05/F-10 coupling) is a Torogao design object, not a genre commodity. *Rides on:* nothing but a fixed-step C# core updated from a single driver; Unity frame time never enters the math (accumulator pattern), which is also what makes the W6 Godot comparison meaningful.
3. **Sex-scene staging / two-body contact.** *Why:* contact, occlusion and guided-hand staging are the pipeline's open research problem (W4 is the Rust stress test); no package models two rigs in sustained believable contact. *Rides on:* rig layering/sort order, per-rig parameter control, Timeline for authored beats, and (Unity-specific) render-texture masks when occlusion cheats are needed.
4. **Lactation / fluid sim.** *Why:* stateful anatomy + fluid timeline is estate simulation content (the 8473238 specificity floor); there is no Asset Store "fluid timeline tied to an arousal engine." (W10 authors the kernel in Mojo; the Unity relevance is as a *consumer*: a table/curve-driven presentation layer.) *Rides on:* animation curves + parameter writes for presentation; the sim itself is data.
5. **Continuation Rig semantics (W12's actual subject).** *Why:* the Rig is a Torogao signature tool with estate semantics — three-phase model, continuation past peak at a reduced curve, per-channel presets, and *persistence*: the second scene opens with the Rig remembering the first. Re-expressing it in C#/Unity is a semantics port, and the test is whether the *numbers* survive. *Rides on:* the pure C# core pattern (item 2) + the save system (§e) for cross-scene memory + UTF golden traces generated from the Rust implementation's outputs.
6. **Reactive voice coverage system (W5's actual subject).** *Why:* torori's design question — does the line set answer stage events (applause, flash, compliment) without repeating itself to death — is a coverage/selection system: event → eligible line set → no-repeat-within-window selection → priority/interrupt rules. Yarn/Ink deliver lines; FMOD/Wwise play them; *coverage logic is neither's job.* *Rides on:* a custom C# event bus fed by dialogue commands and scene triggers, with FMOD parameters (or mixer snapshots in the jam build) as the playback back end. Its sizing findings (how many lines per event class before repetition shows) are the week's port-back deliverable.
7. **Ledgers (written record, book of record, appraisal evidence).** *Why:* estate bookkeeping with canon shapes (entries named, witnessed, append-only) — a data design, not a feature. *Rides on:* the custom save serialisation (§e); UI Toolkit lists for presentation.

---

## Quality bar (Laplace3D-class minimum)

The bar is measured against Laplace3D-class Danbooru work, per the estate's animation doctrine — the same numbers in every engine chapter, so a week cannot pass by being "good for Godot" or "good for a jam":

- **60 fps presentation** on a mid-range laptop browser for the web builds; frame budget is spent on the rigs, not on UI effects.
- **Motion on every frame** — no frozen holds outside authored stillness; idle is a system (breath, blink, look targets), never a static sprite.
- **Secondary-motion lag ≤ 17 ms** behind primary motion (one frame at 60 fps): hair, clothing, and prop follow-through must read as attached, not chasing.
- **Humanised jitter** — micro-variation on holds and loops (seeded, bounded); mechanically perfect loops read as fake at Laplace3D's level.
- **Expression readability at delivery size** — F-code states must be distinguishable at the size the itch page actually renders, not only at editor zoom. This is a screenshot-gate item (D-221 discipline), not a numeric one.
- **Seamless loops** — loop points verified by frame-diff at the seam, not by eye from the start of the clip.
- **Audio sync ≈ +12 ms ± 25 ms** for voice/reaction hits against their visual events; outside that window the reaction reads as a mistake, not a delay.
- **Deterministic evaluation where the engine allows** — and Unity's allowance is narrow, stated honestly: Unity does not guarantee cross-platform floating-point or physics determinism, and WebGL adds its own variance. The estate answer is architectural, not hopeful: all *scored* systems (arousal, face, rig, ledger) run in pure C# fixed-step cores with seeded RNG, where determinism is achievable and test-locked; presentation (particles, physics garnish) is explicitly non-scored and may vary.

## Adult-content policy notes

- **Unity itself** imposes no content ban on games you distribute yourself; the constraints arrive from storefronts and from Unity's *services* (ads/ monetisation policies prohibit adult content — irrelevant here; do not wire Unity Ads or Unity Gaming Services into an adult title).
- **itch.io (the actual storefront):** adult content is allowed when fictional/illustrated, legal, and correctly flagged with the "Has sensitive content" metadata; sexualised images of *real* humans are prohibited, as is any sexual depiction of minors or minor-presenting characters (itch.io creator FAQ, quoted in enforcement notices: https://itch.io/t/5119952/stop-with-the-radio-silence-administrators-we-need-some-kind-of-communication-on-the-nsfw-ban). Two operational facts for the slate: cover images are expected to be SFW even for adult pages, and since the 2025 payment-processor wave, **paid** adult games have faced de-indexing while free adult games remain listed (community status discussion: https://itch.io/t/7081450/am-i-allowed-to-promote-18nsfw-content-in-the-release-announcements-boardforum) — jam entries are free, so the program is on the safe side of that line; a future paid Unity title needs a fresh policy check at submission time, per the estate's checklist workflow.
- **WebGL-specific:** no age-gate or content technology is provided by the engine; the 18+ gate screen is the game's own first scene (GJ-07), built in UI Toolkit.

## Risks / seams

1. **Single-file size (the named seam).** URP tax (~3.6 MB) + Cubism runtime + voice audio + base64 inflation (~+33%) stack multiplicatively against itch per-file limits. Preflight measures a packed spike build with one rig and one minute of voice *before* the week's design is finalised.
2. **Cubism's URP migration is recent** (R5 beta→R6 within 2026): draw-order control changed (custom render pass; `sortingOrder` no longer governs Cubism drawables the BiRP way). Any estate tooling written against R4_1 behaviour must be re-verified, and the version is pinned per project — mid-week SDK upgrades are forbidden.
3. **Licensing drift is a demonstrated Unity behaviour,** not a hypothetical: the 2023 Runtime Fee was announced, applied retroactively in its first form, and cancelled in 2024. The estate mitigation is the same as everywhere: scored systems live in engine-agnostic data cores, so an engine exit costs the presentation layer, not the game.
4. **Yarn Spinner v3 is beta.** Pin 2.4.x for jam weeks; a v3 migration is a full-game decision with its own gate.
5. **WebGL platform limits:** no native threads story comparable to desktop, IndexedDB persistence is browser-evictable (saves are best-effort on web — say so on the settings screen), and audio unlock requires a user gesture (the 18+ gate click doubles as the unlock — design it that way).
6. **tackle note:** tackle's Unity backend observes these builds; every seam above (URP draw order, Addressables layout, Web build pipeline) is a seam tackle will meet in *other people's* projects too — file them as tackle findings, not just jam notes.


## How the research applies to Redlight

- Language: **C# / Unity** under tiger-style-csharp.
- Canon core: A recorded-rite sampler in her studio.
- The parts matrix in the dossier names, for every part of the full game
  (rendering, animation, audio, dialogue/UI, state, persistence, assets,
  build), the library or tool this entry pins — or states plainly that the
  part is written from scratch (see the next chapter).
