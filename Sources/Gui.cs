namespace Module2;

/// Ma première idée était de faire une classe abstraite Widget et d'avoir
/// des enfants : TextWidget, ButtonWidget, etc. sauf que lorsqu'on crée
/// une instance de classe, son allocation est faite dans la heap memory.
/// Et donc faire des new à chaque boucle de jeu fait de l'allocation mémoire
/// qui n'est pas nettoyée même si on fait un clean de liste à chaque tour.
///
/// J'ai donc décidé d'utiliser une struct Widget avec un enum pour connaitre
/// son type, car les structs ne peuvent pas utiliser l'héritage.

public enum WidgetType : byte {
    Text,
    Rect,
    Border,
    Sprite,
}

public enum WidgetState : byte {
    None,
    Hot,
    Pushed,
    Active,
}

public readonly struct Widget {
    public readonly Sprite? Sprite;
    public readonly string? Text;
    public readonly Rectangle Rec;
    public readonly Color Color;
    public readonly WidgetType Type;

    public Widget(WidgetType type, Rectangle rec, Color color) {
        Type = type;
        Rec = rec;
        Color = color;
    }

    public Widget(string text, Rectangle rec, Color color) : this (WidgetType.Text, rec, color) {
        Text = text;
    }

    public Widget(Sprite sprite, Rectangle rec, Color color) : this(WidgetType.Sprite, rec, color) {
        Sprite = sprite;
    }
}

public readonly struct MouseData {
    public readonly Vec2F32 Position;
    public readonly bool Pressed;

    public MouseData(Vec2F32 position, bool pressed) {
        Position = position;
        Pressed = pressed;
    }
}

public struct TextObject {
    private readonly Font _font;
    private readonly string _text;
    private Vec2F32 _origin;
    private float _height;
    
    public Color Color { get; set; }
    public Vec2F32 Position { get; set; }
    public float Height {
        get => _height;
        set {
            _height = value;
            var size = MeasureTextEx(_font, _text, _height, 0);
            _origin = size/2;
        }
    }

    public float Rotation { get; set; }
    
    public TextObject(string text, Font font, float height, Vec2F32 position, float rotation, Color color) {
        _font = font;
        _text = text;
        Position = position;
        Height = height;
        Rotation = rotation;
        Color = color;
    }

    public void Draw() {
        DrawTextPro(_font, _text, Position, _origin, Rotation, Height, 0, Color);
    }
}
