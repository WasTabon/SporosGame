# SporosGame — Iteration 2 FIX

## Что починено
1. **Cell scale (0,0,0)** — Cell.Init теперь принудительно ставит `transform.localScale = Vector3.one` перед сохранением baseScale; GridSystem.Build также форсит scale при Instantiate; Cell prefab создаётся с явным `localScale = Vector3.one`
2. **Background перекрывал поле** — Game scene теперь использует **отдельный BackgroundCanvas** (Render Mode = Screen Space - Camera, plane distance 80, sortingOrder = -100). Game UI canvas (HUD + Inventory) — Overlay sortingOrder = 10. World camera рендерит grid между ними
3. **cellSize: 1.6 → 1.5** (как ты просил)
4. **Cell.Reset() → Cell.ResetState()** — переименовал чтоб не конфликтовало с MonoBehaviour.Reset() (вызывается editor'ом)

## Файлы для замены
- `Assets/SporosGame/Scripts/Gameplay/Cell.cs`
- `Assets/SporosGame/Scripts/Gameplay/GridSystem.cs`
- `Assets/SporosGame/Editor/Iteration02_Setup.cs`

## Что сделать
1. Распаковать архив в проект (перезаписать 3 файла выше)
2. Дождаться компиляции
3. `Tools → SporosGame → Iteration 2 → Setup Game Scene (Iteration 2)` — пересоберёт Game scene и Cell prefab
4. Play

## Запомнено для будущих итераций
- При размерах поля больше экрана нужен **swipe-panning** камеры (вид сверху, как в 2D-играх). Реализовать когда поле выйдет за рамки экрана (4x4+).
