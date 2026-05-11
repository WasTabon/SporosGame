# SporosGame — Iteration 10: Final Polish

## 1. Что добавлено
- **AmbientMusic** — процедурный drone (3 sine осцилляторов 80/120/160 Hz + 3 detune copies, slow LFO breathing 0.1 Hz, 30s seamless loop). DontDestroyOnLoad. Громкость через SoundManager.GetMusicVolume
- **GameBootstrap** spawns AmbientMusic при первой загрузке сцены
- **Cell idle breathing** — Normal inactive клетки тихо дышат alpha 1 → 0.85 (2.5s yoyo, random delay 0-2.5s чтобы не синхронно)
- **Spore glow rotation** — placed spore медленно вращает glow (360° за 8s, linear loop)
- **ScreenShake.MicroShake()** — лёгкий shake 0.04 / 0.1s для мелких событий
- **GameController.HandleUndo** — micro-shake при undo
- **LevelButton sparkle** — 3-star уровни в LevelSelect мягко пульсируют scale (1.0 → 1.04, 1.2s yoyo, random delay)
- **ParticleBurst** — 10 → 12 частиц, duration 0.55s → 0.65s

## 2. Что изменилось с прошлой итерации
- GameBootstrap создаёт AmbientMusic в дополнение к остальным managers
- Cell получил idleBreathTween — запускается в Init для Normal type, останавливается при Activate / MarkOccupied, перезапускается при ForceSetState(Inactive)
- Spore.PlaceAndEmit запускает StartGlowRotation после всех лучей
- ScreenShake получил MicroShake helper

## 3. Editor скрипты
1. Tools -> SporosGame -> Iteration 10 -> Final Polish (Iteration 10)

Скрипт обновляет только ParticleBurst prefab. Остальные изменения чисто кодовые — применятся при компиляции.

## 4. Как тестировать
1. Запусти любую сцену (MainMenu / LevelSelect / Game) — должен играть тихий ambient drone
2. В Settings (MainMenu) - попробуй music slider, drone тише/громче
3. Game scene - неактивные клетки тихо дышат (subtle alpha pulsation), у каждой свой фазовый сдвиг
4. Поставь спору - после placement spore медленно вращает glow
5. Поставь и сделай Undo - ощущение лёгкого тык-shake поля
6. Пройди уровень на 3 звезды, открой LevelSelect - кнопка этого уровня тихо пульсирует scale
7. Активация клеток - чуть больше частиц чем раньше

## 5. Ожидаемый результат
- Игра ощущается живой и атмосферной
- Ничто на экране не статично - всё мягко дышит / пульсирует / вращается
- Каждое взаимодействие подкреплено feedback (sound + haptic + visual + shake)
- Музыка создаёт relaxing biological vibe

## 6. Финальный checklist для production
- [x] Главное меню работает
- [x] LevelSelect показывает 30 уровней (20 base + 10 extra)
- [x] Game scene с полем, drag&drop, лучами, активацией
- [x] Win/Lose/Pause/Settings/Shop попапы
- [x] Undo, Reset, переход на следующий уровень
- [x] Звёзды по времени и кол-ву спор, монеты
- [x] Block / Fixed / Limited клетки, Diagonal спора
- [x] Туториал на L1-L3
- [x] IAP интеграция (com.levelpack.inapp)
- [x] Ambient music, idle pulses, screen shake, haptics, particle effects
- [x] Safe area handling, 60 FPS target

## 7. Известные ограничения
- Все sprites placeholder (генерированные). Перед продакшеном заменить на финальные ассеты
- Шрифт TMP default (LiberationSans). Можно заменить через TMP settings
- IAP catalog требует ручной настройки в Services UI
- L21-L30 layouts требуют тонкой балансировки - запустить solver после ручных правок layout

## 8. Запомнено для будущих итераций
- AmbientMusic создаётся через GameBootstrap синглтон pattern (DontDestroyOnLoad)
- Cell.idleBreathTween запускается с random delay чтобы не синхронно
- 3-star sparkle - subtle scale pulse, не отвлекает
- Когда Cell состояние меняется через ForceSetState - все idle tweens нужно corректно перезапускать (это уже работает)
