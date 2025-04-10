using Module2.Gameplay;
using Module2.Levels;

namespace Module2.Editor;

public class EditorScene : Scene {
    public readonly EditorLayerManager LayerManager;
    private readonly EditorView _view;
    private readonly EditorMenu _menu;

    public EditorScene() {
        LayerManager = new EditorLayerManager();
        LayerManager.CurrentLayerId = 1;

        _view = new EditorView(LayerManager, 64);
        _menu = new EditorMenu(this);
    }

    public override void Enter() {
        ShowCursor();

        if (LevelService.Current.Id != string.Empty) {
            LoadLevel(LevelService.Current);
        }
    }

    public override void Update(float dt) {
        if (_menu.State != EditorMenuState.Input) {
            if (IsKeyPressed(KeyboardKey.F)) { SceneService.Instance.GoTo<GameplayScene>(); return; }
            if (IsKeyPressed(KeyboardKey.P)) { GoToLevel(-1); return; }
            if (IsKeyPressed(KeyboardKey.N)) { GoToLevel(+1); return; }
        }

        SetMouseCursor(MouseCursor.Default);

        var inMenu = _menu.Update(dt);

        if (LayerManager.CurrentLayerId != 0) _view.Update(dt, inMenu);
    }

    private void GoToLevel(int delta) {
        LevelService.GoTo(LevelService.CurrentIndex + delta);
        LoadLevel(LevelService.Current);
    }

    public override void Draw() {
        if (LayerManager.CurrentLayerId != 0) _view.Draw();
        _menu.Draw();
    }

    public void SaveLevel(Level level) {
        level.Layers = LayerManager.ShrinkToLayers();

        if (level.IsTooSmall) {
            NotificationManager.Add(new Notification(LogType.Error, "Level too small!"));
        } else if (level.IsTooLarge) {
            NotificationManager.Add(new Notification(LogType.Error, "Level too large!"));
        } else {
            var isNew = level.Id == string.Empty;
            if (isNew) level.Id = LevelManager.GenerateLevelId();

            LevelManager.Save(level, isNew);
            LevelService.Current = level;
            NotificationManager.Add(new Notification(LogType.Success, "Level saved"));
        }
    }

    public void RecenterView() {
        _view.Recenter();
    }

    public void LoadLevel(Level level) {
        LevelService.Current = level;
        LayerManager.FillFromLevel(level);
        RecenterView();
    }

    public void ClearLevel() {
        LevelService.Current = new Level();
        LayerManager.Clear();
        RecenterView();
    }

    public void DeleteLevel() {
        LevelManager.Delete(LevelService.Current);
        ClearLevel();
    }
}
