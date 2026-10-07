// Координаты первой сцены. Один столбец/ряд = 0,8 м.
// Эта модель служит источником правды для макета; её можно перенести в JSON-контент Godot.
const BUNKER_LAYOUT = {
  columns: 100,
  rows: 54,
  cellMeters: 0.8,
  surfaceLine: 26,
  separatorRows: [26, 27, 40, 41],
  surfacePlayable: { from: 0, to: 54 },
  outsideWorld: { from: 55, to: 99 },
  objects: [
    { id: 'flag', number: 1, name: 'Входной флаг', area: 'outside', x: 65, y: 16, width: 4, height: 10, description: 'Ориентир зоны действий с внешним миром.' },
    { id: 'hedgehog', number: 2, name: 'Противотанковый ёж', area: 'outside', x: 62, y: 23, width: 4, height: 3, description: 'Препятствие у КПП. Пока служит ориентиром.' },
    { id: 'checkpoint', number: 3, name: 'КПП', area: 'outside', x: 57, y: 22, width: 5, height: 4, description: 'Точка перехода к маршрутам и событиям пустоши.' },
    { id: 'ruins', number: 4, name: 'Руины дома', area: 'surface', x: 30, y: 21, width: 21, height: 5, description: 'Разрушенное здание. После ремонта здесь появится место для средней комнаты: 6 клеток стены и 16 мест оснащения.' },
    { id: 'entrance', number: 5, name: 'Вход в бункер', area: 'surface', x: 20, y: 20, width: 8, height: 6, description: 'Наружная дверь. Герой подходит справа, взламывает замок и входит влево.' },
    { id: 'entry', number: 6, name: 'Входная группа', area: 'surface', x: 2, y: 19, width: 18, height: 7, description: 'Общий входной участок. Точное место охранной комнаты ещё не определено.' },
    { id: 'elevator', number: 7, name: 'Лифт', area: 'surface', x: 12, y: 21, width: 7, height: 5, description: 'Шахта совпадает по оси с нижним уровнем. На старте лифт не работает.' },
    { id: 'stairs', number: 8, name: 'Лестница', area: 'surface', x: 3, y: 21, width: 7, height: 5, description: 'Рабочий путь между входной группой и первым подземным этажом.' },
    { id: 'corridor', number: 9, name: 'Подземный коридор', area: 'underground', x: 20, y: 34, width: 26, height: 6, description: 'Коридор идёт вправо от лестницы и лифта. В нём одна дверь маленькой комнаты; справа завал.' },
    { id: 'roomDoor', number: null, name: 'Дверь маленькой комнаты', area: 'underground', x: 27, y: 37, width: 2, height: 3, description: 'Дверь 2 × 3 клетки. За ней маленькая комната на 8 мест; оснащение выбирают в карточке.' },
    { id: 'rubble', number: null, name: 'Завал', area: 'underground', x: 46, y: 34, width: 5, height: 6, description: 'Перекрывает дальнейшее расширение коридора.' }
  ]
};

if (typeof window !== 'undefined') window.BUNKER_LAYOUT = BUNKER_LAYOUT;
if (typeof module !== 'undefined') module.exports = BUNKER_LAYOUT;
