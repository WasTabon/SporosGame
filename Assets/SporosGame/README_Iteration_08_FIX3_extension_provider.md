# SporosGame — Iteration 8 FIX 3 (remove ExtensionProvider call)

## Что починено
Compile error:
```
'CodelessIAPStoreListener' does not contain a definition for 'ExtensionProvider'
```

В Unity IAP 5.x API изменился — `ExtensionProvider` больше не доступен на CodelessIAPStoreListener (или переименован).

## Решение
Убрал ручной вызов RestoreTransactions из MainMenuController. **Restore теперь делается через отдельный IAP Button** с `Button Type = Restore` который ты привязываешь вручную к Restore кнопке (так же как Purchase кнопку).

Кнопки `restoreButton` в SettingsPopup и ShopPopup существуют как UI элементы — на них ты вешаешь второй IAPButton/CodelessIAPButton компонент с Button Type = Restore через Add Component.

## Файлы для замены
- `Assets/SporosGame/Scripts/Scenes/MainMenuController.cs`
- `Assets/SporosGame/Scripts/UI/SettingsPopup.cs`
- `Assets/SporosGame/Scripts/UI/ShopPopup.cs`

## Что сделать
1. Распаковать → перезаписать 3 файла
2. Дождаться компиляции (должна пройти)
3. (Опционально) Если хочешь Restore Purchases работающий:
   - В MainMenu → ShopPopup → Content → RestoreButton → Add Component → IAP Button
   - Set Button Type = `Restore`
   - Не нужен Product ID (Restore не привязан к одному продукту)
   - Аналогично для SettingsPopup → Content → RestoreButton если хочешь Restore оттуда тоже
4. Сам Purchase IAP Button на BuyButton привязываешь как договорились раньше

## Запомнено для будущих итераций
- Не использовать `CodelessIAPStoreListener.ExtensionProvider` в IAP 5.x — API изменён
- Restore Purchases в IAP 5.x делается через IAP Button с Button Type = Restore (не программно)
- Любые ручные вызовы Apple/Google extensions из C# в IAP 5.x требуют другого подхода — пока избегаем
