// Генерация прозрачных погодных слоёв для панорамы 2172 × 724.
// Запуск: node art/panoramas/weather-overlay-generator.js
const fs = require('node:fs');
const path = require('node:path');

const width = 2172;
const height = 724;
const out = path.join(__dirname, 'weather');
fs.mkdirSync(out, { recursive: true });
const svg = body => `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 ${width} ${height}" role="img">${body}</svg>`;
let seed = 2105;
function rand() { seed = (seed * 1664525 + 1013904223) >>> 0; return seed / 4294967296; }
function reset(n) { seed = n; }
function lines(count, color, opacity, dx, dy, stroke) {
  let s = `<g stroke="${color}" stroke-opacity="${opacity}" stroke-width="${stroke}" stroke-linecap="round">`;
  for (let i = 0; i < count; i++) {
    const x = Math.round(rand() * width);
    const y = Math.round(rand() * height);
    s += `<path d="M${x} ${y} l${dx} ${dy}"/>`;
  }
  return s + '</g>';
}
function dots(count, color, opacity, minR, maxR) {
  let s = `<g fill="${color}" fill-opacity="${opacity}">`;
  for (let i = 0; i < count; i++) {
    const x = Math.round(rand() * width);
    const y = Math.round(rand() * height);
    const r = (minR + rand() * (maxR - minR)).toFixed(1);
    s += `<circle cx="${x}" cy="${y}" r="${r}"/>`;
  }
  return s + '</g>';
}
function save(name, content) { fs.writeFileSync(path.join(out, `${name}.svg`), svg(content), 'utf8'); }

reset(11);
save('fog', `<rect width="${width}" height="${height}" fill="#DCE6E2" opacity=".27"/><path d="M0 340 Q450 300 900 345 T2172 330 V510 Q1500 460 900 510 T0 490Z" fill="#E4E9E5" opacity=".34"/><path d="M0 495 Q600 455 1200 500 T2172 480 V724 H0Z" fill="#EDF0EA" opacity=".27"/>`);

reset(12);
save('rain', `<rect width="${width}" height="${height}" fill="#2D4552" opacity=".16"/>${lines(430, '#B9D5D7', .56, -9, 29, 2)}<path d="M0 660 Q500 650 1000 664 T2172 660" fill="none" stroke="#A2BFC5" stroke-opacity=".42" stroke-width="4"/>`);

reset(13);
save('acid-rain', `<rect width="${width}" height="${height}" fill="#617257" opacity=".23"/>${lines(390, '#ACD076', .55, -11, 28, 2)}<path d="M0 642 Q510 657 1050 641 T2172 650" fill="none" stroke="#9BB768" stroke-opacity=".62" stroke-width="6"/>`);

save('scorching-sun', `<rect width="${width}" height="${height}" fill="#E4A54F" opacity=".15"/><circle cx="1620" cy="210" r="145" fill="#F2CB7C" opacity=".28"/><circle cx="1620" cy="210" r="100" fill="#F5D594" opacity=".26"/><path d="M0 570 Q250 554 510 570 T1030 570 T1560 570 T2172 570 M0 610 Q300 594 590 610 T1200 610 T1830 610 T2172 610" fill="none" stroke="#E9C583" stroke-opacity=".30" stroke-width="7"/>`);

reset(14);
save('blizzard', `<rect width="${width}" height="${height}" fill="#DCEAF2" opacity=".38"/>${lines(560, '#FFFFFF', .69, 34, 9, 3)}${dots(170, '#FFFFFF', .72, 1.5, 4)}<path d="M0 640 Q450 615 900 645 T2172 625 V724 H0Z" fill="#EDF5F8" opacity=".25"/>`);

reset(15);
save('thunderstorm', `<rect width="${width}" height="${height}" fill="#172837" opacity=".53"/><path d="M0 140 Q490 110 960 145 T2172 128 V0 H0Z" fill="#202B39" opacity=".48"/>${lines(480, '#9FBCC6', .48, -10, 31, 2)}<path d="M1450 114 L1380 283 L1467 270 L1360 460 L1400 302 L1320 315Z" fill="#E5E7D4" opacity=".90"/>`);

