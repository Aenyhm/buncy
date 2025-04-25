using Module2.Levels;

namespace Module2.Editor;

public enum EditorMenuState : byte {
    Main,
    Input,
    Confirm,
    Load,
    Reorder,
}
    
public class EditorMenu {
    private const int _height = 800;
    private static readonly Vec2F32 _tileButtonSize = new(32, 32);
    
    private readonly EditorScene _scene;
    private readonly GuiPanel _panel;
    private Camera2D _camera;
    public EditorMenuState State { get; private set; }
    private string _inputText;
    private Action<string>? _inputStateComplete;
    private Action? _confirmStateComplete;
    private int _selectedLevelIndex;

    public EditorMenu(EditorScene scene) {
        _scene = scene;
        _camera = new Camera2D();
        State = EditorMenuState.Main;
        _panel = new GuiPanel();
        _inputText = string.Empty;
         
        Window.OnResized += Zoom;
    }    
    
    private void Zoom() {
        _camera.Zoom = Window.Size.Y/(float)_height;
    }
    
    public bool Update(float dt) {
        var mousePosition = GetMousePosition();
        var mouseData = new MouseData(
            GetScreenToWorld2D(mousePosition, _camera),
            IsMouseButtonPressed(MouseButton.Left)
        );
        
        _panel.BeginLayout(mouseData);
        {
            switch (State) {
                case EditorMenuState.Main: UpdateMainState(); break;
                case EditorMenuState.Input: UpdateInputState(); break;
                case EditorMenuState.Confirm: UpdateConfirmState(); break;
                case EditorMenuState.Load: UpdateLoadState(); break;
                case EditorMenuState.Reorder: UpdateReorderState(); break;
            }
        }
        var panelSize = _panel.EndLayout();
        panelSize *= _camera.Zoom;
        
        var panelRec = new Rectangle(
            Window.Size.X - panelSize.X,
            0,
            panelSize.X,
            panelSize.Y
        );
        
        _camera.Offset = panelRec.Position;
        
        return State != EditorMenuState.Main || CheckCollisionPointRec(mousePosition, panelRec);
    }
    
    public void Draw() {
        BeginMode2D(_camera);
            _panel.Draw();
        EndMode2D();
    }
    
    private void UpdateMainState() {
        if (IsKeyPressed(KeyboardKey.Escape)) {
            Window.ShouldClose = true;
            return;
        }
        
        if (LevelService.Current.Id == string.Empty) {
            _panel.AddTitle("<unnamed>");

            if (_panel.PushButtonText("Save As")) {
                AskInput(input => {
                    var level = new Level();
                    level.Name = input;
                    _scene.SaveLevel(level);
                });
            }
        } else {
            _panel.AddTitle(LevelService.Current.Name);
            
            _panel.BeginRow();
            if (_panel.PushButtonText("Save")) _scene.SaveLevel(LevelService.Current);
            if (_panel.PushButtonText("Rename")) {
                AskInput(input => {
                    var level = LevelService.Current;
                    level.Name = input;
                    _scene.SaveLevel(level);
                    LevelService.Current = level;
                });
            }
            _panel.EndRow();
            
            _panel.BeginRow();
            if (_panel.PushButtonText("Copy")) {
                AskInput(input => {
                    var level = new Level();
                    level.Name = input;
                    _scene.SaveLevel(level);
                    LevelService.Current = level;
                });
            }
            if (_panel.PushButtonText("Delete")) AskConfirm(_scene.DeleteLevel);
            _panel.EndRow();
        }
        
        _panel.BeginRow();
        if (_panel.PushButtonText("Clear")) AskConfirm(_scene.ClearLevel);
        if (_panel.PushButtonText("Center")) _scene.RecenterView();
        _panel.EndRow();
        
        _panel.BeginRow();
        if (_panel.PushButtonText("Load")) State = EditorMenuState.Load;
        if (_panel.PushButtonText("Reorder")) State = EditorMenuState.Reorder;
        _panel.EndRow();

        _panel.AddTitle("Tiles");
        for (byte layerIndex = 0; layerIndex < _scene.LayerManager.Layers.Length; layerIndex++) {
            var layer = _scene.LayerManager.Layers[layerIndex];
            var layerId = (byte)(layerIndex + 1);
            
            for (var tileIndex = 0; tileIndex < layer.TileTypes.Length; tileIndex++) {
                var type = layer.TileTypes[tileIndex];

                if (tileIndex % 3 == 0) {
                    _panel.EndRow();
                    _panel.BeginRow();
                }

                var sprite = TileSpriteCollectionService.Instance.Get(type).GetFirstSprite();

                var active = _scene.LayerManager.CurrentLayerId == layerId && layer.CurrentTileId == type;
                if (_panel.PushButtonImage(sprite, _tileButtonSize, active)) {
                    _scene.LayerManager.CurrentLayerId = layerId;
                    layer.CurrentTileId = type;
                }
            }

            _panel.EndRow();
        }
    }

