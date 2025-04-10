namespace Module2.Levels;

/// <summary>
/// Un niveau est composé d'un ID qui sert de nom de fichier,
/// d'un nom qui permet de le retrouver dans l'éditeur et de
/// plusieurs layers qui sont des tableaux 2D représentant les
/// éléments dessus.
/// </summary>
public class Level {
    private static readonly Vec2I32 _minSize = new(2, 2);
    private static readonly Vec2I32 _maxSize = new(30, 30);

    public string Id { get; set; }
    public string Name { get; set; }
    public TileType[][,] Layers { get; set; }

    public Level() {
        Id = string.Empty;
        Name = string.Empty;
        Layers = [];
    }

    public Vec2I32 Size {
        get {
            var maxHeight = 0;
            var maxWidth = 0;
            foreach (var layer in Layers) {
                maxHeight = Math.Max(layer.GetLength(0), maxHeight);
                maxWidth = Math.Max(layer.GetLength(1), maxWidth);
            }
            return new Vec2I32(maxWidth, maxHeight);
        }
    }
    public bool IsTooSmall => Size.X < _minSize.X || Size.Y < _minSize.Y;
    public bool IsTooLarge => Size.X > _maxSize.X || Size.Y > _maxSize.Y;

    public override int GetHashCode() {
        return Id.GetHashCode();
    }
}
