const layout = window.BUNKER_LAYOUT;
const svgNS = 'http://www.w3.org/2000/svg';
const scene = document.getElementById('scene');
const objectLayer = document.getElementById('object-layer');
const gridLayer = document.getElementById('grid-layer');
const heroLayer = document.getElementById('hero-layer');
const objectList = document.getElementById('object-list');
const actionButton = document.getElementById('action-button');
const state = {
  selected: null,
  entranceOpen: false,
  ruinsRepaired: false,
  rubbleCleared: false,
  roomCardOpen: false,
  routeOpen: false,
  hero: { x: 29, y: 25.5 },
  task: null,
  timer: null
};

function svgElement(tag, attributes, parent) {
  const element = document.createElementNS(svgNS, tag);
  for (const [key, value] of Object.entries(attributes)) element.setAttribute(key, String(value));
  parent.appendChild(element);
  return element;
}

function shape(group, tag, attributes) { return svgElement(tag, attributes, group); }

function clear(element) { element.replaceChildren(); }

function objectById(id) { return layout.objects.find(object => object.id === id); }

function makeGroup(object) {
  const group = svgElement('g', {
    class: `object ${object.area}${state.selected === object.id ? ' selected' : ''}`,
    'data-id': object.id,
    tabindex: 0,
    role: 'button',
    'aria-label': object.name
  }, objectLayer);
  group.addEventListener('click', () => selectObject(object.id));
  group.addEventListener('keydown', event => {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      selectObject(object.id);
    }
  });
  return group;
}

function rectangle(group, x, y, width, height, extra = {}) {
  return shape(group, 'rect', { x, y, width, height, ...extra });
}

function number(group, value, x, y) {
  if (value == null) return;
  shape(group, 'text', { x, y, class: 'object-number' }).textContent = value;
}

function drawEntry(object) {
  const group = makeGroup(object);
  rectangle(group, 2, 19, 18, 7, { class: 'structure' });
  shape(group, 'path', { d: 'M2 19 L4 17.8 L18 17.8 L20 19', class: 'structure-line' });
  number(group, object.number, 10.4, 18.9);
}

function drawStairs(object) {
  const group = makeGroup(object);
  rectangle(group, 3, 21, 7, 5, { class: 'shaft' });
  rectangle(group, 2, 33, 8, 7, { class: 'shaft' });
  shape(group, 'polyline', { points: '3.5,25.7 4.4,25.7 4.4,24.8 5.3,24.8 5.3,23.9 6.2,23.9 6.2,23 7.1,23 7.1,22.1 8.2,22.1', class: 'stairs-line' });
  shape(group, 'polyline', { points: '2.8,39.6 4,39.6 4,38.7 5.2,38.7 5.2,37.8 6.4,37.8 6.4,36.9 7.6,36.9 7.6,36 8.8,36', class: 'stairs-line' });
  number(group, object.number, 6.1, 23.5);
}

function drawElevator(object) {
  const group = makeGroup(object);
  rectangle(group, 12, 21, 7, 5, { class: 'shaft' });
  rectangle(group, 12, 33, 7, 7, { class: 'shaft' });
  shape(group, 'line', { x1: 15.5, y1: 21.5, x2: 15.5, y2: 26, class: 'door-seam' });
  shape(group, 'line', { x1: 15.5, y1: 33.5, x2: 15.5, y2: 40, class: 'door-seam' });
  rectangle(group, 17.5, 23.6, .3, .7, { class: 'indicator' });
  number(group, object.number, 14.8, 23.5);
}

function drawEntrance(object) {
  const group = makeGroup(object);
  rectangle(group, object.x, object.y, object.width, object.height, { class: state.entranceOpen ? 'structure open' : 'structure' });
  rectangle(group, 22.3, 21.2, 3.5, 4.8, { class: state.entranceOpen ? 'open-door' : 'metal-door' });
  shape(group, 'line', { x1: 24.05, y1: 21.3, x2: 24.05, y2: 25.9, class: 'door-seam' });
  rectangle(group, 26.2, 23.5, .35, .8, { class: 'indicator' });
  number(group, object.number, 24, 19.3);
}

function drawRuins(object) {
  const group = makeGroup(object);
  const colorClass = state.ruinsRepaired ? 'structure repaired' : 'ruin';
  rectangle(group, object.x, object.y, object.width, object.height, { class: colorClass });
  if (!state.ruinsRepaired) {
    shape(group, 'polyline', { points: '30,21 32,20.4 35,21 38,19.8 41,20.9 44,20.2 47,21 50,20.4 51,21', class: 'ruin-roof' });
  }
  rectangle(group, 37, 22, 6, 4, { class: 'room-project' });
  number(group, object.number, 40, 20.2);
}

