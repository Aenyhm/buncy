namespace Module2.Toolbox;

public struct Sprite {
    public readonly Texture2D Texture;
    public RecI32 Coordinates{ get; set; }
    public Vec2I32 Size { get; set; }
    public Vec2F32 Origin { get; set; }

    public Sprite(Texture2D texture, RecI32 coordinates, Vec2I32 size, Vec2I32 originOffset) {
        Texture = texture;
        Coordinates = coordinates;
        Size = size;
        Origin = size.ToVec2F32()/2f + originOffset.ToVec2F32();
    }

    public Sprite(Texture2D texture, RecI32 coordinates, Vec2I32 size) : this(
        texture,
        coordinates,
        size,
        Vec2I32.Zero
    ) {}

    public Sprite(Texture2D texture, Vec2I32 size, Vec2I32 originOffset) : this(
        texture,
        new RecI32(Vec2I32.Zero, new Vec2I32(texture.Width, texture.Height)),
        size,
        originOffset
    ) {}

    public Sprite(Texture2D texture, RecI32 coordinates) : this(
        texture,
        coordinates,
        coordinates.Size
    ) {}

    public Sprite(Texture2D texture, Vec2I32 size) : this(
        texture,
        new RecI32(Vec2I32.Zero, new Vec2I32(texture.Width, texture.Height)),
        size
    ) {}

    public Sprite(Texture2D texture) : this(
        texture,
        new RecI32(Vec2I32.Zero, new Vec2I32(texture.Width, texture.Height))
    ) {}
}

/// <summary>
/// Permet de regrouper des sprites ensemble.
/// Utile pour un tileset ou une spritesheet.
/// </summary>
public readonly struct SpriteCollection {
    public readonly OrderedDictionary<string, Sprite> SpritesByName;

    public SpriteCollection(OrderedDictionary<string, Sprite> spritesByName) {
        SpritesByName = spritesByName;
    }

    public SpriteCollection(Sprite sprite) {
        SpritesByName = new OrderedDictionary<string, Sprite> {{ "default", sprite }};
    }

    public static SpriteCollection CreateFromTileset(
        Texture2D texture, Vec2I32 tileSetSize, Vec2I32 tileSize, Vec2I32 tileDisplaySize, Vec2I32 originOffset,
        string[] names
    ) {
        var spritesByName = new OrderedDictionary<string, Sprite>(tileSetSize.X*tileSetSize.Y);
        for (var y = 0; y < tileSetSize.Y; y++) {
            for (var x = 0; x < tileSetSize.X; x++) {
                var spriteIndex = tileSetSize.X*y + x;
                var startCoords = new Vec2I32(x, y);
                var endCoords = new Vec2I32(x + 1, y + 1);
                var sprite = new Sprite(
                    texture,
                    new RecI32(startCoords*tileSize, endCoords*tileSize),
                    tileDisplaySize,
                    originOffset
                );
                var spriteName = names.Length > spriteIndex ? names[spriteIndex] : spriteIndex.ToString();
                spritesByName.Add(spriteName, sprite);
            }
        }

        return new SpriteCollection(spritesByName);
    }
    
    // À utiliser à la place de First().Value (Linq) pour éviter de l'allocation mémoire.
    public Sprite GetFirstSprite() {
        foreach (var pair in SpritesByName) return pair.Value;
        
        throw new InvalidOperationException("No sprite in this collection.");
    }
}
