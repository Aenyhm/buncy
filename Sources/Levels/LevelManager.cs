namespace Module2.Levels;

public static class LevelManager {
    private const string _levelFolder = "Assets/Levels";
    private const string _orderFilePath = "Assets/levels_order.txt";
    private const int _idRandomPartLength = 6;
    
    private static string GetLevelPath(string id) => $"{_levelFolder}/{id}.level";

    public static string[] ReadOrderFile() {
        return File.ReadAllLines(_orderFilePath);
    }
        
    public static void Save(Level level, bool isNew) {
        var content = LevelParser.Serialize(level);
        
        using (var stream = new StreamWriter(GetLevelPath(level.Id))) {
            stream.Write(content);
        }
        
        if (isNew) {
            using (var stream = new StreamWriter(_orderFilePath, true)) {
                stream.WriteLine(level.Id);
            }
            LevelService.Register(level);
        }
    }
    
    public static Level Load(string id) {
        Level level;
        
        using (var reader = new StreamReader(GetLevelPath(id))) {
            var content = reader.ReadToEnd();
            level = LevelParser.Deserialize(id, content);
        }
        
        return level;
    }
    
    public static void Delete(Level level) {
        LevelService.Remove(level);
        File.Delete(GetLevelPath(level.Id));
        
        UpdateOrderFile();
    }

    public static void Permute(int index1, int index2) {
        var tmp = LevelService.Levels[index1];
        LevelService.Levels[index1] = LevelService.Levels[index2];
        LevelService.Levels[index2] = tmp;
        
        UpdateOrderFile();
    }
    
    public static string GenerateLevelId() {
        var guid = Guid.NewGuid().ToByteArray();
        var randomPart = Convert.ToBase64String(guid)
            .Replace("/", "")
            .Replace("+", "")
            .Substring(0, _idRandomPartLength);
        var timePart = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
        return $"{randomPart}{timePart}";
    }
    
    private static void UpdateOrderFile() {
        using (var stream = new StreamWriter(_orderFilePath)) {
            foreach (var level in LevelService.Levels) {
                stream.WriteLine(level.Id);
            }
        }
    }
}
