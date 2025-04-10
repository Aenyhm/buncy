namespace Module2;

public static class Window {
    public const float AspectRatio = 14f/9f;
    public static Vec2I32 Size { get; private set; }
    public static bool ShouldClose { get; set; }
    public static event Action? OnResized;

    private const string _title = "Buncy";

    // Remplissage de l'écran en hauteur
#if DEBUG
    private const float _monitorFillRatio = .65f;
#else
    private const float _monitorFillRatio = .8f;
#endif

    /// <summary>
    /// Calcule les dimensions et la position de la fenêtre de jeu par rapport à l'écran.
    ///
    /// Au lieu de mettre un taille de fenêtre en dur, je préfère passer par un
    /// pourcentage d'écran utilisé pour dimensionner la fenêtre de jeu par défaut :
    /// car autant une fenêtre de 800x600 sur un écran HD est correcte, autant sur un
    /// écran 4K elle est peu visible.
    /// </summary>
    public static void Init()
    {
        SetConfigFlags(ConfigFlags.ResizableWindow);
        InitWindow(0, 0, _title);
        SetTargetFPS(60);
        SetExitKey(KeyboardKey.Null);

        var monitorId = GetCurrentMonitor();
        var monitorSize = new Vec2I32(GetMonitorWidth(monitorId), GetMonitorHeight(monitorId));

        var windowHeight = monitorSize.Y * _monitorFillRatio;
        var windowWidth = windowHeight * AspectRatio;

        Size = new Vec2I32((int)windowWidth, (int)windowHeight);

        SetWindowSize(Size.X, Size.Y);

#if DEBUG
        SetWindowPosition(1, 50); // Pour le contour de la fenêtre sur Windows 11
#else
        var windowPosition = (monitorSize - Size)/2;
        SetWindowPosition(windowPosition.X, windowPosition.Y);
#endif
    }

    public static void Update() {
        if (IsWindowResized()) {
            Size = new Vec2I32(GetScreenWidth(), GetScreenHeight());
            OnResized?.Invoke();
        }
    }
}
