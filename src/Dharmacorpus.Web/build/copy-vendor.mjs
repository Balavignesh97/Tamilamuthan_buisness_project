// Copies third-party browser files from node_modules into wwwroot so the site
// self-hosts everything (no CDN calls, works with a strict Content-Security-Policy).
import { copyFileSync, mkdirSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const root = join(dirname(fileURLToPath(import.meta.url)), "..");
const nm = join(root, "node_modules");
const www = join(root, "wwwroot");

const files = [
  ["jquery/dist/jquery.min.js", "lib/jquery/jquery.min.js"],
  ["flowbite/dist/flowbite.min.js", "lib/flowbite/flowbite.min.js"],
  ["@fontsource/playfair-display/files/playfair-display-latin-600-normal.woff2", "fonts/playfair-display-latin-600-normal.woff2"],
  ["@fontsource/playfair-display/files/playfair-display-latin-700-normal.woff2", "fonts/playfair-display-latin-700-normal.woff2"],
  ["@fontsource/lato/files/lato-latin-400-normal.woff2", "fonts/lato-latin-400-normal.woff2"],
  ["@fontsource/lato/files/lato-latin-700-normal.woff2", "fonts/lato-latin-700-normal.woff2"],
];

for (const [from, to] of files) {
  const dest = join(www, to);
  mkdirSync(dirname(dest), { recursive: true });
  copyFileSync(join(nm, from), dest);
  console.log(`copied ${to}`);
}
