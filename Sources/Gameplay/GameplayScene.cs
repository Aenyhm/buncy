using Module2.Editor;
using Module2.Gameplay.Core;
using Module2.Gameplay.UI;
using Module2.Levels;

namespace Module2.Gameplay;

public class GameplayScene : Scene {
    private readonly EntityManager _entityManager = new();
    private readonly EntityViewManager _entityViewManager = new();
    private Camera2D _camera;
    private readonly Vec2F32 _zoomPadding = 2*Game.TileSide*Vec2F32.One;
    private bool _gameTurn;

    public override void Enter() {
        HideCursor();

        if (LevelService.Current.Id == string.Empty && LevelService.Levels.Count != 0) {
            LevelService.GoTo(0);
        }

        EnterLevel();
        Window.OnResized += OnWindowResized;
    }

    public override void Exit() {
        Window.OnResized -= OnWindowResized;
        ExitLevel();
    }

    private void OnWindowResized() {
        UpdateCamera();
    }

    private void EnterLevel() {
        _entityManager.SetupLevel(LevelService.Current);
        _entityViewManager.CreateFromEntities(_entityManager);

        UpdateCamera();
        _gameTurn = false;
    }

    private void ExitLevel() {
        _entityManager.Clear();
        _entityViewManager.Clear();
    }

    private void GoToLevel(int delta) {
        ExitLevel();
        LevelService.GoTo(LevelService.CurrentIndex + delta);
        #if !DEBUG
        if (LevelService.CurrentIndex == 0) {
            SceneService.Instance.GoTo<VictoryScene>();
            return;
        }
        #endif
        EnterLevel();
    }

    private void UpdateCamera() {
        var levelRenderSizeF = LevelService.Current.Size.ToVec2F32()*Game.TileSide;
        var windowSizeF = Window.Size.ToVec2F32();
        _camera.Offset = windowSizeF/2f;

        var zoomV = windowSizeF/(levelRenderSizeF + _zoomPadding);
        _camera.Zoom = Math.Min(zoomV.X, zoomV.Y);

        _camera.Target = levelRenderSizeF/2f;
        // Décalage car l'origine des tuiles est au centre.
        _camera.Target -= Vec2F32.One*Game.TileSide/2f;
    }

    public override void Update(float dt) {
        if (IsKeyPressed(KeyboardKey.Escape)) { Window.ShouldClose = true; return; }

        if (!_gameTurn) {
            #if DEBUG
            if (IsKeyPressed(KeyboardKey.F)) { SceneService.Instance.GoTo<EditorScene>(); return; }
            if (IsKeyPressed(KeyboardKey.P)) { GoToLevel(-1); return; }
            if (IsKeyPressed(KeyboardKey.N)) { GoToLevel(+1); return; }
            #endif

            if (IsKeyPressed(KeyboardKey.R)) { GoToLevel(0); return; }

            var deltaPos = new Vec2I32(0, 0);
            if      (IsKeyDown(KeyboardKey.A) || IsKeyDown(KeyboardKey.Left))  deltaPos.X = -1;
            else if (IsKeyDown(KeyboardKey.D) || IsKeyDown(KeyboardKey.Right)) deltaPos.X = +1;
            else if (IsKeyDown(KeyboardKey.W) || IsKeyDown(KeyboardKey.Up))    deltaPos.Y = -1;
            else if (IsKeyDown(KeyboardKey.S) || IsKeyDown(KeyboardKey.Down))  deltaPos.Y = +1;

            if (deltaPos.X != 0 || deltaPos.Y != 0) {
                var entitiesToMove = _entityManager.ProcessTurn(deltaPos);
                if (entitiesToMove.Count != 0) {
                    _gameTurn = true;
                    _entityViewManager.Move(entitiesToMove, () => {
                        if (_entityManager.CheckLevelCompleted()) {
                            _entityViewManager.OnLevelCompleted(() => GoToLevel(+1));
                        } else {
                            _gameTurn = false;
                        }
                    });
                }
            }
        }
    }

    public override void Draw() {
        DrawRectangle(0, 0, Window.Size.X, Window.Size.Y, Game.WaterColor);

        BeginMode2D(_camera);
            _entityViewManager.Draw();
        EndMode2D();

        #if DEBUG
        const int fontSize = 24;
        var debugColor = GetColor(0x000000aa);
        for (var y = 0; y < _entityManager.Grid.Size.Y; y++) {
            for (var x = 0; x < _entityManager.Grid.Size.X; x++) {
                var entities = _entityManager.Grid.Get(new Vec2I32(x, y));
                if (entities.Count == 0) {
                    DrawTextEx(FontService.Instance.Get("default"), "0", new Vec2F32(x, y)*(fontSize + 8), fontSize, 0, debugColor);
                } else {
                    for (var i = 0; i < entities.Count; i++) {
                        var e = entities[i];
                        DrawTextEx(FontService.Instance.Get("default"), ((int)e.Type).ToString(), new Vec2F32(x*(fontSize + 8) + i*6, y*(fontSize + 8)), fontSize, 0, debugColor);
                    }
                }
            }
        }
        #endif
    }
}
