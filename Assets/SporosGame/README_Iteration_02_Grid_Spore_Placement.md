# SporosGame — Iteration 2: Grid + Basic Spore + Placement

## 1. Что добавлено
Игровое поле 3x3 с hex-стилизацией, базовая спора с drag&drop из inventory, распространение лучей в 4 направления с анимацией glow-линий, активация клеток с пульсацией, HUD с back/level/timer, panel со счётчиком спор внизу.

## 2. Что изменилось с прошлой итерации
- Game сцена полностью переделана: placeholder убран, добавлены Grid, GameController, HUD, InventoryPanel
- Камера в Game сцене теперь автоматически центрируется на grid
- Создана папка `Assets/SporosGame/Prefabs/` с prefabs: Cell, Spore, RaySegment, SporeInventoryItem
- Создана папка `Assets/SporosGame/GeneratedSprites/` со спрайтами hex, circle, square, rounded (как .png ассеты, чтобы prefabs корректно сериализовались)

## 3. Editor скрипты — порядок запуска
1. `Tools → SporosGame → Iteration 2 → Setup Game Scene (Iteration 2)`

(Меню Iteration 1 трогать не надо — оно уже сделало MainMenu и LevelSelect)

## 4. Как тестировать
1. Открыть `Assets/SporosGame/Scenes/MainMenu.unity`
2. Play → MainMenu → Play → LevelSelect → Level 1 → Game
3. Снизу видна панель с 1 спорой (x3 базовых, magenta)
4. Зажать спору в панели → потащить пальцем/мышью на любую клетку
5. Отпустить над клеткой → спора прыгает на клетку, во все 4 стороны разлетаются лучи, активируют клетки
6. Если активированы все 9 клеток → в Console "[SporosGame] WIN! Time: ..."
7. Если все 3 споры использованы а клетки не покрыты → "[SporosGame] LOSE..."
8. Back → возврат в MainMenu

## 5. Ожидаемый результат
- Поле из 9 hex-клеток с чередующимся offset по строкам (hex-стилизация)
- Клетки тёмно-синие с серой обводкой в неактивном состоянии
- При активации клетка светится cyan, пульсирует мягко, scale punch
- Спора визуально magenta-круг с глоу-пульсацией
- Лучи — glow-линии, появляются последовательно от центра, исчезают через ~1 сек
- При placement — звук + medium haptic
- При активации каждой клетки — pop sound
- При победе — success chord + success haptic
- Все плейсменты с easing OutBack, лучи OutQuad, без linear easings

## 6. Известные ограничения
- Только 1 уровень захардкожен (3x3, 3 базовых споры)
- Нет popup на победу/поражение — только Debug.Log (попап в it03)
- Нет undo/reset (в it03)
- Нет специальных клеток (в it07)
- Нет mutated спор (в it07)
- Спрайты в `GeneratedSprites/` — пересоздаются если файлов нет; если уже есть — переиспользуются

## 7. Что в следующей итерации
Iteration 3 — Win/Lose/Pause попапы, Undo (1 действие), Reset, переход на следующий уровень.
