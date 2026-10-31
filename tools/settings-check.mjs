// settings-check.mjs — gates R-202/R-203/R-403: settings + archive
// bridges round-trip, hostile values clamp, and settings are reachable
// from boot with no progression.
import { createGame, INPUT } from "../web/game.mjs";

const game = await createGame(null, { seed: 7, level: 0, demo: false });
function steps(n, bits = 0) { for (let i = 0; i < n; i++) game.step(bits); }
function fail(msg) { console.error(`settings-check: ${msg}`); process.exit(1); }

if (game.screen() !== 0) fail("boot screen is not the 18+ gate");
game.applySettings(true, false);
if (game.settingsBits() !== 1) fail("pace bit did not round-trip");
game.applySettings(false, true);
if (game.settingsBits() !== 2) fail("text bit did not round-trip");
game.applySettings(true, true);
game.applyArchive(2, 1);
if (game.archiveSessions() !== 2) fail("archive sessions did not round-trip");
if (game.archiveBestStage() !== 1) fail("archive best stage did not round-trip");
game.applyArchive(5000, 99);
if (game.archiveSessions() !== 999 || game.archiveBestStage() !== 2) fail("archive did not clamp");

steps(1, INPUT.confirm);
if (game.screen() !== 1) fail("confirm did not leave the gate");
steps(1, INPUT.down); steps(1, INPUT.down);
steps(1, INPUT.confirm);
if (game.screen() !== 5) fail(`settings not reachable from title (screen=${game.screen()})`);
steps(2);
if (!game.frameText().includes("QUICK")) fail("settings screen does not show restored QUICK pace");
if (!game.frameText().includes("LARGE")) fail("settings screen does not show restored LARGE text");
console.log("settings-check: bridges round-trip + clamp; settings reachable ungated; restored values render");
