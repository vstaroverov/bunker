const assert = require('node:assert/strict');
const layout = require('./layout.js');

assert.equal(layout.columns, 100);
assert.equal(layout.surfacePlayable.to, 54);
assert.equal(layout.outsideWorld.from, 55);
assert.deepEqual(layout.separatorRows, [26, 27, 40, 41]);

const ids = new Set();
for (const object of layout.objects) {
  assert(!ids.has(object.id), `Повторный id: ${object.id}`);
  ids.add(object.id);
  assert(object.x >= 0 && object.x + object.width <= layout.columns, `${object.id}: столбцы вне сетки`);
  assert(object.y >= 0 && object.y + object.height <= layout.rows, `${object.id}: ряды вне сетки`);
  if (object.area === 'outside') assert(object.x >= 55, `${object.id}: объект внешнего мира слева от границы`);
  if (object.area === 'surface') assert(object.x + object.width <= 55, `${object.id}: объект базы справа от границы`);
  if (object.area === 'underground') assert(object.y >= 28 && object.y + object.height <= 40, `${object.id}: объект вне первого подземного этажа`);
}

assert.equal(layout.objects.filter(object => object.number != null).length, 9);
const door = layout.objects.find(object => object.id === 'roomDoor');
assert.equal(door.width, 2);
assert.equal(door.height, 3);
const corridor = layout.objects.find(object => object.id === 'corridor');
assert(door.x >= corridor.x && door.x + door.width <= corridor.x + corridor.width);

console.log('Планировка проверена: объекты, зоны и дверь находятся в ожидаемых клетках.');
