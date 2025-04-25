namespace Module2.Editor;

public class EditorView {
    private const int _defaultGridWidth = 14;
    private const float _zoomSpeed = 5f;
    private const float _zoomMin = 0.5f;
    private const float _zoomMax = 3.0f;
    private static readonly Vec2I32 _defaultLevelPadding = new(4, 4);

    private static readonly Color _gridColor = GetColor(0xffffff33);
    private static readonly Color _hotCellColor = GetColor(0xffffff44);

    private readonly EditorLayerManager _layerManager;
    private Camera2D _camera;
    private RecI32 _gridArea;
    private Vec2I32? _hotPosition;
    private readonly int _tileSide;
    private readonly Vec2F32 _defaultLevelSize;

    public EditorView(EditorLayerManager layerManager, int tileSide) {
        _layerManager = layerManager;
        _tileSide = tileSide;
        _camera = new Camera2D();
        _camera.Zoom = 1.0f;
        _gridArea = new RecI32();
        _defaultLevelSize = new Vec2F32(_defaultGridWidth, (int)(_defaultGridWidth/Window.AspectRatio));
        
        Window.OnResized += SetSize;
        ResetViewCamera();
    }

    private void SetSize() {
        _camera.Offset = Window.Size.ToVec2F32()/2;
        UpdateGridSize();
    }
    
    public void Recenter() {
        ResetViewCamera();
        UpdateGridSize();
    }

    public void Update(float dt, bool inMenu) {
        var moving = HandleMove();
        HandleZoom(dt);
        
        if (moving || inMenu) {
            _hotPosition = null;
        } else {
            HandleMouse();
        }
    }
    
    private bool HandleMove() {
        var result = false;
        
        if (IsKeyDown(KeyboardKey.LeftControl)) {
            _hotPosition = null;
            SetMouseCursor(MouseCursor.ResizeAll);
            
            if (IsMouseButtonDown(MouseButton.Left)) {
                var mouseDelta = GetMouseDelta();
                _camera.Target -= mouseDelta;
                UpdateGridSize();
            }
            
            result = true;
        }
        
        return result;
    }
    
    private void HandleZoom(float dt) {
        var deltaZoom = GetMouseWheelMove();
        if (deltaZoom != 0) {
            var mousePositionBefore = GetScreenToWorld2D(GetMousePosition(), _camera);

            var zoomTarget = _camera.Zoom + deltaZoom*_zoomSpeed*dt;
            _camera.Zoom = Math.Clamp(zoomTarget, _zoomMin, _zoomMax);
            
            // Pour centrer là où le curseur pointe.
            var mousePositionAfter = GetScreenToWorld2D(GetMousePosition(), _camera);
            _camera.Target += mousePositionBefore - mousePositionAfter;
            
            UpdateGridSize();
        }
    }
    
    private void HandleMouse() {
        var mousePosition = GetScreenToWorld2D(GetMousePosition(), _camera);
        var relativePosition = Vec2I32.Floor(mousePosition/_tileSide);
        
        var r =_gridArea.ToRaylibRectangle();

        if (CheckCollisionPointRec(relativePosition.ToVec2F32(), _gridArea.ToRaylibRectangle())) {
            _hotPosition = relativePosition;

            if (_layerManager.CurrentLayerId != 0) {
                var currentLayer = _layerManager.Layers[_layerManager.CurrentLayerId - 1];
                if (currentLayer.CurrentTileId != 0 && IsMouseButtonDown(MouseButton.Left)) {
                    currentLayer.Set(relativePosition, currentLayer.CurrentTileId);
                } else if (IsMouseButtonDown(MouseButton.Right)) { 
                    currentLayer.Set(relativePosition, 0);
                }
            }
        } else {
            _hotPosition = null;
        }
    }
    
