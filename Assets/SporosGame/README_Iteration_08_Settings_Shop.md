# SporosGame — Iteration 8: Settings + Shop + Extra Levels

## 1. Что добавлено
- IAPManager — статический обработчик: HandlePurchaseComplete, HandlePurchaseFailed, HandleProductFetched, UnlockExtraPack, event OnExtraPackUnlocked
- SettingsPopup — popup с SFX/Music sliders, Haptics toggle, Restore Purchases, Close
- ShopPopup — popup с превью 10 extra уровней, IAPButton (productID com.levelpack.inapp, NonConsumable), price text auto-updates via onProductFetched, Owned label вместо кнопки если уже куплено
- SliderRow — переиспользуемый UI компонент для volume sliders
- 10 extra LevelData: idx 21-30, isExtraPack=true, сложные layouts. Балансировку запускать через Iteration 7 solver
- MainMenuController — подключает Settings/Shop попапы, при Restore вызывает Apple RestoreTransactions через CodelessIAPStoreListener

## 2. Editor скрипты — порядок запуска
1. Unity IAP package уже установлен (5.2.1) — ОК
2. Tools -> SporosGame -> Iteration 8 -> Settings + Shop + Extra Levels (Iteration 8)
3. После — Tools -> SporosGame -> Iteration 7 -> Auto-Solve and Balance Levels чтобы сбалансировать spore counts для L21-30

## 3. Настройка IAP Catalog (вручную)
1. Services -> In-App Purchasing -> IAP Catalog
2. Добавить продукт: Product ID com.levelpack.inapp, Type Non Consumable
3. Editor скрипт уже подключил handlers к IAPButton:
   - On Purchase Complete -> ShopPopup.OnPurchaseCompleted
   - On Purchase Failed -> ShopPopup.OnPurchaseFailedEvent
   - On Product Fetched -> ShopPopup.OnProductFetched
   Проверить в Inspector что они там

## 4. Как тестировать
1. MainMenu -> шестерёнка -> SettingsPopup. Слайдеры меняют громкость, toggle — haptics
2. MainMenu -> звезда -> ShopPopup, видны 10 превью L21-30
3. Цена placeholder "$0.99" -> при загрузке product info обновится на локализованную
4. Buy кнопка зелёная UNLOCK -> IAP flow
5. После успешной покупки -> ShopPopup.OnPurchaseCompleted -> IAPManager.UnlockExtraPack -> PlayerPrefs spo_extra_unlocked=1, попап обновляется на OWNED
6. В LevelSelect L21-30 разблокированы (magenta extraBadge)
7. Restore Purchases -> Apple RestoreTransactions

## 5. Известные ограничения
- IAP catalog настраивается вручную через Services UI
- В Unity editor без реальных credentials IAP покупка не работает. Тестировать на устройстве
- L21-30 без балансировки будут иметь дефолтные spore counts — запустить Iteration 7 solver
- ShopPopup.OnProductFetched при первом открытии может вызваться позже чем popup показан — price text обновится позже

## 6. Запомнено для будущих итераций
- IAP code в #if UNITY_PURCHASING ... #endif — проект компилируется и без пакета
- IAPButton handlers подключаются через UnityEventTools.AddPersistentListener — сохраняется в .unity файле как persistent reference

## 7. Что в следующей итерации
Iteration 9 — туториал на первых 3 уровнях через подсветку (highlight inventory -> highlight cell -> hand pointer drag), без текста