function drawCheckpoint(object) {
  const group = makeGroup(object);
  rectangle(group, object.x, object.y, object.width, object.height, { class: 'checkpoint' });
  rectangle(group, 58, 23, 2.8, 1.1, { class: 'checkpoint-window' });
  number(group, object.number, 59.5, 21.4);
}

function drawHedgehog(object) {
  const group = makeGroup(object);
  rectangle(group, 61.8, 22.8, 4.5, 3.2, { class: 'hitbox' });
  shape(group, 'path', { d: 'M62.5 25.7 L65.5 23.2 M62.5 23.2 L65.5 25.7 M62 24.5 L66 24.5', class: 'external-line' });
  number(group, object.number, 63.8, 22.3);
}

function drawFlag(object) {
  const group = makeGroup(object);
  rectangle(group, 65, 16, 4, 10, { class: 'hitbox' });
  shape(group, 'line', { x1: 66, y1: 17, x2: 66, y2: 26, class: 'external-line' });
  shape(group, 'path', { d: 'M66 17 L69 18 L66 19 Z', class: 'flag-cloth' });
  number(group, object.number, 67, 15.8);
}

function drawCorridor(object) {
  const group = makeGroup(object);
  rectangle(group, object.x, object.y, object.width, object.height, { class: 'corridor' });
  for (const lightX of [23, 35, 43]) rectangle(group, lightX, 34.2, 1.2, .22, { class: 'indicator' });
  number(group, object.number, 36, 33.5);
}

function drawRoomDoor(object) {
  const group = makeGroup(object);
  rectangle(group, object.x, object.y, object.width, object.height, { class: 'room-door' });
  shape(group, 'line', { x1: 28, y1: 37.2, x2: 28, y2: 39.8, class: 'door-seam' });
  rectangle(group, 28.5, 38.2, .2, .35, { class: 'indicator' });
}

function drawRubble(object) {
  const group = makeGroup(object);
  if (state.rubbleCleared) {
    rectangle(group, 46, 34, 5, 6, { class: 'corridor cleared' });
    return;
  }
  rectangle(group, 46, 34, 5, 6, { class: 'hitbox' });
  shape(group, 'polygon', { points: '46,40 47,38.2 48,38.5 48.6,36.8 49.7,36.3 50.6,34.6 51,40', class: 'rubble' });
}

const drawers = {
  entry: drawEntry, stairs: drawStairs, elevator: drawElevator,
  entrance: drawEntrance, ruins: drawRuins, checkpoint: drawCheckpoint,
  hedgehog: drawHedgehog, flag: drawFlag, corridor: drawCorridor,
  roomDoor: drawRoomDoor, rubble: drawRubble
};

function drawScene() {
  clear(objectLayer);
  const order = ['entry', 'corridor', 'ruins', 'entrance', 'stairs', 'elevator', 'checkpoint', 'hedgehog', 'flag', 'roomDoor', 'rubble'];
  for (const id of order) drawers[id](objectById(id));
  clear(heroLayer);
  const hero = svgElement('g', { 'aria-label': 'Герой' }, heroLayer);
  shape(hero, 'circle', { cx: state.hero.x, cy: state.hero.y - 1.8, r: .42, class: 'hero-head' });
  shape(hero, 'path', { d: `M${state.hero.x} ${state.hero.y - 1.3} L${state.hero.x} ${state.hero.y - .15} M${state.hero.x - .5} ${state.hero.y - 1} L${state.hero.x + .5} ${state.hero.y - 1} M${state.hero.x} ${state.hero.y - .15} L${state.hero.x - .5} ${state.hero.y + .45} M${state.hero.x} ${state.hero.y - .15} L${state.hero.x + .5} ${state.hero.y + .45}`, class: 'hero-body' });
}

function drawGrid() {
  clear(gridLayer);
  for (let x = 0; x <= layout.columns; x++) shape(gridLayer, 'line', { x1: x, y1: 0, x2: x, y2: layout.rows, class: x % 5 ? 'grid-minor' : 'grid-major' });
  for (let y = 0; y <= layout.rows; y++) shape(gridLayer, 'line', { x1: 0, y1: y, x2: layout.columns, y2: y, class: y % 5 ? 'grid-minor' : 'grid-major' });
}

