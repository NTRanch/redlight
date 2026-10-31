// playtest.mjs — the demo policy plays every level from a fresh start
// against the shipped JS realization and must close it WON first try.
import { createGame } from "../web/game.mjs";

const SCREENS = ["gate", "title", "howto", "play", "pause", "settings", "victory", "gameover"];
let failures = 0;
for (let level = 0; level < 3; level++) {
  const game = await createGame(null, { seed: 9000 + level, level, demo: true });
  game.startLevel(level);
  let steps = 0;
  while (game.screen() !== 6 && game.screen() !== 7 && steps < 30000) {
    game.step(0);
    steps++;
  }
  const won = game.screen() === 6;
  console.log(`level ${level + 1}: screen=${SCREENS[game.screen()]} steps=${steps} lines=${game.distinctCount()} stage=${game.knownStage()} ${won ? "VICTORY" : "FAIL"}`);
  if (!won) failures++;
}
if (failures) { console.error(`playtest: ${failures} level(s) not closed won`); process.exit(1); }
console.log("playtest: all 3 levels closed won, first attempt");
