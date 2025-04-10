namespace Module2.Gameplay.UI;

/// <summary>
/// Permet de dessiner les entités à l'écran.
/// </summary>
public class EntityView {
    public Sprite Sprite { get; set; }
    public Color Color { get; set; }
    public Vec2F32 Position { get; set; }
    public byte DisplayLayer { get; set; }

    public void StartMoveAnimation(Vec2I32 deltaMove, float duration, Easing easing, Action? onComplete = null) {
        var target = Position + deltaMove.ToVec2F32()*Game.TileSide;

        var anim = new MoveAnimation(
            this,
            Position,
            target,
            duration,
            easing,
            onComplete
        );
        TimedActionManager.Add(anim);
    }
    
    public void Draw() {
        var dest = new Rectangle(Position, Game.TileSide*Vec2F32.One);
        dest.Size = Sprite.Size.ToVec2F32();
        
        DrawTexturePro(Sprite.Texture, Sprite.Coordinates.ToRaylibRectangle(), dest, Sprite.Origin, 0, Color);
    }
}
