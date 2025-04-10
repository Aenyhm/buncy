namespace Module2;

public class VictoryScene : Scene {
    private const int _maxTextAngle = 35;
    private const int _textSpeed = 30;
    
    private TextObject _textObject;
    private float _textScale;
    private bool _rotateRight;
    
    public VictoryScene() {
        _textObject = new TextObject(
            "Merci d'avoir joué !", FontService.Instance.Get("default"), 50, Vec2F32.Zero, 0, Color.White
        );
        OnWindowResized();
    }
    
    public override void Enter() {
        Window.OnResized += OnWindowResized;
    }
    
    public override void Exit() {
        Window.OnResized -= OnWindowResized;
    }
    
    private void OnWindowResized() {
        _textObject.Position = Window.Size.ToVec2F32()/2;
        _textScale = Window.Size.X/500f;
    }
    
    public override void Update(float dt) {
        if (GetKeyPressed() != 0) { Window.ShouldClose = true; return; }
        
        if (Math.Abs(_textObject.Rotation) > _maxTextAngle) _rotateRight = !_rotateRight;
        
        _textObject.Rotation += dt*_textSpeed*(_rotateRight ? 1 : -1);
        _textObject.Height = Math.Abs(_textObject.Rotation)*_textScale;
    }

    public override void Draw() {
        _textObject.Draw();
    }
}
