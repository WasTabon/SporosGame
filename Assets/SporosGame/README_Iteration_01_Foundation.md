# SporosGame — Iteration 1: Foundation

## 1. Что добавлено
Базовые core-системы: SoundManager (процедурные синусоиды), HapticManager, TransitionManager (fade между сценами), SafeAreaFitter, PopupBase, ButtonAnimator. Три сцены: MainMenu, LevelSelect, Game с переходами между ними.

## 2. Что изменилось с прошлой итерации
Первая итерация — изменений нет.

## 3. Editor скрипты — порядок запуска

**Pre-requisite:** установить **DOTween Free** (Asset Store → My Assets → DOTween → Import). После импорта запустить DOTween Utility Setup Panel (Tools → Demigiant → DOTween Utility Panel → Setup DOTween).

Затем:
1. `Tools → SporosGame → Iteration 1 → Setup All Scenes`

Если хочется по одной:
1. `Tools → SporosGame → Iteration 1 → Setup MainMenu Scene`
2. `Tools → SporosGame → Iteration 1 → Setup LevelSelect Scene`
3. `Tools → SporosGame → Iteration 1 → Setup Game Scene`
4. `Tools → SporosGame → Iteration 1 → Add Scenes To Build Settings`

## 4. Как тестировать
1. Открыть сцену `Assets/SporosGame/Scenes/MainMenu.unity`
2. Нажать Play
3. Потыкать кнопки: Play → LevelSelect → Level 1 → Game → Back. Также Settings и Shop попапы

## 5. Ожидаемый результат
- На старте видно тёмно-синий фон с глоу-пятнами и логотип "SPOROS" с пульсацией
- Кнопки имеют scale punch на нажатие + click sound (синусоида) + лёгкая вибрация на мобильном
- Тап Play → плавный fade-переход на LevelSelect
- В LevelSelect одна гекс-кнопка с цифрой "1" по центру — тап → Game scene
- В Game — placeholder текст и Back кнопка
- Settings/Shop попапы открываются с scale 0→1 OutBack, бэкдроп fade, закрываются по кнопке Close или клику по бэкдропу
- Safe area работает (на iPhone с notch верх не залезает под челку)

## 6. Известные ограничения
- Звуки процедурные (синусоиды) — финальные пока не подобраны
- Шрифт TMP default (LiberationSans) — без кастома
- Background music ещё не добавлена (будет в Iteration 10)
- Все спрайты генерируются процедурно в Editor (circle/rounded-rect/hex), сохраняются в кеше скрипта — после Setup в сцене они станут permanent

## 7. Что в следующей итерации
Iteration 2 — игровое поле (Grid), базовая спора с drag&drop, распространение лучей в 4 направления с анимацией, активация клеток, inventory панель снизу.
