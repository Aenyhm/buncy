namespace Module2.Levels;

public static class LevelService {
    public static readonly List<Level> Levels = [];
    public static int CurrentIndex { get; private set; }
    public static Level Current { get; set; } = new();
    
    public static void Init() {
        foreach (var id in LevelManager.ReadOrderFile()) {
            var level = LevelManager.Load(id);
            Register(level);
        }
    }
    
    public static void Register(Level level) {
        Levels.Add(level);
    }

    public static void Remove(Level level) {
        Levels.Remove(level);
    }
 
    public static void GoTo(int index) {
        if (index < 0) index += Levels.Count;
        else if (index >= Levels.Count) index -= Levels.Count;
        
        CurrentIndex = index;
        Current = Levels[index];
    }
}
