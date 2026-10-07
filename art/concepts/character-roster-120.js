// Генератор обзорного листа. Запуск: node art/concepts/character-roster-120.js
const fs = require('node:fs');
const path = require('node:path');

const W = 2200;
const H = 2540;
const startY = 170;
const rowH = 190;
const startX = 390;
const colW = 174;
const ink = '#172127';
const rows = [
  { label: 'Белые · мужчины', type: 'human', group: 'white', sex: 'm', scale: 1 },
  { label: 'Белые · женщины', type: 'human', group: 'white', sex: 'f', scale: 1 },
  { label: 'Азиаты · мужчины', type: 'human', group: 'asian', sex: 'm', scale: .92, width: .93 },
  { label: 'Азиатки · женщины', type: 'human', group: 'asian', sex: 'f', scale: .92, width: .93 },
  { label: 'Чернокожие · мужчины', type: 'human', group: 'black', sex: 'm', scale: 1.08, width: 1.07 },
  { label: 'Чернокожие · женщины', type: 'human', group: 'black', sex: 'f', scale: 1.08, width: 1.07 },
  { label: 'Мутанты · мужчины', type: 'mutant', group: 'mutant', sex: 'm', scale: 1.08, width: 1.07 },
  { label: 'Мутанты · женщины', type: 'mutant', group: 'mutant', sex: 'f', scale: 1.08, width: 1.07 },
  { label: 'Киборги · мужчины', type: 'cyborg', group: 'cyborg', sex: 'm', scale: 1 },
  { label: 'Киборги · женщины', type: 'cyborg', group: 'cyborg', sex: 'f', scale: 1 },
  { label: 'Псионики · мужчины', type: 'psionic', group: 'psionic', sex: 'm', scale: .92, width: .93 },
  { label: 'Псионики · женщины', type: 'psionic', group: 'psionic', sex: 'f', scale: .92, width: .93 },
];

const skin = {
  white: ['#E6BE9D', '#F0CCAC', '#D5A987', '#EBC3A1', '#D6B093'],
  asian: ['#DBAE86', '#C9956F', '#E0B78F', '#BC8D6D', '#D4A47A'],
  black: ['#8E5F45', '#654331', '#A47151', '#795039', '#B37D58'],
  mutant: ['#C19D79', '#9DA88E', '#C3A38A', '#9C8B72', '#B4A67F'],
};
const cloth = ['#526168', '#46535C', '#617468', '#80725D', '#5B6C72'];
const accent = ['#E4A54F', '#D6B17A', '#65ADB4', '#86A96E', '#B8A878'];
const hair = {
  white: ['#3D332E', '#84603F', '#D0AE7F', '#5B4B3D', '#8C5B4A'],
  asian: ['#252D31', '#323337', '#3A302E', '#222B2D', '#41403D'],
  black: ['#25272A', '#34302B', '#222629', '#3E3027', '#2D2926'],
  mutant: ['#3D3932', '#465044', '#74644E', '#383A35', '#596258'],
};
const esc = s => s.replaceAll('&', '&amp;').replaceAll('<', '&lt;');

function hairShape(group, sex, i) {
  const c = hair[group][i % 5];
  const k = i % 5;
  if (group === 'black' && k < 3) {
    return `<g fill="${c}" stroke="${ink}" stroke-width="2"><circle cx="39" cy="17" r="6"/><circle cx="48" cy="13" r="7"/><circle cx="58" cy="15" r="7"/><circle cx="64" cy="22" r="5"/>${sex === 'f' ? '<circle cx="70" cy="28" r="6"/>' : ''}</g>`;
  }
  if (sex === 'f' && k % 2 === 0) {
    return `<path d="M33 29 Q31 13 48 11 Q64 8 68 24 L68 48 L61 45 L61 20 Q48 26 36 22 L37 47 L32 43Z" fill="${c}" stroke="${ink}" stroke-width="2"/>`;
  }
  if (sex === 'f') {
    return `<path d="M33 28 Q33 11 49 11 Q66 10 67 27 L62 24 Q51 24 36 21Z" fill="${c}" stroke="${ink}" stroke-width="2"/><circle cx="69" cy="25" r="7" fill="${c}" stroke="${ink}" stroke-width="2"/>`;
  }
  return `<path d="M34 28 Q31 12 50 11 Q67 11 66 29 L61 23 Q46 20 35 25Z" fill="${c}" stroke="${ink}" stroke-width="2"/>`;
}

