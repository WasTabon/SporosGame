# SporosGame — Iteration 11: Daily Reward + Streak

## 1. Что добавлено
- DailyRewardManager — статический менеджер streak'а. PlayerPrefs: spo_daily_last_claim_date (yyyyMMdd UTC), spo_daily_streak
- Таблица наград: Day 1=10, Day 2=15, Day 3=20, Day 4=30, Day 5=40, Day 6=60, Day 7=150 (jackpot). После Day 7 цикл начинается заново
- DailyRewardPopup — попап с 7 day-boxes, claim кнопкой, авто-показывается на MainMenu при первом запуске за день
- DayBoxView — визуал отдельной коробки дня: future (серая), current (cyan/magenta jackpot + pulse ring + scale 1.1), claimed (green + checkmark)
- MainMenuController.TryShowDailyReward — auto-show после 0.6s delay при загрузке
- Editor: меню сброса для тестирования (Tools -> SporosGame -> Iteration 11 -> Reset Daily Reward)

## 2. Streak логика
- Сегодня уже забрал: попап не показывается
- Вчера забрал: streak + 1 (если был 7 -> возврат на 1)
- 2+ дней пропустил: streak сбрасывается на 1
- Никогда не забирал: streak = 1

## 3. Editor скрипты — порядок запуска
1. Tools -> SporosGame -> Iteration 11 -> Daily Reward Setup (Iteration 11)
2. (Опционально для теста) Tools -> SporosGame -> Iteration 11 -> Reset Daily Reward

## 4. Как тестировать
1. Запусти editor скрипт setup
2. Play -> MainMenu
3. Через 0.6s появляется попап Daily Reward
4. Day 1 подсвечен cyan, pulse ring, scale 1.1
5. Тап Claim -> монеты летят в HUD counter, +10 монет добавлено
6. Попап закрывается через 1.2s
7. Возврат в MainMenu -> попап не появляется (already claimed today)
8. Reset Daily Reward через editor menu -> снова показывается с Day 2 если пройти на следующий день, или с Day 1 если сброс

## 5. Симуляция следующего дня для теста
PlayerPrefs key `spo_daily_last_claim_date` хранит дату формата yyyyMMdd UTC.
Чтобы симулировать "следующий день":
- Edit -> Preferences -> ... либо через PlayerPrefs viewer редактор
- Изменить значение `spo_daily_last_claim_date` на дату ВЧЕРА (например если сегодня 20260518, поменять на 20260517)
- Перезайти в MainMenu -> попап покажет Day 2 (next in streak)

## 6. Ожидаемый результат
- Player первый запуск каждый день -> попап с reward
- Streak награды растут (10 -> 15 -> 20 -> 30 -> 40 -> 60 -> 150)
- День 7 jackpot выглядит особенно (magenta цвет вместо cyan)
- Если пропуск 2+ дней -> reset на Day 1, начало нового streak

## 7. Известные ограничения
- Время по UTC, не по локали игрока. На границе часовых поясов может быть небольшая разница
- Day boundary считается серверного UTC дня. Игроки могут "читить" меняя системное время устройства
- Чтобы защитить от чита нужен server-based check (не реализовано, можно добавить через Unity Cloud Save в продакшен)

## 8. Запомнено для будущих итераций
- DailyRewardManager использует UTC даты для согласованности
- Auto-show через DOVirtual.DelayedCall(0.6f, ...).SetUpdate(true) после scene load (timing accommodation для fade transition)
- DayBoxView состояния (Future/Current/Claimed) через enum + visual swap, чистый pattern для будущих "карточных" UI

## 9. Что в следующей итерации
Iteration 12 — Achievements: 10-15 простых awards (first 3-star, 5 levels no undo, 100 coins accumulated, L20 passed). Popup при unlock. PlayerPrefs storage, проверки в существующих callback'ах