reset(16);
save('hurricane', `<rect width="${width}" height="${height}" fill="#213643" opacity=".34"/>${lines(550, '#BDD0CE', .49, 41, 5, 2)}<g fill="none" stroke="#D5DEDA" stroke-opacity=".44" stroke-width="7"><path d="M0 233 C550 160 860 330 1370 230 S1960 200 2172 240"/><path d="M0 450 C500 380 890 510 1300 400 S1900 370 2172 420"/><path d="M0 610 C570 550 990 650 1400 560 S1900 540 2172 570"/></g>`);

reset(17);
save('downpour', `<rect width="${width}" height="${height}" fill="#243947" opacity=".39"/>${lines(920, '#B3D1D5', .70, -7, 47, 3)}<path d="M0 660 Q400 645 850 662 T1650 662 T2172 655" fill="none" stroke="#C2DCE0" stroke-opacity=".50" stroke-width="7"/>`);

reset(18);
save('tornado', `<rect width="${width}" height="${height}" fill="#3C4D53" opacity=".28"/><path d="M1120 95 Q1420 70 1710 105 Q1600 200 1480 275 Q1600 290 1540 380 Q1480 455 1430 640 L1395 650 Q1420 490 1470 405 Q1505 335 1420 282 Q1290 205 1120 95Z" fill="#4B5A60" opacity=".79"/><path d="M1130 110 Q1420 75 1700 115 M1260 178 Q1460 154 1600 204 M1370 275 Q1500 264 1570 295 M1400 665 Q1530 638 1660 660" fill="none" stroke="#C7D0C9" stroke-opacity=".47" stroke-width="12"/>${lines(170, '#A6AAA1', .42, 35, -8, 3)}`);

reset(19);
save('flood', `<rect x="0" y="558" width="${width}" height="166" fill="#456879" opacity=".62"/><path d="M0 560 Q150 535 300 560 T600 560 T900 560 T1200 560 T1500 560 T1800 560 T2172 560" fill="none" stroke="#A6CAD0" stroke-opacity=".73" stroke-width="10"/><g fill="none" stroke="#B3D4D5" stroke-opacity=".49" stroke-width="5"><path d="M90 603 h170 m280 35 h240 m220-42 h190 m230 49 h310 m150-30 h170"/></g>`);

reset(20);
save('snowfall', `<rect width="${width}" height="${height}" fill="#D5E4ED" opacity=".12"/>${dots(440, '#F6F8F7', .80, 1.5, 4.5)}${dots(85, '#E5F0F3', .70, 4, 7)}`);

reset(21);
let hail = `<rect width="${width}" height="${height}" fill="#B7CDD2" opacity=".15"/><g fill="#EAF5F7" fill-opacity=".82" stroke="#BDD4D9" stroke-width="1">`;
for (let i = 0; i < 430; i++) {
  const x = Math.round(rand() * width), y = Math.round(rand() * height), r = 2 + Math.round(rand() * 3);
  hail += `<path d="M${x} ${y-r} l${r} ${r} -${r} ${r} -${r} -${r}Z"/>`;
}
save('hail', hail + '</g>');

reset(22);
save('dust-storm', `<rect width="${width}" height="${height}" fill="#A8875E" opacity=".42"/>${lines(360, '#C6A57B', .50, 40, 5, 3)}<path d="M0 390 Q450 340 900 395 T2172 380 V610 Q1600 555 1100 610 T0 585Z" fill="#B3956D" opacity=".23"/>`);

reset(23);
save('ashfall', `<rect width="${width}" height="${height}" fill="#555B5D" opacity=".22"/>${dots(470, '#D7D2C7', .43, 1, 3.5)}${lines(130, '#A6A8A5', .33, -7, 13, 2)}`);

save('night-sky', `<rect width="${width}" height="${height}" fill="#0B1D32" opacity=".50"/><circle cx="1620" cy="195" r="38" fill="#EEE4C8" opacity=".67"/>${dots(80, '#E4E8E3', .68, .7, 1.8)}`);
