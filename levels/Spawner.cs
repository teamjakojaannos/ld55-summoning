using System.Collections.Generic;
using Godot;
using Godot.Collections;

public partial class Spawner : TileMap {
    [Export]
    public Array<PackedScene> SpawnPool = new();

	[Export]
	public int SpawnCount = 100;

    public override void _Ready() {
        for (var i = 0; i < SpawnCount; ++i) {
			Spawn();
		}
    }

    public void Spawn() {
        var spawnTiles = new List<Vector2I>();
        foreach (var tile in GetUsedCells(1)) {
            var data = GetCellTileData(1, tile);

            if (data != null && data.GetCustomData("Allow Spawning").AsBool()) {
                spawnTiles.Add(tile);
            }
        }

        if (spawnTiles.Count == 0) {
            GD.PushWarning("No spawn tiles available.");
            return;
        }

        var rng = new RandomNumberGenerator();
        var spawnTile = spawnTiles[rng.RandiRange(0, spawnTiles.Count - 1)];

        var template = SpawnPool[rng.RandiRange(0, SpawnPool.Count - 1)];

        var spawned = template.Instantiate<Node2D>();
        spawned.GlobalPosition = new Vector2(spawnTile.X * 32.0f + 16.0f, spawnTile.Y * 32.0f + 16.0f);
        AddChild(spawned);
    }
}
