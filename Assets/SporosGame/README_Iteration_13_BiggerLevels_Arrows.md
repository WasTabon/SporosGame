# SporosGame — Iteration 13: Bigger Levels + Arrow Direction Indicators

## 1. Что добавлено
- 4 стрелки на Spore (cardinal для Basic, diagonal для Diagonal). Цвет = цвет споры. Pulse animation outward-inward 0.6s yoyo
- Тот же визуал на SporeInventoryItem в инвентаре
- При placement (PlaceAndEmit) стрелки fade-out за 0.2s и скрываются — лучи становятся видимыми, стрелки не нужны
- Triangle sprite procedurally generated
- Все 20 уровней пересозданы с увеличенными размерами (4x4 -> 8x8). Block тайлы добавлены для разнообразия

## 2. Прогрессия размеров уровней
- L1-L2: 4x4 (туториал, без Block)
- L3-L4: 5x4 с Block
- L5-L6: 5x5 с Block
- L7-L8: 6x5 с Block + Fixed
- L9-L10: 6x6 с Block + Fixed
- L11-L12: 6x6 со всеми типами клеток
- L13-L14: 7x6 со всеми типами
- L15-L16: 7x7 со всеми типами
- L17: 7x7 + Diagonal спора
- L18: 8x7 + Diagonal
- L19-L20: 8x8 + Diagonal + все типы

## 3. Что изменилось с прошлой итерации
- Spore.cs — добавлены arrowBasic и arrowDiagonal [SerializeField] arrays. SetupArrows() в Init, HideArrows() в PlaceAndEmit
- SporeInventoryItem.cs — те же arrays для UI Image. SetupArrows() в Init
- Spore.prefab — добавлены 4+4 child renderers (Arrow_B_0..3, Arrow_D_0..3)
- SporeInventoryItem.prefab — аналогично через RectTransform + Image
- Layout каждого LevelData полностью пересоздан с большим размером

## 4. Editor скрипты - порядок запуска
1. Tools -> SporosGame -> Iteration 13 -> Bigger Levels + Arrow Indicators (Iteration 13)
2. ОБЯЗАТЕЛЬНО: Tools -> SporosGame -> Iteration 7 -> Auto-Solve and Balance Levels
   (Без этого spore counts могут быть неправильные для новых layouts)

## 5. Как тестировать
1. Запусти setup it13
2. Запусти solver it07 (балансировка spore counts)
3. Play -> L1 -> в инвентаре снизу видна спора с 4 пульсирующими стрелками magenta cardinal directions
4. Тяни спору -> стрелки сопровождают drag
5. Drop на клетку -> стрелки fade-out, появляются лучи
6. L17+ -> в инвентаре зелёная Diagonal спора с 4 стрелками по углам
7. Сами уровни визуально крупнее, больше клеток, больше Block тайлов для интереса

## 6. Ожидаемый результат
- Стрелки чётко показывают игроку в каких 4 направлениях луч пойдёт
- Pulse animation outward-inward делает индикацию живой и заметной
- После placement стрелки исчезают чтоб не загромождать вид (лучи уже видны)
- Уровни визуально внушительнее на экране, больше возможностей для дизайна

## 7. Известные ограничения
- Если запустить только it13 БЕЗ it07 solver — spore counts для новых layouts взяты эвристикой (totalForCurrentStars). Могут быть нерешаемые уровни. ОБЯЗАТЕЛЬНО запустить solver
- Стрелки добавляются на prefab через editor скрипт — если у тебя в сцене уже instance Spore до setup it13, его арейs не обновятся. Если ругается — открой Spore.prefab и проверь поля arrowBasic/arrowDiagonal в инспекторе. Они должны содержать ссылки на 4 child Arrow_B_X / Arrow_D_X
- Triangle pointing up - rotation вокруг Z через atan2 даёт правильное направление для всех 8 vectors

## 8. Запомнено для будущих итераций
- При добавлении прогрессивных индикаторов в инвентаре и на gameplay объектах - дублировать setup в обоих prefabs (UI Image и SpriteRenderer)
- Triangle sprite ориентирован "вверх" по умолчанию (y растёт), rotation в editor через `Mathf.Atan2(dir.y, dir.x) * Rad2Deg - 90f`
- При изменении layouts уровней - всегда запускать solver after для пересчёта spore counts. Layouts отдельно, balance отдельно
- Layout конфликты проверять: одна и та же клетка не может быть и Block и Fixed одновременно
