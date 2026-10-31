// conformance.mjs — replay conformance/vectors.json (dumped FROM the
// C# core, the behaviour of record) against the JS realization. Exact:
// final state fields and the FNV-1a checksum over snapshots + the final
// draw list must match bit for bit.
import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";
import { createCore, INPUT } from "../web/game.mjs";

const root = join(dirname(fileURLToPath(import.meta.url)), "..");
const vectors = JSON.parse(readFileSync(join(root, "conformance/vectors.json"), "utf8"));

function scriptBits(mode, step) {
  if (mode === 1) {
    if (step === 0 || step === 1) return INPUT.confirm;
    let bits = 0;
    if (step % 37 === 0) bits |= INPUT.tap;
    if (step % 91 === 0) bits |= INPUT.confirm;
    return bits;
  }
  return 0;
}

function fnv1a(text, hash) {
  let h = hash >>> 0;
  for (let i = 0; i < text.length; i++) {
    h ^= text.charCodeAt(i) & 0xFF;
    h = Math.imul(h, 16777619) >>> 0;
  }
  return h >>> 0;
}

let failures = 0;
for (const sc of vectors.scenarios) {
  const core = createCore(sc.seed, sc.level, sc.demo);
  core.startLevel(sc.level);
  let hash = 2166136261 >>> 0;
  for (let i = 0; i < sc.steps; i++) {
    core.step(scriptBits(sc.mode, i));
    if (i % 100 === 0) {
      const snap = `${i};${core.screen()};${core.attention()};${core.floor()};` +
        `${core.eventsFired()};${core.distinctCount()};${core.lastLineEncoded()};` +
        `${core.lensHits()};${core.knownStage()};${core.elapsed()}\n`;
      hash = fnv1a(snap, hash);
    }
  }
  hash = fnv1a(core.draw(), hash);
  const got = {
    screen: core.screen(), attention: core.attention(), floor: core.floor(),
    events: core.eventsFired(), distinct: core.distinctCount(),
    lensHits: core.lensHits(), stage: core.knownStage(), checksum: hash,
  };
  const want = {
    screen: sc.screen, attention: sc.attention, floor: sc.floor,
    events: sc.events, distinct: sc.distinct, lensHits: sc.lensHits,
    stage: sc.stage, checksum: sc.checksum,
  };
  const ok = JSON.stringify(got) === JSON.stringify(want);
  console.log(`${sc.name}: ${ok ? "MATCH" : "MISMATCH"} got=${JSON.stringify(got)}${ok ? "" : " want=" + JSON.stringify(want)}`);
  if (!ok) failures++;
}
if (failures) { console.error(`conformance: ${failures} scenario(s) diverged`); process.exit(1); }
console.log(`conformance: ${vectors.scenarios.length}/${vectors.scenarios.length} scenarios match the C# core exactly`);
