using Module2.Gameplay.Core;

namespace Module2.Gameplay.UI;

public class EntityViewManager {
    private static readonly Color _sandColor = GetColor(0xc3be25ff);
    
    private readonly ObjectPool<EntityView> _pool = new();
    private readonly Dictionary<Entity, EntityView> _viewsByEntity = [];
    private EntityView? _playerView;

    private const float _moveDuration = 0.3f;
    private const float _jumpDuration = 0.6f;
    
    public void CreateFromEntities(EntityManager entityManager) {
        foreach (var e in entityManager.All) {
            var view = _pool.Create();
            
            if (e.Type == TileType.Sand) {
                var spriteIndex = 0;
                if (entityManager.Grid.Get(new Vec2I32(e.GridPosition.X, e.GridPosition.Y - 1)).Any(x => x.Type == TileType.Sand)) spriteIndex += 1;
                if (entityManager.Grid.Get(new Vec2I32(e.GridPosition.X + 1, e.GridPosition.Y)).Any(x => x.Type == TileType.Sand)) spriteIndex += 2;
                if (entityManager.Grid.Get(new Vec2I32(e.GridPosition.X, e.GridPosition.Y + 1)).Any(x => x.Type == TileType.Sand)) spriteIndex += 4;
                if (entityManager.Grid.Get(new Vec2I32(e.GridPosition.X - 1, e.GridPosition.Y)).Any(x => x.Type == TileType.Sand)) spriteIndex += 8;

                view.Sprite = TileSpriteCollectionService.Instance.Get(e.Type).SpritesByName[spriteIndex.ToString()];
            } else {
                view.Sprite = TileSpriteCollectionService.Instance.Get(e.Type).SpritesByName.First().Value;
            }
            
            view.Color = e.Type == TileType.Exit ? _sandColor : Color.White;

            view.Position = e.GridPosition.ToVec2F32()*Game.TileSide;
            view.DisplayLayer = e.Layer;
            _viewsByEntity.Add(e, view);

            if (e.Type == TileType.Guy) _playerView = view;
        }
    }
    
    public void Clear() {
        _pool.DestroyAll();
        _viewsByEntity.Clear();
    }

    public void Move(Dictionary<Entity, Vec2I32> entitiesToMove, Action onCompleted) {
        var maxDuration = 0f;
        
        foreach (var (e, deltaMove) in entitiesToMove) {
            if (_viewsByEntity.TryGetValue(e, out var view)) {
                var jump = Math.Abs(deltaMove.X) == 2 || Math.Abs(deltaMove.Y) == 2;
                
                if (e.Type == TileType.Guy) {
                    string spriteName;
                    if (deltaMove.X < 0) spriteName= "left";
                    else if (deltaMove.X > 0) spriteName = "right";
                    else if (deltaMove.Y < 0) spriteName = "up";
                    else spriteName = "down";
                        
                    view.Sprite = TileSpriteCollectionService.Instance.Get(e.Type).SpritesByName[spriteName];
                }
                
                if (e.Layer == 0 && view.DisplayLayer > 0) {
                    view.StartMoveAnimation(deltaMove, _moveDuration, Easings.InOutQuad, () => SinkCrate(view));
                    maxDuration = Math.Max(maxDuration, _moveDuration);
                } else if (jump) {
                    view.StartMoveAnimation(deltaMove, _jumpDuration, Easings.InBounce);
                    maxDuration = Math.Max(maxDuration, _jumpDuration);
                } else {
                    view.StartMoveAnimation(deltaMove, _moveDuration, Easings.InOutQuad);
                    maxDuration = Math.Max(maxDuration, _moveDuration);
                }
            }
        }
        
        TimedActionManager.Add(new TimedAction(maxDuration, onCompleted));
    }
    
    private static void SinkCrate(EntityView view) {
        var sprite = view.Sprite;
        var coordinatesTo = sprite.Coordinates.To;
        coordinatesTo.Y -= 2;
        sprite.Coordinates = new RecI32(sprite.Coordinates.From, coordinatesTo);
        sprite.Size = new Vec2I32(sprite.Size.X, sprite.Size.Y - 2);
        sprite.Origin = new Vec2F32(sprite.Origin.X, sprite.Origin.Y - 8);
        
        view.Sprite = sprite;
        view.DisplayLayer = 0;
    }

    public void Draw() {
        for (byte layerIndex = 0; layerIndex < Game.TilesByLayer.Length; layerIndex++) {
            foreach (var view in _viewsByEntity.Values) {
                if (view.DisplayLayer == layerIndex) { 
                    view.Draw();
                }
            }
        }
    }

    public void OnLevelCompleted(Action onComplete) {
        TimedActionManager.Add(new EndAnimation(_playerView!, 1.5f, onComplete));
    }
}
