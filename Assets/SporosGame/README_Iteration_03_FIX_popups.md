# SporosGame — Iteration 3 FIX (popups visibility)

## Что починено
Попапы (Win/Lose/Pause/Settings/Shop) не показывались — alpha анимировалась, но GameObject оставался выключенным.

**Причина:** в `PopupBase.Awake()` стоял `gameObject.SetActive(false)`. Когда GameObject загружается из сцены уже выключенным, Awake не вызывается. При первом `Show()` → `SetActive(true)` → Awake срабатывает → `SetActive(false)` снова деактивирует объект, но DOTween твины уже стартуют на невидимом объекте.

**Фикс:** убрал `SetActive(false)` из Awake. Editor скрипт уже сам деактивирует попапы при создании сцены. Hide() деактивирует после анимации закрытия.

**Бонус:** добавил `.SetUpdate(true)` к backdrop и canvasGroup твинам в Show/Hide — попапы теперь корректно анимируются при Time.timeScale=0 (важно для Pause popup).

## Файл для замены
- `Assets/SporosGame/Scripts/UI/PopupBase.cs`

## Что сделать
1. Распаковать архив → перезаписать PopupBase.cs
2. Дождаться компиляции
3. Play (никакого editor setup запускать не нужно — баг только в коде PopupBase)

## Запомнено для будущих итераций
- В PopupBase.Awake() **никогда** не деактивировать gameObject — деактивация только при создании в editor скрипте и в Hide() OnComplete
- Все DOTween твины в попапах должны иметь `.SetUpdate(true)` для работы при паузе
