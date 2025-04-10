namespace Module2.Editor;

/// <summary>
/// Gère la partie GUI de l'éditeur.
/// Pour afficher le menu, on ajoute des éléments un à un au <c>GuiPanel</c>.
/// Ce dernier garde la position où il doit afficher le prochain élément.
/// Il retourne ensuite ses dimensions pour l'afficher correctement.
/// Les éléments ajoutés au panneau sont stockés dans une liste pour être
/// - à la fin de la boucle de jeu - affichés dans le Draw.
/// </summary>
public class GuiPanel {
    private const int _childGap = 15;
    private const int _borderThickness = 3;
    private const int _titleHeight = 24;
    private const int _textHeight = 20;
    private const float _buttonRoundness = 0.3f;
    private const float _buttonBorderThickness = 1;
    private static readonly Vec2F32 _borderSize = _borderThickness*Vec2F32.One;
    private static readonly Vec2F32 _padding = new(20, 20);
    private static readonly Vec2F32 _buttonTextPadding = new(15, 5);
    private static readonly Vec2F32 _buttonSpritePadding = new(5, 5);
    private static readonly Color _bgColor = GetColor(0x12141bff);
    private static readonly Color _borderColor = GetColor(0x2d3038ff);

    private static readonly Color _textColor = Color.RayWhite;
    private static readonly Color _buttonBgColor = GetColor(0x333344ff);
    private static readonly Color _buttonBorderColor = GetColor(0x555566ff);
    private static readonly Color _buttonHoverColor = GetColor(0x444455ff);
    private static readonly Color _buttonPushedColor = GetColor(0x777788ff);
    private static readonly Color _buttonActiveColor = GetColor(0x666677ff);
    private static readonly Color _inputTextColor = GetColor(0x222222ff);
    private static readonly Color _inputBgColor = GetColor(0xddddddff);

    private static Font _font;

    private readonly List<Widget> _widgets = [];
    private Vec2F32 _size;
    private Vec2F32 _at;
    private MouseData _mouseData;
    private bool _rowMode;
    private float _nextY;

    public GuiPanel() {
        _font = FontService.Instance.Get("default");
    }

    public void BeginLayout(MouseData mouseData) {
        _mouseData = mouseData;
        _widgets.Clear();
        _at = _borderSize + _padding;
        _size = _at + _padding + _borderSize;
    }

    public Vec2F32 EndLayout() {
        return _size;
    }

    public void BeginRow() {
        _rowMode = true;
    }

    public void EndRow() {
        if (_rowMode) {
            _rowMode = false;
            _at.X = _borderSize.X + _padding.X;
            _at.Y = _nextY;
            _nextY = 0;
        }
    }

    public void AddTitle(string text) {
        var textSize = MeasureTextEx(_font, text, _titleHeight, 0);
        var rec = new Rectangle(_at, textSize);
        _widgets.Add(new Widget(text, rec, _textColor));

        EndElement(textSize);
    }

    public void AddInputText(string text) {
        var textSize = MeasureTextEx(_font, text, _textHeight, 0);
        var textRec = new Rectangle(_at + _buttonTextPadding, textSize);

        var buttonSize = textSize + _buttonTextPadding*2;
        var buttonRec = new Rectangle(_at, buttonSize);

        _widgets.Add(new Widget(WidgetType.Rect, buttonRec, _inputBgColor));
        _widgets.Add(new Widget(WidgetType.Border, buttonRec, _buttonBorderColor));
        _widgets.Add(new Widget(text, textRec, _inputTextColor));

        EndElement(buttonSize);
    }