    private void ResetViewCamera() {
        var levelRec = _layerManager.GetFilledRectangle();

        if (levelRec.Size.X < _defaultLevelSize.X) {
            var diffX = _defaultLevelSize.X - levelRec.Width;
            levelRec.X -= diffX;
            levelRec.Width = _defaultLevelSize.X;
        }
        if (levelRec.Size.Y < _defaultLevelSize.Y) {
            var diffY = _defaultLevelSize.Y - levelRec.Height;
            levelRec.Y -= diffY;
            levelRec.Height = _defaultLevelSize.Y;
        }

        var zoomV = Window.Size.ToVec2F32()/((levelRec.Size + _defaultLevelPadding.ToVec2F32())*_tileSide);

        _camera.Zoom = Math.Min(zoomV.X, zoomV.Y);
        _camera.Target = (levelRec.Position + levelRec.Size)/2*_tileSide + Vec2F32.One*_tileSide/2f;
    }

    private void UpdateGridSize() {
        var worldSize = _camera.Offset*2/_camera.Zoom;
        var topLeftPoint = _camera.Target - worldSize/2;
        
        _gridArea.From = Vec2I32.Floor(topLeftPoint/_tileSide - Vec2F32.One);
        _gridArea.To = Vec2I32.Floor((topLeftPoint + worldSize)/_tileSide + Vec2F32.One);
    }
    
    public void Draw() {
        var bgColor = Game.WaterColor;
        if (_layerManager.CurrentLayerId > 1) {
            bgColor.A = 128;
        }
        DrawRectangleV(Vec2F32.Zero, Window.Size.ToVec2F32(), bgColor);

        BeginMode2D(_camera);
            for (var y = _gridArea.From.Y; y <= _gridArea.To.Y; y++) {
                var startPos = new Vec2F32(_gridArea.From.X, y)*_tileSide;
                var endPos = new Vec2F32(_gridArea.To.X, y)*_tileSide;
                DrawLineV(startPos, endPos, _gridColor);
            }
            for (var x = _gridArea.From.X; x <= _gridArea.To.X; x++) {
                var startPos = new Vec2F32(x, _gridArea.From.Y)*_tileSide;
                var endPos = new Vec2F32(x, _gridArea.To.Y)*_tileSide;
                DrawLineV(startPos, endPos, _gridColor);
            }
            
            for (var layerIndex = 0; layerIndex < _layerManager.Layers.Length; layerIndex++) {
                var layer = _layerManager.Layers[layerIndex];
                var layerId = layerIndex + 1;
                var color = layerId == _layerManager.CurrentLayerId ? Color.White : GetColor(0xffffffaa);
                for (var y = _gridArea.From.Y; y < _gridArea.To.Y; y++) {
                    for (var x = _gridArea.From.X; x < _gridArea.To.X; x++) {
                        var position = new Vec2I32(x, y);
                        var tileValue = layer.Get(position);
                        if (tileValue != 0) {
                            var sprite = TileSpriteCollectionService.Instance.Get(tileValue).GetFirstSprite();
                            var dest = new Rectangle((position*_tileSide).ToVec2F32(), _tileSide*Vec2F32.One);
                            DrawTexturePro(sprite.Texture, sprite.Coordinates.ToRaylibRectangle(), dest, Vec2F32.Zero, 0, color);
                        }
                    }
                }
            }
            
            if (_hotPosition != null) {
                var hotPosition = _hotPosition.Value;
                var currentLayer = _layerManager.Layers[_layerManager.CurrentLayerId - 1];
                var rec = new Rectangle((hotPosition*_tileSide).ToVec2F32(), _tileSide*Vec2F32.One);

                if (currentLayer.CurrentTileId != 0) {
                    var sprite = TileSpriteCollectionService.Instance.Get(currentLayer.CurrentTileId).GetFirstSprite();
                    DrawTexturePro(sprite.Texture, sprite.Coordinates.ToRaylibRectangle(), rec, Vec2F32.Zero, 0, GetColor(0xffffff66));
                } else {
                    DrawRectangleRec(rec, _hotCellColor);
                }
            }
        EndMode2D();
    }
}