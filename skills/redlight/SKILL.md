---
name: redlight
description: Build and validate Redlight (Torogao Jam Program week 5, C# / Unity). Use when working on the Redlight jam entry — its mechanism group, C# / Unity build chain, single-file delivery, and port-back.
---

# Redlight — week 5 jam entry

**Canon core:** A recorded-rite sampler in her studio.
**Cast:** Lux · **Language:** C# / Unity (tiger-style-csharp)

## Mechanism group

- torori reactive voice — stage events (applause, a camera flash, a compliment from a wheel) fire her lines; coverage is the test
- The Lens Toy core mode (attention-driven; praise raises the floor)
- Audience presets (solo record / live)
- The written record — session stats inscribed, the body-writing archive in stub form
- Revelation: exhibitionism Glimpsed → Known across the session arc

## Build

- Delivery: Unity WebGL build packed into the single file (loader + wasm inlined). Named risk: build size; if the packed file breaches itch limits, ship the WebGL build as the jam's downloadable twin and a reduced-core shell as the page build — decided at this week's preflight, never silently. Toolchain home: primo (Unity lives there).
- Planning book: `book/` (mdBook; `mdbook build book`).
- Engine/library pins: book chapter "Engine Research" — read before adding
  any dependency; the from-scratch register (book chapter "Write From
  Scratch") names what must NOT be replaced by a library.

## Gates

1. bloch: start the roadmap item before work; `bloch task done` earns green.
2. Single-file assembly runs offline with all ≥6 screens.
3. Seahawk playtest pass; breakage filed to bloch.
4. Visual gate (D-221) against the Laplace3D quality bar (book chapter).
5. Port-back note + language verdict + §9 filing (book chapter "Port-Back").

## Standing law

D-292 keeper art only · audio silence (metrics only, Owner presses play) ·
18+ surfaces flagged (GJ-07) · restricted lane.
