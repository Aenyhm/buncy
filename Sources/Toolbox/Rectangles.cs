namespace Module2.Toolbox;

/// <summary>
/// Rectangle de points entiers.
///
/// Attention, contrairement au Rectangle de Raylib, je trouve
/// plus pratique d'avoir les 4 points du rectangle plutôt que
/// de devoir additionner Start + Size.
/// </summary>
public record struct RecI32 {
    public Vec2I32 From { get; set; }
    public Vec2I32 To { get; set; }

    public RecI32(Vec2I32 from, Vec2I32 to) {
        From = from;
        To = to;
    }

    public readonly Vec2I32 Size => To - From;

    public readonly Rectangle ToRaylibRectangle() => new(From.ToVec2F32(), Size.ToVec2F32());
}
