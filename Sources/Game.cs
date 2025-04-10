global using Raylib_cs;
global using static Raylib_cs.Raylib;
global using Module2.Toolbox;
using Module2.Gameplay;
using Module2.Levels;

#if DEBUG
using Module2.Editor;
#endif

namespace Module2;

public enum TileType : byte {
    None = 0,
    Sand = 1,
    Guy = 2,
    Exit = 3,
    Crate = 4,
    Rock = 5,
    Waterlily = 6,
}

public class SpriteCollectionService : ServiceLocator<SpriteCollectionService, string, SpriteCollection>;
public class TileSpriteCollectionService : ServiceLocator<TileSpriteCollectionService, TileType, SpriteCollection>;
public class FontService : ServiceLocator<FontService, string, Font>;

public static class Game {
    public const int TileSide = 32;
    public static readonly Color WaterColor = GetColor(0x00a8daff);
    public static readonly TileType[][] TilesByLayer = [
        [TileType.Sand, TileType.Waterlily],
        [TileType.Exit, TileType.Crate, TileType.Rock],
        [TileType.Guy]
    ];

    private static void Main() {
        #if !DEBUG
        SetTraceLogLevel(TraceLogLevel.Error);
        #endif

        Window.Init();
        LevelService.Init();
        LoadImages();
        FontService.Instance.Register("default", LoadFontEx("Assets/rellion.regular.otf", 128, null, 255));

        #if DEBUG
        SceneService.Instance.Register(new EditorScene());
        #endif
        SceneService.Instance.Register(new GameplayScene());
        SceneService.Instance.Register(new VictoryScene());
        SceneService.Instance.GoTo<GameplayScene>();

        while (!(Window.ShouldClose || WindowShouldClose())) {
            Window.Update();
            var dt = GetFrameTime();
            SceneService.Instance.Current.Update(dt);
            NotificationManager.Update(dt);
            TimedActionManager.Update(dt);

            BeginDrawing();
                ClearBackground(Color.Black);
                SceneService.Instance.Current.Draw();
                NotificationManager.Draw();
            EndDrawing();
        }

        CloseWindow();
    }

    private static void LoadImages() {
        SpriteCollectionService.Instance.Register("sand", SpriteCollection.CreateFromTileset(
            LoadTexture("Assets/Images/SandOverWaterTileset.png"),
            new Vec2I32(4, 4),
            new Vec2I32(32, 32),
            new Vec2I32(TileSide, TileSide),
            Vec2I32.Zero,
            []
        ));
        SpriteCollectionService.Instance.Register("exit", new SpriteCollection(
            new Sprite(LoadTexture("Assets/Images/portal.png"), new Vec2I32(48, 24))
        ));
        SpriteCollectionService.Instance.Register("guy", SpriteCollection.CreateFromTileset(
            LoadTexture("Assets/Images/character.png"),
            new Vec2I32(4,  1),
            new Vec2I32(48,  48),
            new Vec2I32(64, 64),
            new Vec2I32(0, 10),
            ["down", "up", "left", "right"]
        ));
        SpriteCollectionService.Instance.Register("crate", new SpriteCollection(
            new Sprite(LoadTexture("Assets/Images/crate.png"), new Vec2I32(TileSide - 8, TileSide - 8), new Vec2I32(0, 10))
        ));
        SpriteCollectionService.Instance.Register("rock", new SpriteCollection(
            new Sprite(LoadTexture("Assets/Images/rock.png"), new Vec2I32(TileSide, TileSide))
        ));
        SpriteCollectionService.Instance.Register("waterlily", new SpriteCollection(
            new Sprite(LoadTexture("Assets/Images/waterlily.png"), new Vec2I32(TileSide - 6, TileSide - 6))
        ));

        TileSpriteCollectionService.Instance.Register(TileType.Sand, SpriteCollectionService.Instance.Get("sand"));
        TileSpriteCollectionService.Instance.Register(TileType.Guy, SpriteCollectionService.Instance.Get("guy"));
        TileSpriteCollectionService.Instance.Register(TileType.Exit, SpriteCollectionService.Instance.Get("exit"));
        TileSpriteCollectionService.Instance.Register(TileType.Crate, SpriteCollectionService.Instance.Get("crate"));
        TileSpriteCollectionService.Instance.Register(TileType.Rock, SpriteCollectionService.Instance.Get("rock"));
        TileSpriteCollectionService.Instance.Register(TileType.Waterlily, SpriteCollectionService.Instance.Get("waterlily"));
    }
}
