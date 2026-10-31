# Changelog — Redlight

All notable changes, newest first. Format: Keep a Changelog.

## [1.0.0] — 2026-10-30

- The full mechanism group, playable in the single-file `web/index.html`:
  torori reactive voice (3 event classes × 6 lines, no-immediate-repeat,
  coverage counted per class), the Lens Toy attention economy (decay per
  audience size, praise-raised floor decay cannot cross, timed flash
  answers), audience presets as levels (Solo Record / Live Small /
  Live Full), the written record closing every session with the
  body-writing archive in stub form, and the Glimpsed → Seen → Known
  revelation arc.
- Native C# core (`core/Redlight.Core`, net8.0, tiger-style-csharp) as
  the behaviour of record: deterministic fixed-step, draw-list seam,
  settings + written-record bridges. Console test suite: 24 checks
  (13 validation / 11 adversarial).
- JS realization of the same core shipped in the page, locked by
  conformance vectors dumped from the C# core (3/3 scenarios exact).
- 8 screens (18+ gate, title, how-to, play, pause, settings, victory,
  game over), generated WebAudio answering input only, demo mode
  (`?demo=<level>&seed=<n>`) closing all 3 levels won.
- Preview videos per level in `docs/previews/`, rendered from the
  shipped build's demo mode.
- Port-back filed (`docs/PORT-BACK.md`): reaction-set sizing law,
  attention floor mechanic vs Exposure's slot economy, C#/Unity
  deferred-packaging verdict.
- Unity WebGL packaging deferred to primo (tunnel down at build time),
  breakage-logged (B-1), never faked.

## [Unreleased]

- Repo scaffolded 2026-10-30: bloch roadmap (kind=game, lane=restricted),
  planning, per-game skill, baseline C# / Unity config, NTRanch-themed mdBook,
  brand-asset slots.
