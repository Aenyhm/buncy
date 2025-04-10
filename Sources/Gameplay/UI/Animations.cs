namespace Module2.Gameplay.UI;

public static class EasingsExtensions {
    public static float Compute(this Easing easing, float start, float end, float amount) {
        return start + easing(amount)*(end - start);
    }

    public static Vec2F32 ComputeV(this Easing easing, Vec2F32 v1, Vec2F32 v2, float amount) {
       return new Vec2F32(
            Compute(easing, v1.X, v2.X, amount),
            Compute(easing, v1.Y, v2.Y, amount)
        );
    }
}

public class MoveAnimation : TimedAction {
    private readonly EntityView _view;
    private readonly Easing _easing;
    private readonly Vec2F32 _from;
    private readonly Vec2F32 _to;
    
    public MoveAnimation(EntityView view, Vec2F32 from, Vec2F32 to, float duration, Easing easing, Action? onComplete) :
        base(duration, () => Finish(view, to, onComplete)) {
        _view = view;
        _from = from;
        _to = to;
        _easing = easing;
    }
 
    public override void Update() {
        _view.Position = _easing.ComputeV(_from, _to, TimeAmount);
    }
    
    private static void Finish(EntityView view, Vec2F32 to, Action? onComplete) {
        view.Position = to;
        onComplete?.Invoke();
    }
}

public class EndAnimation : TimedAction {
    private readonly EntityView _view;
    private readonly Sprite _fromSprite;
    
    public EndAnimation(EntityView view, float duration, Action onComplete) : base(duration, onComplete) {
        _view = view;
        _fromSprite = _view.Sprite;
    }
 
    public override void Update() {
        var sprite = _view.Sprite;
        Easing easing = Easings.InOutBack;
        
        var coordsToY = easing.Compute(_fromSprite.Coordinates.To.Y, 0, TimeAmount);
        var newCoordsTo = _fromSprite.Coordinates.To;
        newCoordsTo.Y = (int)coordsToY;
        sprite.Coordinates = new RecI32(_fromSprite.Coordinates.From, newCoordsTo);
        
        var sizeY = easing.Compute(_fromSprite.Size.Y, 0, TimeAmount);
        sprite.Size = new Vec2I32(sprite.Size.X, (int)sizeY);
        
        var originY = easing.Compute(_fromSprite.Origin.Y, 14, TimeAmount);
        sprite.Origin = new Vec2F32(sprite.Origin.X, originY);

        _view.Sprite = sprite;
    }
}
