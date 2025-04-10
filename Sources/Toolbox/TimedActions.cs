namespace Module2.Toolbox;

/// <summary>
/// Permet de différer des actions dans le temps et/ou
/// de faire une action à un certain moment entre le
/// début et la durée de l'action (ex: animations).
/// </summary>
public class TimedAction {
    public readonly Action OnComplete;
    public readonly float Duration;
    public float Elapsed { get; set; }

    protected float TimeAmount => Elapsed/Duration;

    public TimedAction(float duration, Action onComplete) {
        Duration = duration;
        OnComplete = onComplete;
    }

    public virtual void Update() {}
}

public static class TimedActionManager {
    private static readonly List<TimedAction> _items = [];

    public static void Add(TimedAction item) {
        _items.Add(item);
    }

    public static void Update(float dt) {
        for (var i = _items.Count - 1; i >= 0; i--) {
            var item = _items[i];

            if (item.Elapsed < item.Duration) {
                item.Update();
                item.Elapsed += dt;
            } else {
                item.OnComplete();
                _items.RemoveAt(i);
            }
        }
    }
}
