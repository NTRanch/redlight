# Redlight

**Nice Thoughts Ranch** · Torogao Jam Program week 5 (Wave B — Sound) · Lux · **C# / Unity week**

A recorded-rite sampler in her studio. The lens watches, attention
decays, and Lux answers every event the room fires — applause, a camera
flash, a compliment from the wheel — from a reactive line set whose
coverage is the test. Praise raises the floor under the session;
revelation runs Glimpsed → Seen → Known; the written record closes
every session, body-writing archive in stub form.

An 18+ game built for the weekly Gameotron jam on itch.io (NTRanch), as
one rung of the 14-week Torogao system-validation program
(D-333/D-334/D-335/D-336). The full planning book lives in
[`book/`](book/) (mdBook, NTRanch theme); this repo's bloch roadmap is
[`.ai/roadmap.md`](.ai/roadmap.md).

## Play

Open `web/index.html` in a browser (one self-contained file, offline).
Confirm 18+ at the gate, pick an audience on START with ←/→, then:

- **Tap / Space** — pose (+attention, on a short cooldown)
- **Enter** — answer a flash before its ring closes (the lens pays)
- **↑/↓** — menus · **P / Esc** — pause · **Backspace** — back

Events fire on the room's schedule: applause feeds attention,
compliments raise the floor (decay never crosses it), and every event
draws a line from Lux — never the same line twice in a row within its
class. End the session above the attention target to go **on record**.

## Levels

1. **Solo Record** — just the lens. Decay 0.30/step, target 420.
2. **Live Small** — a small room. Decay 0.38/step, target 480.
3. **Live Full** — the full room, fastest events. Decay 0.46/step, target 540.

## Previews (rendered from the shipped build, demo mode)

- [Level 1 — Solo Record](docs/previews/level-1-redlight.mp4)
- [Level 2 — Live Small](docs/previews/level-2-redlight.mp4)
- [Level 3 — Live Full](docs/previews/level-3-redlight.mp4)

## Build & verify

The behaviour of record is the native C# core (`core/Redlight.Core`,
plain net8.0, tiger-style-csharp); the shipped page runs its JS
realization, locked to the core by `conformance/vectors.json`
(dumped from the C# core, replayed exactly by the JS).

```sh
just build   # assemble web/index.html
just check   # C# suite (24) + conformance (3/3) + bridges + perf + playtest
just previews
just book
```

Unity WebGL packaging is a primo follow-up (Unity lives there; the
tunnel was down at build time) — see `docs/BREAKAGE-LOG.md` B-1.
Port-back: `docs/PORT-BACK.md`. Playtest record:
`docs/PLAYTEST-2026-10-30.md`.

- **Cast/art law (D-292):** existing keeper art only; staging here is
  abstract shapes, light, and typography by design.
- **Audio:** generated live, only ever answering the player's own
  input; nothing autoplays, and no agent ever plays it aloud.

Status: **v1.0.0** (2026-10-30).
