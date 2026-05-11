# SporosGame — Iteration 7: Special Cells + Mutated Spore + Levels Redesign

## 1. Что добавлено

### Спец клетки
- **Block** (тёмная клетка с X-mark): луч останавливается, клетка не активируется, не учитывается для победы
- **Fixed** (золотая обводка, idle пульсация): обязательна для активации. При активации — сильный glow, screen shake, heavy haptic
- **Limited** (оранжевая dashed обводка): активируется только один раз, после активации становится сплошной и **блокирует** дальнейшие лучи (как Block)

### Mutated спора
- **Diagonal** (зелёная): лучи по 4 диагоналям вместо 4 ортогональных направлений

### Полный пересмотр 20 уровней
- **L1-3**: туториал, Normal 3x3 / 4x3
- **L4-7**: вводятся Block клетки (одна за одной)
- **L8-10**: вводятся Fixed клетки
- **L11-15**: вводятся Limited клетки, потом mixed конфигурации
- **L16**: сложный layout
- **L17-20**: Diagonal mutated спора + большие поля (5x5 → 7x7) со всеми типами клеток

## 2. Что изменилось с прошлой итерации
- **Cell.cs** — кардинально расширен: визуал для всех 4 типов клеток, методы `IsBlockingRay()`, `CanBeActivated()`, `CountsForWin()`
- **Spore.cs.EmitRay** — корректная блокировка: Block → break, Limited активируется и потом break
- **GridSystem.AreAllActivated** — через `CountsForWin()` (Block игнорируется)
- **LevelData** — поддерживает все CellType через rows
- Cell prefab пересоздаётся: добавлены 3 SpriteRenderer (BlockMark, FixedInner, LimitedOverlay)
- LevelDatabase полностью перестраивается с новыми specs (20 уровней)
- thresholds для звёзд автоматически пересчитываются: `playableCells = cells - blockCount`

## 3. Editor скрипты — порядок запуска
1. `Tools → SporosGame → Iteration 7 → Special Cells + Levels Redesign (Iteration 7)`

Скрипт:
- Генерирует sprites: `block_x.png`, `limited_dashes.png` в GeneratedSprites/
- Обновляет Cell.prefab (добавляет BlockMark, FixedInner, LimitedOverlay рендереры)
- Пересоздаёт все 20 LevelData с новыми specs
- Обновляет LevelDatabase

## 4. Как тестировать
1. **Сначала сбросить прогресс** (опционально): открыть PlayerPrefs (в редакторе: Edit → Clear All PlayerPrefs или через скрипт) — иначе старые звёзды унаследуются и доступ к уровням не нуждается в перепрохождении
2. MainMenu → Play → LevelSelect → видно 20 уровней
3. Level 1-3: пройти без проблем (туториал)
4. Level 4: 1 Block клетка в середине — луч на ней останавливается
5. Level 8: 2 Fixed клетки золотые — должны быть активированы для победы
6. Level 11: 1 Limited клетка оранжевая — активируется один раз, потом блокирует
7. Level 17: появляется 2-я спора в inventory зелёная (Diagonal) — лучи по диагоналям
8. Level 20: самый сложный — 4 Block, 4 Limited, 3 Fixed, mix спор

## 5. Ожидаемый результат
- Спецклетки визуально отличаются с первого взгляда
- При активации Fixed клетки — особенный feedback (большой shake + heavy haptic + золотой burst)
- Limited клетка переходит из dashed orange в solid orange после активации
- Diagonal спора визуально зелёная в inventory, лучи зелёные по диагоналям
- LevelSelect grid имеет реальное разнообразие — некоторые уровни заметно сложнее

## 6. Известные ограничения
- **Балансировка ручная**: я попытался спроектировать каждый уровень так, чтобы был видимый 3-star path, но некоторые могут оказаться слишком лёгкими или слишком сложными. Если конкретный уровень не работает — открой `Assets/SporosGame/Data/Level_XX.asset` и редактируй: рассыпь Block клетки, измени количество спор, поменяй thresholds (minSporesForThreeStars, timeForThreeStars)
- **Уровни не проверены автотестами** — возможно где-то нерешаемо (особенно L18-20 с диагоналями) — потребуется ручная проверка
- Для редактирования cell layout: открыть LevelData asset → раздел Rows → выбрать row → cells (массив CellType enum для каждой клетки в ряду)
- Старые звёзды от it05/it06 могут давать unfair access к новым уровням (это просто прогресс)

## 7. Что в следующей итерации
Iteration 8 — Settings popup (volume, haptics, restore), Shop popup с IAP placeholder (+10 extra levels), которые добавляются как extraPack в database.
