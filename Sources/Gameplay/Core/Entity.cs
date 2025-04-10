namespace Module2.Gameplay.Core;

/// <summary>
/// Représente chaque élément de la grille de jeu.
///
/// Id unique utilisé comme HashCode pour optimiser
/// le stockage en clé de dictionnaire.
/// </summary>
public class Entity {
    public Vec2I32 GridPosition { get; set; }
    public TileType Type { get; set; }
    public int Id { get; set; }
    public byte Layer { get; set; }
    public bool Moveable { get; set; }
    public bool Walkable { get; set; }

    public override int GetHashCode() => Id;
}