    public bool PushButtonText(string text, bool active = false) {
        var textSize = MeasureTextEx(_font, text, _textHeight, 0);
        var textRec = new Rectangle(_at + _buttonTextPadding, textSize);

        var buttonSize = textSize + _buttonTextPadding*2;
        var buttonRec = new Rectangle(_at, buttonSize);
        var state = GetElementState(buttonRec, active);
        var color = state switch {
            WidgetState.Hot => _buttonHoverColor,
            WidgetState.Pushed => _buttonPushedColor,
            WidgetState.Active => _buttonActiveColor,
            _ => _buttonBgColor
        };

        _widgets.Add(new Widget(WidgetType.Rect, buttonRec, color));
        _widgets.Add(new Widget(WidgetType.Border, buttonRec, _buttonBorderColor));
        _widgets.Add(new Widget(text, textRec, _textColor));

        EndElement(buttonSize);

        return state == WidgetState.Pushed;
    }

    public bool PushButtonImage(Sprite sprite, Vec2F32 size, bool active) {
        var spriteRec = new Rectangle(_at + _buttonSpritePadding, size);

        var buttonSize = size + _buttonSpritePadding*2;
        var buttonRec = new Rectangle(_at, buttonSize);

        var state = GetElementState(buttonRec, active);
        var color = state switch {
            WidgetState.Hot => _buttonHoverColor,
            WidgetState.Pushed => _buttonPushedColor,
            WidgetState.Active => _buttonActiveColor,
            _ => _buttonBgColor
        };

        _widgets.Add(new Widget(WidgetType.Rect, buttonRec, color));
        _widgets.Add(new Widget(WidgetType.Border, buttonRec, _buttonBorderColor));
        _widgets.Add(new Widget(sprite, spriteRec, Color.White));

        EndElement(buttonSize);

        return state == WidgetState.Pushed;
    }

    private WidgetState GetElementState(Rectangle rec, bool active) {
        var result = WidgetState.None;

        var hot = CheckCollisionPointRec(_mouseData.Position, rec);
        var pushed = hot && _mouseData.Pressed;

        if (pushed) {
            result = WidgetState.Pushed;
        } else if (active) {
            result = WidgetState.Active;
        } else if (hot) {
            result = WidgetState.Hot;
        }

        if (hot) SetMouseCursor(MouseCursor.PointingHand);

        return result;
    }

    private void EndElement(Vec2F32 elementSize) {
        if (_rowMode) {
            _size.X = Math.Max(_size.X, _at.X + elementSize.X + _padding.X + _borderSize.X);
            _size.Y = Math.Max(_size.Y, _at.Y + elementSize.Y + _padding.Y + _borderSize.Y);
            _at.X += elementSize.X + _childGap;
            _nextY = Math.Max(_nextY, _at.Y + elementSize.Y + _childGap);
        } else {
            _size.X = Math.Max(_size.X, _at.X + elementSize.X + _padding.X + _borderSize.X);
            _size.Y = _at.Y + elementSize.Y + _padding.Y + _borderSize.Y;
            _at.Y += elementSize.Y + _childGap;
        }
    }

    public void Draw() {
        DrawRectangleV(Vec2F32.Zero, _size, _bgColor);

        var borderRec = new Rectangle(Vec2F32.Zero, _size);
        DrawRectangleLinesEx(borderRec, _borderThickness, _borderColor);

        foreach (var w in _widgets) {
            switch (w.Type) {
                case WidgetType.Text:
                    DrawTextEx(_font, w.Text, w.Rec.Position, w.Rec.Size.Y, 0, w.Color);
                    break;
                case WidgetType.Rect:
                    DrawRectangleRounded(w.Rec, _buttonRoundness, 4, w.Color);
                    break;
                case WidgetType.Border:
                    DrawRectangleRoundedLinesEx(w.Rec, _buttonRoundness, 4, _buttonBorderThickness, w.Color);
                    break;
                case WidgetType.Sprite:
                    var sprite = w.Sprite!.Value;
                    DrawTexturePro(sprite.Texture, sprite.Coordinates.ToRaylibRectangle(), w.Rec, Vec2F32.Zero, 0, w.Color);
                    break;
            }
        }
    }
}
