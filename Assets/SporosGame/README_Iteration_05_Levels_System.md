# SporosGame — Iteration 5: Levels system + progression

## 1. Что добавлено
- **LevelData** ScriptableObject — описывает уровень (size, cells, spores, isExtraPack)
- **LevelDatabase** ScriptableObject — массив 20 LevelData, хранится в `Assets/SporosGame/Resources/LevelDatabase.asset`
- **LevelManager** static — API: GetLevel, GetStars/SetStars, IsUnlocked, IsExtraPackUnlocked, CurrentLevel, AdvanceLevel
- **20 уровней** захардкожены (Normal cells): 3x3 → 7x7, прогрессивная сложность spore count
- **LevelButton** — UI компонент: number, lock icon, 3 star sockets, extra badge (для it08)
- **LevelSelect scene** полностью переделана — ScrollView с GridLayoutGroup 4 колонки, 20 кнопок
- **Прогресс уровней** — PlayerPrefs ключи `spo_level_stars_<idx>`
- **Unlock логика** — следующий уровень открыт если предыдущий пройден ≥1 звезда

## 2. Что изменилось с прошлой итерации
- LevelConfig теперь читает LevelData через LevelManager (старый CreateByIndex с захардкоженными 3 уровнями удалён)
- GameController на WIN сохраняет звёзды через LevelManager.SetStars()
- LevelSelectController полностью переделан
- LevelProgress сохранён как тонкий обёрточный класс для совместимости (deprecated, использовать LevelManager напрямую)

## 3. Editor скрипты — порядок запуска
1. `Tools → SporosGame → Iteration 5 → Build LevelDatabase + LevelSelect (Iteration 5)`

(остальные iteration setup не запускать)

## 4. Как тестировать
1. MainMenu → Play → LevelSelect
2. Видна сетка из 20 hex-кнопок. Только Level 1 разблокирован (cyan), остальные locked (серые с замком)
3. Тап на Level 1 → Game с 3x3 полем
4. Пройти уровень → WIN → звёзды сохраняются → возврат через Menu в LevelSelect
5. Level 2 теперь разблокирован
6. Тап на locked уровень → shake кнопки + fail sound, не открывается
7. Прогресс сохраняется между сессиями (PlayerPrefs)

## 5. Ожидаемый результат
- LevelSelect показывает scrollable grid 4xN кнопок-сот
- Каждая кнопка имеет: номер, 3 звезды снизу (заполнены/пустые), lock icon если заблокирована
- Тап на разблокированную → переход с fade
- Тап на locked → горизонтальный shake + fail sound + warning haptic
- На WIN записываются 3 звезды (реальная формула в it06)

## 6. Известные ограничения
- Все уровни — Normal cells (Block/Fixed/Limited в it07)
- Mutated споры в уровнях ещё нет (it07)
- Звёзды всегда 3 при WIN (реальная формула в it06)
- LevelData в Editor inspector — default Unity view (можно редактировать вручную: открыть `Assets/SporosGame/Data/Level_XX.asset` → менять width/height/rows/spores)
- LevelSelect grid: 4 кнопки в ряд, при portrait это даёт ~30 кнопок видимых без проблем

## 7. Что в следующей итерации
Iteration 6 — реальная star rating system (время + кол-во использованных спор), currency (монеты) за звёзды, top bar с coin counter, анимация прилёта монет.
