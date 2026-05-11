# SporosGame — Iteration 4: Polish core mechanic

## 1. Что добавлено
- **ParticleBurst** — ручные частицы (10 точек разлёт за 0.55s) при активации клетки и при placement споры
- **RingExpand** — кольцо-ударная волна при placement (растёт от 0 до 2.2x, fade)
- **ScreenShake** — таргетится на Grid transform (НЕ камера — чтоб background не дрожал). Тихий shake при placement, сильный при WIN
- **EffectsManager** singleton (DontDestroyOnLoad) с prefab ссылками на burst + ring
- **Spore idle pulse** — после placement спора плавно "дышит" scale yoyo 1→1.06
- **RaySegment head particle** — яркая точка движется впереди роста луча
- **Haptics дополнительно** — Warning haptic на неверный drop, Failure haptic на LOSE, Success на WIN

## 2. Что изменилось с прошлой итерации
- Cell.Activate спавнит burst в дополнение к pop sound
- Spore.PlaceAndEmit спавнит ring + burst + screen shake + после всех лучей запускает idle pulse
- RaySegment имеет 3й SpriteRenderer (Head) — head particle прокидывается через editor
- GameController устанавливает `ScreenShake.SetTarget(grid.transform)` в Start
- На win — большой screen shake перед показом попапа
- Создан Ring sprite (PNG в GeneratedSprites/ring.png)

## 3. Editor скрипты
1. `Tools → SporosGame → Iteration 4 → Update Effects + Polish (Iteration 4)`

(Не запускать Iteration 1/2/3 setup заново)

## 4. Как тестировать
1. MainMenu → Play → Level 1 → Game
2. Drag спору на клетку:
   - Видишь: ring expand, particle burst в цвет споры, лёгкий рывок поля, medium haptic
3. Лучи летят с яркой "головой" впереди
4. Каждая активированная клетка: pop звук + burst частиц cyan
5. После активации всех лучей — спора начинает "дышать"
6. Все клетки заполнены → большой shake поля → success haptic → через 0.55s WinPopup
7. Тащишь спору на занятую/неверную клетку → отпускаешь → fail sound + warning haptic

## 5. Ожидаемый результат
- Каждое взаимодействие "сочное": haptics + sound + visual effect одновременно
- Поле дрожит при placement (subtle) и при win (heavy)
- Background и UI остаются стабильны (не дрожат)
- Споры на поле живые — постоянно мягко пульсируют
- Лучи имеют яркие head-точки которые "тащат" свет от клетки к клетке

## 6. Известные ограничения
- Particle pool не реализован — каждый burst создаёт GameObjects и уничтожает (для мобайла на текущей частоте burst-ов это ок)
- Background ambient music нет (it10)
- Звёзды в WinPopup всё ещё всегда 3 (рейтинг в it06)

## 7. Что в следующей итерации
Iteration 5 — система уровней через LevelData ScriptableObject, LevelManager, 20 захардкоженных уровней разной сложности, LevelSelect grid со звёздами и progress.
