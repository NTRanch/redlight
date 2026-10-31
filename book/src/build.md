# Build & Delivery — Redlight

**Language:** C# / Unity (tiger-style-csharp)

**Delivery shape:** Unity WebGL build packed into the single file (loader + wasm inlined). Named risk: build size; if the packed file breaches itch limits, ship the WebGL build as the jam's downloadable twin and a reduced-core shell as the page build — decided at this week's preflight, never silently. Toolchain home: primo (Unity lives there).

**Jam rule:** one self-contained `index.html`, ≥6 screens (Title / Pause /
How to Play / Gameplay / Settings / Game Over-or-Victory), documented
process (GAMEOTRON orchestrates; the language build chain is part of the
documentation).

**Book:** `mdbook build book` renders this planning book to `book/html/`
with the NTRanch theme (dark background, hot pink).