function equipment(i, a) {
  switch (i % 5) {
    case 0: return `<rect x="69" y="60" width="11" height="15" rx="2" fill="${a}" stroke="${ink}" stroke-width="2"/>`;
    case 1: return `<path d="M19 69 L29 66 L31 79 L20 81Z" fill="${a}" stroke="${ink}" stroke-width="2"/>`;
    case 2: return `<path d="M72 60 v22 m-5-17 h10" stroke="${a}" stroke-width="4" stroke-linecap="round"/>`;
    case 3: return `<path d="M32 54 L63 70" stroke="${a}" stroke-width="5"/>`;
    default: return `<circle cx="72" cy="72" r="6" fill="${a}" stroke="${ink}" stroke-width="2"/>`;
  }
}

function mutantTraits(i, s) {
  const animal = i % 5;
  if (animal === 0) return `<path d="M36 19 L29 4 L43 13 M61 14 L72 3 L66 23" fill="${s}" stroke="${ink}" stroke-width="3"/><path d="M67 92 Q90 101 81 121" fill="none" stroke="${s}" stroke-width="9" stroke-linecap="round"/>`;
  if (animal === 1) return `<path d="M38 19 Q25 7 29 2 M60 17 Q74 6 69 1" fill="none" stroke="#D6B17A" stroke-width="6" stroke-linecap="round"/><path d="M20 75 l-6 8 m9-5 l-4 9" stroke="${s}" stroke-width="4"/>`;
  if (animal === 2) return `<path d="M31 27 l-14-8 15 2 M67 26 l14-8-14 4" fill="${s}" stroke="${ink}" stroke-width="2"/><path d="M70 88 Q82 95 75 117" fill="none" stroke="${s}" stroke-width="8"/>`;
  if (animal === 3) return `<path d="M37 13 l-3-16 9 14 M49 10 l4-18 4 20" fill="#86A96E" stroke="${ink}" stroke-width="2"/><path d="M25 62 l-10-7 5 18" fill="${s}" stroke="${ink}" stroke-width="2"/>`;
  return `<path d="M34 15 L24 8 L29 31 M65 16 L76 8 L71 32" fill="${s}" stroke="${ink}" stroke-width="2"/><path d="M72 91 Q89 103 82 119" fill="none" stroke="${s}" stroke-width="7"/>`;
}

