namespace Module2.Gameplay.Core;

/// <summary>
/// Gère la grille d'entités d'un niveau de jeu.
///
/// Permet de calculer les mouvements possibles en fonction
/// de l'action du joueur et de modifier le tableau 2D en
/// fonction.
///
/// Si un bloc est poussé en dehors de la grille, elle est
/// agrandie.
/// </summary>
public class EntityGrid {
    private List<Entity>[,]? _grid;
    public Vec2I32 Size { get; private set; }

    public void Init(Vec2I32 size) {
        Size = size;
        _grid = new List<Entity>[size.X, size.Y];

        for (var y = 0; y < size.Y; y++) {
            for (var x = 0; x < size.X; x++) {
                _grid[x, y] = [];
            }
        }
    }

    public bool ComputeMovements(Entity entity, Vec2I32 deltaPosition, Dictionary<Entity, Vec2I32> entitiesToMove) {
        var result = true;

        /*
        Si je suis un Guy :
          Si la case suivante a :
            une entité déplaçable, je regarde si elle peut être déplacée
            une entité non traversable, je ne bouge pas
          S'il n'y a pas d'entités (eau), je regarde pour aller sur celle d'après
            S'il n'y a que des entités traversables, je vais dessus

        Si je suis une entité déplaçable (crate) :
          Si la case suivante n'a que des entités traversables, je me déplace dessus
          Si la case suivante n'a pas d'entités (eau), je tombe dedans et je deviens traversable et plus déplaçable
        */
        var to = entity.GridPosition + deltaPosition;
        var targetEntities = Get(to);

        if (entity.Type == TileType.Guy) {
            if (targetEntities.Count != 0) {
                foreach (var targetEntity in targetEntities) {
                    if (targetEntity.Moveable) {
                        if (!ComputeMovements(targetEntity, deltaPosition, entitiesToMove)) {
                            result = false;
                            break;
                        }
                    } else if (!targetEntity.Walkable) {
                        result = false;
                        break;
                    }
                }
            } else {
                deltaPosition *= 2;
                to = entity.GridPosition + deltaPosition;
                targetEntities = Get(to);
                if (!IsCellWalkable(targetEntities)) {
                    result = false;
                }
            }
        } else if (entity.Moveable) {
            if (targetEntities.Count != 0) {
                if (!IsCellWalkable(targetEntities)) {
                    result = false;
                }
            } else {
                entity.Moveable = false;
                entity.Walkable = true;
                entity.Layer--;
            }
        } else {
            result = false;
        }

        if (result) entitiesToMove[entity] = deltaPosition;

        return result;
    }

    public Dictionary<Entity, Vec2I32> UpdateGrid(Dictionary<Entity, Vec2I32> entitiesToMove) {
        // Step 1: Retrait des entités à déplacer
        foreach (var e in entitiesToMove.Keys) {
            Remove(e.GridPosition, e);
        }

        // Step 2: Calcul de la nouvelle grille
        var currentBounds = new RecI32(Vec2I32.Zero, Size);
        var newBounds = currentBounds;
        foreach (var entry in entitiesToMove) {
            var e = entry.Key;
            var deltaMove = entry.Value;
            var to = e.GridPosition + deltaMove;

            var minX = Math.Min(to.X, newBounds.From.X);
            var minY = Math.Min(to.Y, newBounds.From.Y);
            var maxX = Math.Max(to.X + 1, newBounds.To.X);
            var maxY = Math.Max(to.Y + 1, newBounds.To.Y);

            newBounds = new RecI32(new Vec2I32(minX, minY), new Vec2I32(maxX, maxY));
        }
        if (newBounds != currentBounds) {
            StretchGrid(newBounds);
        }

        // Step 3: Remettre les entités sur la bonne case
        var result = new Dictionary<Entity, Vec2I32>();
        foreach (var entry in entitiesToMove) {
            var e = entry.Key;
            var deltaMove = entry.Value;
            var to = e.GridPosition + deltaMove - newBounds.From;
            Add(to, e);
            e.GridPosition = to;
            result.Add(e, deltaMove);
        }
        return result;
    }

    public List<Entity> Get(Vec2I32 position) {
        return InBounds(position) ? _grid![position.X, position.Y] : [];
    }

    public void Add(Vec2I32 position, Entity e) {
        _grid![position.X, position.Y].Add(e);
    }

    public void Remove(Vec2I32 position, Entity e) {
        _grid![position.X, position.Y].Remove(e);
    }

    private bool InBounds(Vec2I32 position) {
        return (
            position.X >= 0 && position.X < Size.X &&
            position.Y >= 0 && position.Y < Size.Y
        );
    }

    private static bool IsCellWalkable(IEnumerable<Entity> entities) {
        var result = false;

        foreach (var e in entities) {
            if (e.Walkable) {
                result = true;
            } else {
                result = false;
                break;
            }
        }

        return result;
    }

    private void StretchGrid(RecI32 newBounds) {
        var newSize = newBounds.Size;

        var newGrid = new List<Entity>[newSize.X, newSize.Y];

        for (var y = 0; y < newSize.Y; y++) {
            for (var x = 0; x < newSize.X; x++) {
                var position = new Vec2I32(x, y);
                var entities = Get(position + newBounds.From);
                newGrid[x, y] = entities;
                foreach (var e in entities) {
                    e.GridPosition = position;
                }
            }
        }

        _grid = newGrid;
        Size = newSize;
    }
}
