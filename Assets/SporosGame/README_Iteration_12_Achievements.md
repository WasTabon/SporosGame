# SporosGame — Iteration 12: Achievements

## 1. Что добавлено
- AchievementsManager (static) — список 12 ачивок захардкожен. PlayerPrefs storage. Hook методы для проверки из существующих callbacks
- AchievementDef — простой класс (id, title, description, target, reward, type, color)
- AchievementUnlockedPopup — snackbar внизу экрана при разблокировке (slide-up + hold 2.4s + slide-down). Queue для нескольких unlocks подряд
- AchievementsPopup — полный экран всех ачивок с прокруткой, прогресс-барами, наградами
- AchievementRow — отдельная строка списка (icon + title + desc + progress bar + reward)
- Кнопка Achievements в MainMenu (trophy icon, top-left)
- AchievementUnlockedPopup есть и в MainMenu и в Game scene — ачивки могут срабатывать во время игры
- CurrencyManager.AddCoinsWithoutTracking — для предотвращения рекурсии при выдаче coin reward от ачивок

## 2. Список 12 ачивок
| ID | Title | Target | Reward | Type |
|----|-------|--------|--------|------|
| first_win | First Steps | 1 win | 20 | Once |
| first_3star | Perfectionist | 1x 3-star | 30 | Once |
| complete_l5 | Pioneer | L5 done | 30 | Once |
| complete_l10 | Explorer | L10 done | 50 | Once |
| complete_l20 | Champion | L20 done | 100 | Once magenta |
| complete_l30 | Legend | L30 done | 200 | Once magenta |
| no_undo_5 | Decisive | 5 levels no undo | 50 | Progressive |
| 3star_10 | Star Hunter | 3-star 10 levels | 80 | Progressive magenta |
| coins_100 | Saver | 100 coins earned total | 30 | Progressive |
| coins_500 | Wealthy | 500 coins earned total | 100 | Progressive magenta |
| daily_3 | Loyal | 3 daily rewards | 50 | Progressive |
| extra_pack | Supporter | IAP purchased | 50 | Once gold |

## 3. Hooks
- GameController.OnSporeResolved -> AchievementsManager.OnLevelCompleted(idx, stars, usedUndo) при WIN
- CurrencyManager.AddCoins -> AchievementsManager.OnCoinsEarned(amount)
- DailyRewardPopup.OnClaimed -> MainMenu -> AchievementsManager.OnDailyClaim
- ShopPopup.OnPurchaseSuccess -> MainMenu -> AchievementsManager.OnExtraPackPurchased
- usedUndoThisLevel флаг в GameController сбрасывается в Start, ставится в HandleUndo

## 4. Editor скрипты
1. Tools -> SporosGame -> Iteration 12 -> Achievements Setup (Iteration 12)
2. (Опц) Tools -> SporosGame -> Iteration 12 -> Reset All Achievements

Скрипт:
- Генерирует trophy.png sprite
- Создаёт AchievementRow.prefab
- В MainMenu: AchievementUnlockedPopup, AchievementsPopup, кнопка Achievements (top-left)
- В Game scene: AchievementUnlockedPopup (для unlocks во время игры)

## 5. Как тестировать
1. Tools -> SporosGame -> Iteration 12 -> Achievements Setup (Iteration 12)
2. Edit -> Clear All PlayerPrefs (для свежего прогресса) ИЛИ Tools -> Reset All Achievements
3. Play -> Level 1 -> пройти -> snackbar "First Steps +20" внизу экрана
4. Если 3 звезды -> второй snackbar "Perfectionist +30"
5. Перейти в MainMenu -> тап trophy icon (top-left) -> попап со всеми 12 ачивками
6. Progressive ачивки показывают X / target и прогресс-бар
7. Купить extra pack в Shop -> snackbar "Supporter +50"
8. Накопить 100 монет (играя) -> "Saver +30"

## 6. Ожидаемый результат
- При разблокировке: 3-секундный snackbar slide-up снизу
- Если несколько unlock-ов подряд: показываются последовательно (queue)
- Coin reward автоматически добавляется без рекурсии (AddCoinsWithoutTracking)
- Полный список в MainMenu кнопкой с trophy иконкой

## 7. Известные ограничения
- Старые игроки с накопленными coins не получат ачивку coins_100/500 автоматически - tracker считает только новые поступления после установки it12
- "no_undo" check работает per-level: усдоn хотя бы раз -> false. Бьёт только если ВСЕ 5 уровней подряд без undo
- "complete_lX" срабатывает только при WIN на конкретном уровне idx, не "достигнут или превзойдён"
- Все ачивки захардкожены - для новых нужно править C# AchievementsManager
- Нет сохранения старых coin-balance как "earned" - tracker стартует с 0

## 8. Запомнено для будущих итераций
- CurrencyManager.AddCoinsWithoutTracking для выдачи rewards от ачивок (избегает рекурсии)
- AchievementsManager.OnCoinsEarned трекает total earned отдельно от current balance
- AchievementUnlockedPopup использует queue для multiple unlocks
- Hook pattern: managers статические, hooks вызываются из существующих callback-ов в нужных точках
