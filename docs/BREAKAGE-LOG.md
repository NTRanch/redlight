# Breakage log — Redlight

Every breakage found in playtest or build lands here with: what broke,
the evidence, the bloch attempt id, and the fix or the filed finding.
Copied to bloch at the week's close (the log is the input, bloch is the
record).

## B-1 — Unity WebGL packaging deferred (primo leg, OPEN)

Unity lives on primo; the sandbox→primo tunnel was down for the whole
build night (2026-10-30). Per the program contract's engine-week rule,
the shipped artefact is the tested native C# core (core/Redlight.Core,
24 checks green) plus the JS realization of the same core in
web/index.html, locked by conformance vectors dumped from the C#
behaviour (3/3 exact). The Unity project packaging (WebGL build,
single-file inlining, tackle observation of the build) is a primo
follow-up — filed, never faked. R-904-class live entry and the seahawk
playtest also ride the same recovery.

## B-2 — Conformance scenario 2 first dumped an empty session (FIXED)

The first vector dump's scripted scenario (3600 steps at level 2,
3300-step session) overran the session end: the surplus steps bounced
through title and restarted a fresh session, so the recorded final
state showed 0 events — a true but useless vector. Fixed by shortening
the scenario to 3000 steps (inside the first session); the redumped
vector records 9 events, floor 100, 3 lens hits. The JS realization
matched both versions exactly, which is how the restart behaviour was
confirmed identical rather than assumed.

## B-3 — `Core` name collides with the Redlight.Core namespace (FIXED)

In the test/dump projects, unqualified `Core` binds to the
`Redlight.Core` namespace segment instead of the aliased type. Fixed by
dropping the file-scoped namespace in the two console programs so the
`using Core = Redlight.Core.Core;` alias governs. Noted for W12, which
shares this project shape.

## B-4 — Shared text-audit script does not fit this repo's harness (NOTED)

The estate /tmp text-audit script imports a sibling game's web module
shape and fails on this repo (`Cannot find package 'web'`). The audit
was run as an equivalent inline walk over this game's own core (952
text ops, 0 overflows); the law is enforced, the shared script wants a
per-seam adapter — filed as a tooling finding, not a game defect.
