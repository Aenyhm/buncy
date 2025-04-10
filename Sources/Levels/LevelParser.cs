using System.Text;

namespace Module2.Levels;

public static class LevelParser {
    private const string _commandSeparator = ":";
    private const string _layerSeparator = "---";

    public static string Serialize(Level level) {
        var data = new StringBuilder();
        data.AppendLine("v1");
        data.AppendLine($"{_commandSeparator}name {level.Name}");
        data.AppendLine($"{_commandSeparator}layers");
        
        var layersStrings = new List<string>();
        foreach (var layer in level.Layers) {
            layersStrings.Add(SerializeLayer(level, layer));
        }
        data.AppendJoin($"\n{_layerSeparator}\n", layersStrings);

        return data.ToString();
    }

    private static string SerializeLayer(Level level, TileType[,] layer) {
        var size = level.Size;

        var lines = new List<string>();
        

        for (var y = 0; y < size.Y; y++) {
            var line = new char[size.X];
            for (var x = 0; x < size.X; x++) {
                line[x] = (char)('0' + (byte)layer[y, x]);
            }
            lines.Add(new string(line));
        }

        return string.Join('\n', lines);
    }
    
    public static Level Deserialize(string id, string content) {
        var level = new Level();
        level.Id = id;
        
        var commands = content.Split($"\n{_commandSeparator}");
        //var version = commands[0];

        for (var i = 1; i < commands.Length; i++) {
            var command = commands[i];
            var commandParts = command.Split(null, count: 2); // null = whitespaces
            var commandName = commandParts[0];
            var commandValue = commandParts[1].Trim();

            switch (commandName) {
                case "name":
                    level.Name = commandValue;
                    break;
                case "layers": {
                    var layersContent = commandValue.Split(_layerSeparator);
                    level.Layers = new TileType[layersContent.Length][,];
                    for (var j = 0; j < layersContent.Length; j++) {
                        var layerData = layersContent[j].Trim();
                        level.Layers[j] = DeserializeLayer(layerData);
                    }
                    break;
                }
            }
        }

        return level; 
    }
    
    private static TileType[,] DeserializeLayer(string input) {
        var rows = SplitLines(input);

        var width = rows[0].Length;
        var height = rows.Length;

        var result = new TileType[height, width];
        
        for (var y = 0; y < height; y++) {
            var row = rows[y];
            for (var x = 0; x < width; x++) {
                var c = row[x];
                result[y, x] = (TileType)char.GetNumericValue(c);
            }
        }

        return result;
    }
    
    private static string[] SplitLines(string input, int count = int.MaxValue) {
        return input.Split(["\r\n", "\r", "\n"], count, StringSplitOptions.None);
    }
}
