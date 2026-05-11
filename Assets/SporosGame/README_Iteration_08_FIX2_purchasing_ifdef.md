# SporosGame — Iteration 8 FIX 2 (remove #if UNITY_PURCHASING)

## Что починено
Compile errors:
```
'IAPManager' does not contain a definition for 'HandlePurchaseComplete'
'IAPManager' does not contain a definition for 'HandlePurchaseFailed'
```

Причина: символ `UNITY_PURCHASING` не определён автоматически в твоей версии IAP package (5.x), но namespace `UnityEngine.Purchasing` доступен. Из-за этого блок `#if UNITY_PURCHASING ... #endif` в IAPManager.cs не компилировался → методы отсутствовали → ShopPopup на них ссылался → ошибки.

## Решение
Убрал все `#if UNITY_PURCHASING ... #endif` обёртки. Теперь IAP — hard dependency, без него не скомпилируется. Раз пакет установлен — не проблема.

## Файлы для замены
- `Assets/SporosGame/Scripts/Managers/IAPManager.cs`
- `Assets/SporosGame/Scripts/UI/ShopPopup.cs`
- `Assets/SporosGame/Scripts/Scenes/MainMenuController.cs`

## Что сделать
1. Распаковать → перезаписать 3 файла
2. Дождаться компиляции (ошибки должны исчезнуть)
3. Если editor был перезапущен — заново настроить IAP Button (вручную, как договорились)

## Запомнено для будущих итераций
- Символ `UNITY_PURCHASING` не определён автоматически в Unity IAP 5.x — не использовать `#if UNITY_PURCHASING` без явного добавления в Project Settings → Scripting Define Symbols
- В будущем IAP code писать без `#if` обёрток — раз пакет установлен это hard dependency
