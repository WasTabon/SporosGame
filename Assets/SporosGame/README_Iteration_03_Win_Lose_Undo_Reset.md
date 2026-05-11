# SporosGame — Iteration 3: Win/Lose/Pause + Undo/Reset + Level progression

## 1. Что добавлено
WinPopup со звёздами и кнопками Next/Retry/Menu, LosePopup с Retry/Menu, PausePopup с Resume/Restart/Menu (Time.timeScale = 0). UndoSystem на одно действие. Pause/Undo/Reset кнопки в HUD. Прогрессия 3 placeholder уровней (3x3, 4x3, 4x4) через PlayerPrefs.

## 2. Что изменилось с прошлой итерации
- Game scene дополняется (не пересоздаётся): добавлены Pause кнопка в HUD top-right, ActionButtons контейнер справа-снизу с Undo+Reset, 3 popup-а
- Cell получил метод `ForceSetState(CellState)` для undo restore без анимаций
- SporeInventory получил `GetItems()` и `SetCount(type, count)`
- LevelConfig: 3 уровня + класс `LevelProgress` (PlayerPrefs key `spo_current_level`)
- GameController читает текущий уровень из PlayerPrefs, обрабатывает все попапы и кнопки

## 3. Editor скрипты — порядок запуска
1. `Tools → SporosGame → Iteration 3 → Update Game Scene (Iteration 3)`

(Iteration 1 и 2 setup уже сделаны — не запускать заново, иначе сцена пересоздастся и потеряет правки)

## 4. Как тестировать
1. Открыть MainMenu → Play → Level 1 → Game
2. Поставить споры → активировать все клетки → WinPopup появится через ~0.5s со звёздами (все 3 заполнены пока)
3. Тап Next → Level 2 (4x3), Next → Level 3 (4x4), Next → снова Level 1 (циклично)
4. На любом уровне поставить спору → тап Undo (стрелка) → спора убирается, клетки возвращаются в Inactive, счётчик восстановлен. Undo доступен только 1 раз
5. Тап Reset (круговая стрелка) → перезапуск уровня
6. Тап Pause (II) → попап, игра на паузе. Resume / Restart / Menu
7. Использовать все споры не активировав поле → LosePopup

## 5. Ожидаемый результат
- Все попапы появляются с scale 0→1 OutBack, backdrop fade-in
- В WinPopup звёзды появляются последовательно с delay 0.18s, scale 0→1 с pop sound
- Undo кнопка серая когда нет snapshot, активная — magenta glow цвет
- Pause замораживает время; popup на TimeScale=0 анимирует через .SetUpdate(true)
- Все переходы между уровнями через fade transition

## 6. Известные ограничения
- В WinPopup всегда 3 звезды (реальная star rating в it06 — на основе времени и кол-ва использованных спор)
- Undo — только 1 действие (как ты просил)
- Уровни 1-3 — placeholder. Реальные 20 уровней в it05
- Спец клетки и mutated споры — it07
- Если поле не помещается на экран (4x4) — пока работает через FitCameraToGrid (зум камеры), swipe-panning в будущем

## 7. Что в следующей итерации
Iteration 4 — полировка core mechanic: particle burst при активации, trail effect лучей, screen shake, haptics на ключевых событиях, neon glow.
