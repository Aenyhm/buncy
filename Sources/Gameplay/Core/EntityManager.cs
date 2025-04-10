using Module2.Levels;

namespace Module2.Gameplay.Core;

public class EntityManager {
    public EntityGrid Grid { get; private set; } = new();
    
    private ObjectPool<Entity> _pool = new();
    private int _currentId;
    private Entity? _player;

    public IEnumerable<Entity> All => _pool.AllUsed;
    
    public void SetupLevel(Level level) {
        Grid.Init(LevelService.Current.Size);
        
        for (byte layerIndex = 0; layerIndex < level.Layers.Length; layerIndex++) {
            var layer = level.Layers[layerIndex];
            for (var y = 0; y < layer.GetLength(0); y++) {
                for (var x = 0; x < layer.GetLength(1); x++) {
                    var tileType = layer[y, x];
                    if (tileType == TileType.None) continue;
                    
                    var position = new Vec2I32(x, y);
                    var e = Create(tileType, position, layerIndex);
                    Grid.Add(position, e);
                    
                    if (e.Type == TileType.Guy) _player = e;
                }
            }
        }
    }
    
    public Dictionary<Entity, Vec2I32> ProcessTurn(Vec2I32 deltaPos) {
        var entitiesToMove = new Dictionary<Entity, Vec2I32>();
        if (Grid.ComputeMovements(_player!, deltaPos, entitiesToMove)) {
            entitiesToMove = Grid.UpdateGrid(entitiesToMove);
        }
        
        return entitiesToMove;
    }
    
    
    public bool CheckLevelCompleted() {
        var entities = Grid.Get(_player!.GridPosition);
        
        return entities.Any(e => e.Type == TileType.Exit);
    }
    
    public void Clear() {
        _pool.DestroyAll();
    }
    
    private Entity Create(TileType tileType, Vec2I32 gridPosition, byte layerIndex) {
        var e = _pool.Create();
        e.Id = ++_currentId;
        e.Type = tileType;
        e.Layer = layerIndex;
        e.GridPosition = gridPosition;
        e.Moveable = tileType == TileType.Crate;
        e.Walkable = tileType is TileType.Sand or TileType.Exit or TileType.Waterlily;
        
        return e;
    }
}