function actionFor(object) {
  if (!object) return null;
  if (state.task) return { label: 'Выполняется работа…', disabled: true };
  switch (object.id) {
    case 'entrance': return state.entranceOpen ? { label: 'Войти в бункер', run: () => { state.hero = { x: 19, y: 25.3 }; log('Герой вошёл во входную группу.'); refresh(); } } : { label: 'Взломать вход · демо 5 с', run: () => startTask('Взлом входа', () => { state.entranceOpen = true; log('Наружная дверь открыта.'); }) };
    case 'stairs': return state.entranceOpen ? { label: 'Спуститься на этаж 1', run: () => { state.hero = { x: 9, y: 39.4 }; log('Герой спустился по лестнице.'); refresh(); } } : { label: 'Сначала откройте вход', disabled: true };
    case 'ruins': return state.ruinsRepaired ? null : { label: 'Починить дом · демо 5 с', run: () => startTask('Ремонт руин', () => { state.ruinsRepaired = true; log('Руины восстановлены; доступен проект средней комнаты.'); }) };
    case 'checkpoint': return { label: state.routeOpen ? 'Скрыть маршрут' : 'Открыть маршрут', run: () => { state.routeOpen = !state.routeOpen; log(state.routeOpen ? 'Открыта карточка маршрута пустоши.' : 'Карточка маршрута закрыта.'); refresh(); } };
    case 'roomDoor': return { label: state.roomCardOpen ? 'Закрыть карточку' : 'Открыть карточку комнаты', run: () => { state.roomCardOpen = !state.roomCardOpen; refresh(); } };
    case 'rubble': return state.rubbleCleared ? null : { label: 'Расчистить · демо 5 с', run: () => startTask('Расчистка завала', () => { state.rubbleCleared = true; log('Завал убран. Коридор можно расширять.'); }) };
    default: return null;
  }
}

function renderDetail() {
  const object = objectById(state.selected);
  document.getElementById('detail-title').textContent = object ? object.name : 'Первый экран';
  document.getElementById('detail-location').textContent = object ? `${object.area === 'outside' ? 'Внешний мир' : object.area === 'surface' ? 'Поверхность' : 'Подземный этаж 1'} · x ${object.x}–${object.x + object.width - 1}` : 'Поверхность · 100 клеток';
  document.getElementById('detail-text').textContent = object ? object.description : 'Выбери объект и посмотри, где он находится и какое действие будет доступно игроку.';
  const extra = document.getElementById('detail-extra');
  extra.replaceChildren();
  if (state.task && object) {
    const paragraph = document.createElement('p');
    paragraph.className = 'task-progress';
    paragraph.textContent = `${state.task.label}: ${Math.ceil((state.task.endsAt - Date.now()) / 1000)} с`;
    extra.appendChild(paragraph);
  }
  if (object?.id === 'roomDoor' && state.roomCardOpen) {
    const panel = document.createElement('div');
    panel.className = 'mini-panel';
    panel.innerHTML = '<strong>Маленькая комната</strong><span>8 мест оснащения · дверь 2 × 3 клетки</span><span>Предметы на сцене не расставляются: они выбираются здесь.</span>';
    extra.appendChild(panel);
  }
  if (object?.id === 'checkpoint' && state.routeOpen) {
    const panel = document.createElement('div');
    panel.className = 'mini-panel';
    panel.innerHTML = '<strong>Маршрут: ближние руины</strong><span>Первый выход в пустошь · событие и добыча будут следующим шагом.</span>';
    extra.appendChild(panel);
  }
  const action = actionFor(object);
  actionButton.classList.toggle('hidden', !action);
  if (action) {
    actionButton.textContent = action.label;
    actionButton.disabled = Boolean(action.disabled);
    actionButton.onclick = action.run || null;
  }
}

function renderList() {
  objectList.replaceChildren();
  for (const object of layout.objects.filter(item => item.number != null)) {
    const button = document.createElement('button');
    button.type = 'button';
    button.className = `object-list-item${state.selected === object.id ? ' active' : ''}`;
    button.innerHTML = `<b>${object.number}</b><span>${object.name}</span>`;
    button.addEventListener('click', () => selectObject(object.id));
    objectList.appendChild(button);
  }
}

function log(message) {
  document.getElementById('status').textContent = message;
  const item = document.createElement('li');
  item.textContent = message;
  document.getElementById('event-log').prepend(item);
  while (document.getElementById('event-log').children.length > 5) document.getElementById('event-log').lastElementChild.remove();
}

function refresh() { drawScene(); renderDetail(); renderList(); }
function selectObject(id) { state.selected = id; refresh(); }

function startTask(label, onDone) {
  state.task = { label, endsAt: Date.now() + 5000 };
  log(`${label} начат. В полном сценарии длительность задаст симуляция.`);
  refresh();
  state.timer = window.setInterval(() => {
    if (Date.now() < state.task.endsAt) { renderDetail(); return; }
    window.clearInterval(state.timer);
    state.timer = null;
    state.task = null;
    onDone();
    refresh();
  }, 150);
}

document.getElementById('grid-toggle').addEventListener('change', event => gridLayer.classList.toggle('hidden', !event.target.checked));
document.getElementById('reset-button').addEventListener('click', () => window.location.reload());
scene.addEventListener('click', event => {
  if (!event.target.closest('[data-id]')) { state.selected = null; refresh(); }
});

drawGrid();
refresh();
