# SporosGame — Iteration 2 FIX 2 (hex tiling)

## Что починено
1. **Клетки залазили друг в друга** — hex sprite перегенерируется как pointy-top с правильной aspect ratio (256x296), без прозрачных полей; pixelsPerUnit подгоняется так что sprite ровно 1 unit по ширине; GridSystem масштабирует клетку до `cellSize`; spacingX = cellSize, spacingY = cellSize * 0.866 (sqrt(3)/2) — клетки касаются плоскими гранями без overlap
2. **Spore слишком крупная** — теперь scale спора привязан к cellSize (* 0.45), помещается внутрь клетки
3. **Hex sprite принудительно пересоздаётся** при запуске editor скрипта (старый удаляется), чтоб старая текстура с overlap не использовалась

## Файлы для замены
- `Assets/SporosGame/Scripts/Gameplay/Cell.cs`
- `Assets/SporosGame/Scripts/Gameplay/GridSystem.cs`
- `Assets/SporosGame/Scripts/Gameplay/Spore.cs`
- `Assets/SporosGame/Scripts/Gameplay/GameController.cs`
- `Assets/SporosGame/Editor/Iteration02_Setup.cs`

## Что сделать
1. Распаковать архив (перезапись 5 файлов)
2. Дождаться компиляции
3. `Tools → SporosGame → Iteration 2 → Setup Game Scene (Iteration 2)`
4. Play

## Известное
- Если кажется что клетки слишком мелкие — можно увеличить `cellSize` в Grid компоненте Inspector (сейчас 1.5)
- Spore scale = `cellSize * 0.45` (если хочется крупнее/мельче — поменяй в GameController.HandleDragBegin)
