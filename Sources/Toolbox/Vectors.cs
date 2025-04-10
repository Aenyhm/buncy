global using Vec2F32 = System.Numerics.Vector2;

namespace Module2.Toolbox;

/// <summary>
/// Vecteur d'entiers dans R².
/// </summary>
public record struct Vec2I32 {
    public int X { get; set; }
    public int Y { get; set; }

    public Vec2I32(int x, int y) {
        X = x;
        Y = y;
    }

    public static readonly Vec2I32 Zero = new(0, 0);

    public readonly Vec2F32 ToVec2F32() => new(X, Y);

    public static Vec2I32 Floor(Vec2F32 v) {
        return new Vec2I32(
            (int)Math.Floor(v.X),
            (int)Math.Floor(v.Y)
        );
    }

    public static Vec2I32 Clamp(Vec2I32 v, Vec2I32 min, Vec2I32 max) {
        return new Vec2I32(
            Math.Clamp(v.X, min.X, max.X),
            Math.Clamp(v.Y, min.Y, max.Y)
        );
    }

    public static Vec2I32 operator +(Vec2I32 a, Vec2I32 b)  => new(a.X + b.X, a.Y + b.Y);
    public static Vec2I32 operator -(Vec2I32 a, Vec2I32 b)  => new(a.X - b.X, a.Y - b.Y);
    public static Vec2I32 operator *(Vec2I32 a, Vec2I32 b)  => new(a.X*b.X, a.Y*b.Y);
    public static Vec2I32 operator *(Vec2I32 v, int factor) => new(v.X*factor, v.Y*factor);
    public static Vec2I32 operator /(Vec2I32 a, Vec2I32 b)  => new(a.X/b.X, a.Y/b.Y);
    public static Vec2I32 operator /(Vec2I32 v, int factor) => new(v.X/factor, v.Y/factor);
}

public static class Vec2F32Extensions {
    public static Vec2I32 ToVec2I32(this Vec2F32 v) => new((int)v.X, (int)v.Y);
}
