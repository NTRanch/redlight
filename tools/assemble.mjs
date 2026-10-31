// assemble.mjs — build the single-file jam entry web/index.html from
// web/game.mjs (the JS realization) + web/shell.html.
import { readFileSync, writeFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const root = join(dirname(fileURLToPath(import.meta.url)), "..");
const gameMjs = readFileSync(join(root, "web/game.mjs"), "utf8").replace(/^export /gm, "");
const shell = readFileSync(join(root, "web/shell.html"), "utf8");
const out = shell.replace("/*__GAME_MJS__*/", () => gameMjs);
if (out === shell) throw new Error("shell placeholder not found — nothing assembled");
writeFileSync(join(root, "web/index.html"), out);
console.log(`assembled web/index.html (${out.length} bytes, js core ${gameMjs.length} chars)`);