function figure(row, i) {
  const v = [0, -.025, .025, -.04, .04, -.015, .015, -.03, .03, 0][i];
  const sc = row.scale + v;
  const c = cloth[(i + (row.sex === 'f' ? 1 : 0)) % cloth.length];
  const a = accent[i % accent.length];
  const s = skin[row.group]?.[i % 5] || '#B6B9B0';
  const pants = i % 2 ? '#39474E' : '#46535C';
  let body = '';
  if (row.type === 'cyborg') {
    const metal = i % 2 ? '#73848A' : '#8C9898';
    body = `<g stroke="${ink}" stroke-width="3" stroke-linejoin="round">
      <path d="M35 86 L47 86 L46 119 L37 119Z M54 86 L66 86 L64 119 L54 119Z" fill="${metal}"/>
      <path d="M36 116 h13 v8 H34Z M54 116 h13 v8 H53Z" fill="#273238"/>
      <path d="M31 51 L68 51 L72 88 H28Z" fill="${metal}"/>
      <path d="M29 55 L20 58 L20 85 L28 85 L36 58 M70 55 L79 58 L79 85 L71 85 L64 58" fill="#526168"/>
      <rect x="34" y="13" width="32" height="33" rx="5" fill="${metal}"/>
      <rect x="39" y="26" width="22" height="7" rx="2" fill="${a}"/>
      <path d="M41 17 h18 M39 69 h22 M40 95 h8 M55 103 h8" fill="none" stroke="#273238" stroke-width="3"/>
      <circle cx="49" cy="76" r="5" fill="#65ADB4"/>
    </g>`;
  } else if (row.type === 'psionic') {
    body = `<g stroke="${ink}" stroke-width="3" stroke-linejoin="round">
      <path d="M41 88 L48 88 L46 120 H40Z M53 88 L60 88 L60 120 H54Z" fill="${pants}"/>
      <path d="M39 118 h10 v6 H36Z M53 118 h11 v6 H52Z" fill="#273238"/>
      <path d="M41 54 L59 54 L62 90 H38Z" fill="${c}"/>
      <path d="M40 59 L30 61 L28 91 M60 59 L70 61 L72 91" fill="none" stroke="${c}" stroke-width="7" stroke-linecap="round"/>
      <ellipse cx="50" cy="28" rx="22" ry="23" fill="${s}"/>
      <path d="M31 22 Q32 5 50 6 Q69 6 69 22 L63 15 Q50 19 34 16Z" fill="${hair[row.group]?.[i % 5] || '#3B3B3D'}"/>
      <circle cx="43" cy="29" r="1.7" fill="${ink}" stroke="none"/><circle cx="57" cy="29" r="1.7" fill="${ink}" stroke="none"/>
      <path d="M69 26 h5 v6 h-5" fill="#65ADB4" stroke="none"/>
    </g>`;
  } else {
    body = `<g stroke="${ink}" stroke-width="3" stroke-linejoin="round">
      <path d="M35 86 L48 86 L47 119 L36 119Z M52 86 L65 86 L64 119 L53 119Z" fill="${pants}"/>
      <path d="M34 117 h15 v7 H32Z M52 117 h15 v7 H51Z" fill="#273238"/>
      <path d="M31 50 L68 50 L71 89 H28Z" fill="${c}"/>
      <path d="M31 56 L22 61 L21 84 L28 86 L37 61 M68 56 L77 61 L79 84 L72 86 L63 61" fill="${c}"/>
      <path d="M29 78 h42" fill="none" stroke="#273238" stroke-width="4"/>
      <ellipse cx="50" cy="30" rx="16" ry="18" fill="${s}"/>
      <circle cx="44" cy="31" r="1.6" fill="${ink}" stroke="none"/><circle cx="56" cy="31" r="1.6" fill="${ink}" stroke="none"/>
    </g>`;
    body += hairShape(row.group, row.sex, i);
    body += equipment(i, a);
    if (row.type === 'mutant') body += mutantTraits(i, s);
  }
  const x = startX + colW * i + (1 - sc) * 50;
  const y = 0;
  const width = row.width || 1;
  return `<g transform="translate(${x} ${y}) scale(${sc.toFixed(3)})"><g transform="translate(50 0) scale(${width} 1) translate(-50 0)">${body}</g></g>`;
}

const parts = [`<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 ${W} ${H}" role="img" aria-label="120 упрощённых персонажей: двенадцать рядов по десять фигур">
<rect width="${W}" height="${H}" fill="#273238"/>
<text x="46" y="63" fill="#E8E3D6" font-family="Arial,sans-serif" font-size="40" font-weight="bold">ЖИВОЙ БУНКЕР · ПЕРСОНАЖИ</text>
<text x="47" y="102" fill="#D6B17A" font-family="Arial,sans-serif" font-size="23">12 групп · 10 вариантов в каждой · плоские формы и простой силуэт</text>`];
for (let i = 0; i < 10; i++) parts.push(`<text x="${startX + colW * i + 46}" y="151" fill="#9EA7A3" font-family="Arial,sans-serif" font-size="18">${String(i + 1).padStart(2, '0')}</text>`);
rows.forEach((row, r) => {
  const y = startY + r * rowH;
  parts.push(`<g transform="translate(0 ${y})"><rect x="28" y="0" width="2144" height="174" rx="12" fill="${r % 2 ? '#303B41' : '#354149'}"/><path d="M300 17 v140" stroke="#526168" stroke-width="2"/><text x="53" y="71" fill="#E8E3D6" font-family="Arial,sans-serif" font-size="26" font-weight="bold">${esc(row.label)}</text><text x="53" y="105" fill="#B8AD95" font-family="Arial,sans-serif" font-size="17">${row.type === 'human' ? 'люди · внешний облик' : row.type === 'mutant' ? 'черты животных' : row.type === 'cyborg' ? 'целиком металл' : 'большая голова · хрупкое тело'}</text>`);
  for (let i = 0; i < 10; i++) parts.push(figure(row, i));
  parts.push('</g>');
});
parts.push(`<text x="46" y="2503" fill="#B8AD95" font-family="Arial,sans-serif" font-size="18">Различия роста человеческих рядов — только ориентир рисунка; внутри каждой группы есть разброс, игровых бонусов по происхождению нет.</text></svg>`);

fs.writeFileSync(path.join(__dirname, 'character-roster-120.svg'), parts.join('\n'), 'utf8');
