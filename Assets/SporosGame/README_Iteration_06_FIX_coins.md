# SporosGame — Iteration 6 FIX (coins not awarded)

## Что починено
В `RebuildWinPopup` editor скрипта использовался `Object.FindObjectOfType<WinPopup>()` — он ищет **только в активных** объектах сцены. WinPopup при загрузке сцены **выключен** (`SetActive(false)`) → метод возвращал null → `return` без ошибки → CoinReward/CoinFlyFx не создавались, ссылки в WinPopup оставались пустыми → монеты не выдавались, +N текст не показывался.

**Исправлено двумя путями:**
1. **Editor** — теперь использует `Resources.FindObjectsOfTypeAll<WinPopup>()` (находит disabled) + фильтр по принадлежности к сцене. Добавлены Debug.LogWarning если не найдено
2. **WinPopup runtime** — даже если ссылки на coinFlyEffect/coinFlySource/coinTarget пусты, `CurrencyManager.AddCoins(coinsEarned)` всё равно вызывается (fallback без анимации). Так что монеты гарантированно засчитываются

## Файлы для замены
- `Assets/SporosGame/Editor/Iteration06_Setup.cs`
- `Assets/SporosGame/Scripts/UI/WinPopup.cs`

## Что сделать
1. Распаковать архив (перезаписать 2 файла)
2. Дождаться компиляции
3. `Tools → SporosGame → Iteration 6 → Star Rating + Currency (Iteration 6)` (запустить заново)
4. Play

## Запомнено для будущих итераций
- В editor скриптах для поиска объектов которые могут быть disabled (попапы, выключенные UI элементы) использовать `Resources.FindObjectsOfTypeAll<T>()` + фильтр `obj.gameObject.scene == scene`. **Никогда `Object.FindObjectOfType<T>()` для попапов** — они всегда disabled на старте сцены
- Все награды/прогрессии должны иметь fallback-путь без анимации. Если visual fx ссылки пусты — логика всё равно выполняется
