# SporosGame — Iteration 9: Tutorial (no text)

## 1. Что добавлено
- TutorialManager — управляет 3 шагами (ShowSpore -> ShowCell -> Done), запускается на L1-L3 при первом входе
- HighlightOverlay — затемнение экрана с прозрачным "окном" вокруг target (4-rect cutout, pulse ring внутри)
- HandPointer — анимация руки от inventory item к target клетке (loop пока не stopped)
- TutorialCanvas в Game scene (sortingOrder 50, поверх UI, raycaster disabled чтоб не блокировать input)
- Hand sprite (палец вверх) генерируется автоматически
- PlayerPrefs spo_tutorial_completed_<idx> — туториал каждого уровня запускается один раз

## 2. Что изменилось с прошлой итерации
- GameController.Start вызывает TryStartTutorial если levelIdx <= 3 и не completed
- HandleDragBegin вызывает tutorialManager.OnDragStarted() (overlay переходит к cell)
- HandleDragEnd успешное placement вызывает tutorialManager.OnPlacementSucceeded() (marks completed)
- HandleDragEnd неудачное — обновляет overlay (туториал остаётся)
- Reset/Back/Pause/Menu останавливают tutorial. Resume пробует перезапустить

## 3. Editor скрипты
1. Tools -> SporosGame -> Iteration 9 -> Tutorial Setup (Iteration 9)

Скрипт:
- Создаёт hand_pointer.png в GeneratedSprites
- Создаёт TutorialCanvas в Game scene с HighlightOverlay + HandPointer + TutorialManager
- Прикручивает TutorialManager к GameController

## 4. Как тестировать
1. Edit -> Clear All PlayerPrefs (опционально, чтоб сбросить tutorial completion)
2. MainMenu -> Play -> Level 1 -> Game
3. Должна появиться затемнённая маска с прозрачным окном вокруг первой споры в inventory + cyan pulse ring
4. Hand pointer показывает drag motion от инвентаря к центральной клетке
5. Тапнуть spore -> overlay переключается к target клетке, hand pointer над ней
6. Drop spore на клетку -> tutorial исчезает, нормальная игра
7. Перезапуск L1 -> tutorial НЕ показывается (already completed)
8. L2, L3 -> tutorial показывается отдельно

## 5. Ожидаемый результат
- Tutorial никогда не блокирует input (только визуально направляет)
- При reset/pause/menu корректно скрывается
- После successful placement помечается completed, не повторяется
- На L4+ tutorial не запускается

## 6. Известные ограничения
- HighlightOverlay использует 4-rect cutout (rectangular подсветка), не настоящий vector mask
- Target клетка для туториала — центральная (или ближайшая non-Block если центр Block)
- Без текстовых подсказок (как в ТЗ)
- Hand pointer статический sprite, движется по линейной траектории

## 7. Запомнено для будущих итераций
- TutorialCanvas всегда поверх UI (sortingOrder 50), raycaster disabled чтоб не блокировать
- HighlightOverlay через 4 rect-маски простой и совместимый подход (vs shader mask)

## 8. Что в следующей итерации
Iteration 10 — финальная полировка: ambient drone music, idle pulsations везде, micro-shake на мелких действиях, финальная проверка
