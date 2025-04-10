using Module2.Levels;

namespace Module2.Editor;

/// Ce module permet de gérer les layers de l'éditeur.
/// Un layer stocke les tiles dans un dictionnaire plutôt
/// qu'une grille pour optimiser l'ajout et permettre
/// d'afficher une grille "infinie".
///
/// On peut remplir des layers depuis des tableaux 2D des
/// niveaux et à l'inverse convertir le dictionnaire en
/// tableau 2D.

public class EditorLayer {
    public readonly Dictionary<Vec2I32, TileType> Items = [];
    public readonly TileType[] TileTypes;
    public TileType CurrentTileId { get; set; }

    public EditorLayer(TileType[] tileTypes) {
        TileTypes = tileTypes;
    }

    public void Set(Vec2I32 position, TileType value) {
        if (value == 0) Items.Remove(position);
        else Items[position] = value;
    }

    public TileType Get(Vec2I32 position) {
        Items.TryGetValue(position, out var result);
        return result;
    }
}

public class EditorLayerManager {
    public readonly EditorLayer[] Layers;
    public byte CurrentLayerId { get; set; }

    public EditorLayerManager() {
        Layers =  new EditorLayer[Game.TilesByLayer.Length];
        for (var i = 0; i < Game.TilesByLayer.Length; i++) {
            Layers[i] = new EditorLayer(Game.TilesByLayer[i]);
        }
    }

    public void Clear() {
        foreach (var layer in Layers) {
            layer.Items.Clear();
        }
    }

    public void FillFromLevel(Level level) {
        Clear();

        for (var layerIndex = 0; layerIndex < level.Layers.Length; layerIndex++) {
            var layer = level.Layers[layerIndex];
            for (var y = 0; y < level.Size.Y; y++) {
                for (var x = 0; x < level.Size.X; x++) {
                    Layers[layerIndex].Set(new Vec2I32(x, y), layer[y, x]);
                }
            }
        }
    }

    public TileType[][,] ShrinkToLayers() {
        var result = new TileType[Layers.Length][,];

        var filledRec = GetFilledRectangle();
        var cols = Math.Max(0, filledRec.Width - filledRec.X + 1);
        var rows = Math.Max(0, filledRec.Height - filledRec.Y + 1);

        for (byte layerIndex = 0; layerIndex < Layers.Length; layerIndex++) {
            var layer = Layers[layerIndex];
            var shrunkenArray = new TileType[(int)rows, (int)cols];

            foreach (var key in layer.Items.Keys) {
                var col = key.X - filledRec.X;
                var row = key.Y - filledRec.Y;
                shrunkenArray[(int)row, (int)col] = layer.Get(key);
            }

            result[layerIndex] = shrunkenArray;
        }

        return result;
    }

    public Rectangle GetFilledRectangle() {
        var minRow = int.MaxValue;
        var maxRow = int.MinValue;
        var minCol = int.MaxValue;
        var maxCol = int.MinValue;

        var hasTile = false;
        foreach (var layer in Layers) {
            if (!hasTile) hasTile = layer.Items.Count > 0;

            foreach (var key in layer.Items.Keys) {
                var col = key.X;
                var row = key.Y;

                if (col < minCol) minCol = col;
                if (col > maxCol) maxCol = col;
                if (row < minRow) minRow = row;
                if (row > maxRow) maxRow = row;
            }
        }

        if (!hasTile) {
            minCol = minRow = maxCol = maxRow = 0;
        }

        return new Rectangle(minCol, minRow, maxCol, maxRow);
    }
}