    private void UpdateInputState() {
        var inputText = _inputText != string.Empty ? _inputText : "...";
        _panel.AddInputText(inputText);

        if (IsKeyPressed(KeyboardKey.Enter)) {
            if (_inputText != string.Empty) {
                _inputStateComplete?.Invoke(_inputText);
            }
            State = EditorMenuState.Main;
            _inputText = string.Empty;
            _inputStateComplete = null;
        } else if (_inputText.Length > 0 && IsKeyPressed(KeyboardKey.Backspace)) {
            _inputText = _inputText.Substring(0, _inputText.Length - 1);
        } else {
            var key = GetCharPressed();
            if (key != 0) _inputText += (char)key;
        }
        
        if (IsKeyPressed(KeyboardKey.Escape) || _panel.PushButtonText("Cancel")) {
            _inputText = string.Empty;
            State = EditorMenuState.Main;
        }
    }

    private void UpdateConfirmState() {
        _panel.AddTitle("Sure?");
        
        _panel.BeginRow();
        {
            if (IsKeyPressed(KeyboardKey.Enter) || _panel.PushButtonText("Yes")) {
                _confirmStateComplete?.Invoke();
                _confirmStateComplete = null;
                State = EditorMenuState.Main;
            }

            if (IsKeyPressed(KeyboardKey.Escape) || _panel.PushButtonText("No")) {
                State = EditorMenuState.Main;
            }
        }
        _panel.EndRow();
    }
    
    private void UpdateLoadState() {
        foreach (var level in LevelService.Levels) {
            if (_panel.PushButtonText(level.Name)) {
                _scene.LoadLevel(level);
                State = EditorMenuState.Main;
            }
        }
        
        if (IsKeyPressed(KeyboardKey.Escape) || _panel.PushButtonText("Cancel")) {
            _inputText = string.Empty;
            State = EditorMenuState.Main;
        }
    }
    
    private void UpdateReorderState() {
        for (var i = 0; i < LevelService.Levels.Count; i++) {
            var level = LevelService.Levels[i];
            if (_panel.PushButtonText(level.Name, _selectedLevelIndex == i)) {
                _selectedLevelIndex = i;
            }
        }

        if (IsKeyPressed(KeyboardKey.Up) && _selectedLevelIndex > 0) {
            LevelManager.Permute(_selectedLevelIndex, _selectedLevelIndex - 1);
            _selectedLevelIndex--;
        } else if (IsKeyPressed(KeyboardKey.Down) && _selectedLevelIndex < LevelService.Levels.Count - 1) {
            LevelManager.Permute(_selectedLevelIndex, _selectedLevelIndex + 1);
            _selectedLevelIndex++;
        }

        if (IsKeyPressed(KeyboardKey.Escape) || _panel.PushButtonText("Cancel")) {
            _selectedLevelIndex = 0;
            State = EditorMenuState.Main;
        }
    }
   
    private void AskInput(Action<string> callback) {
        _inputStateComplete = callback;
        State = EditorMenuState.Input;
    }
    
    private void AskConfirm(Action callback) {
        _confirmStateComplete = callback;
        State = EditorMenuState.Confirm;
    }
}
