# SporosGame — Iteration 6: Star Rating + Currency

## 1. Что добавлено
- **CurrencyManager** static — `Coins`, `AddCoins`, `SpendCoins`, event `OnCoinsChanged(old,new)`
- **LevelData** расширен: `minSporesForThreeStars`, `maxSporesForOneStar`, `timeForThreeStars`, `timeForOneStar`, `coinsReward`
- **StarCalculator** — реальный расчёт 1-3 звезды (среднее между spore-score и time-score)
- **LevelManager.AwardCoinsForLevel** — выдаёт только delta монет (защита от фарма перепрохождением)
- **CoinCounter UI** — animated TMP counter с DOTween, scale punch при изменении
- **CoinFlyEffect** — N монет летят по дуге Bezier от source к target, stagger 0.07s
- **WinPopup** показывает coin reward (+N) и запускает coin fly после анимации звёзд
- **TopBar coin display** в MainMenu (под логотипом), LevelSelect (top-right), Game (под HUD top bar)
- Coin sprite (золотой кружок с тёмным outline)

## 2. Что изменилось с прошлой итерации
- LevelData получил поля thresholds + reward
- WinPopup переделан: `ShowWithStars(stars)` → `ShowWithResults(stars, coins, target)`
- GameController трекает `sporesUsed`, на WIN вычисляет реальные звёзды + reward
- В каждой сцене (Game/MainMenu/LevelSelect) появился CoinCounter
- LevelManager: новый ключ `spo_level_coins_awarded_<idx>` — сколько монет уже выдано за уровень. При повышении звёзд выдаётся только разница

## 3. Editor скрипты
1. `Tools → SporosGame → Iteration 6 → Star Rating + Currency (Iteration 6)`

Скрипт:
- Создаёт coin sprite + CoinIcon prefab
- Обновляет thresholds во всех LevelData (формула на основе size)
- Добавляет CoinCounter в Game/MainMenu/LevelSelect
- Перестраивает WinPopup: добавляет coin reward панель + CoinFlyFx контейнер

## 4. Как тестировать
1. MainMenu → видна панель с монетами (0) под логотипом
2. Play → LevelSelect → видна панель с монетами в углу
3. Тап Level 1 → Game → видна панель с монетами в HUD
4. Пройти уровень быстро с минимумом спор → WIN → 3 звезды + полный reward (+X монет летят к counter)
5. Пройти медленно или с лишними спорами → 1-2 звезды + меньше монет
6. Counter в HUD анимируется (0→X с пульсацией иконки)
7. Вернуться в MainMenu — счётчик монет уже обновлён
8. Перепройти тот же уровень с лучшим результатом → выдаётся только delta монет
9. Перепройти с худшим — звёзды не понижаются, монеты не выдаются (level "max-keeping")

## 5. Ожидаемый результат
- При WIN с задержкой ~0.95s после открытия попапа летят монеты (Bezier по дуге)
- Каждая монета издаёт pop sound при приземлении в counter
- Counter в HUD анимируется плавно (DOTween) + scale punch иконки
- Между сценами Coins значение сохраняется (PlayerPrefs)
- Lose не даёт монет, не записывает звёзды

## 6. Известные ограничения
- Звёзды считаются от времени и кол-ва спор — иногда давать 3* очень легко если уровень с запасом спор. Балансировка через inspector (отредактировать `Level_XX.asset` → minSporesForThreeStars и timeForThreeStars)
- Coin animation использует Random в Bezier control point — каждая летит немного по-разному (это эффект, не баг)
- Settings/Shop попапы пока не работают с currency (это в it08 — Shop with IAP)

## 7. Что в следующей итерации
Iteration 7 — спец клетки (Block, Fixed, Limited), Diagonal mutated спора, полный пересмотр dataset 20 уровней с настоящим разнообразием и сложностью.
