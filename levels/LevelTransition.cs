using Godot;

public partial class LevelTransition : Area2D {

	[Export]
	private string levelName;

	public override void _Ready() {
		if (levelName == null) {
			GD.PrintErr("You forgor to set level transition's destination level :skull_emoji:");
		}
	}

	public void OnBodyEntered(PhysicsBody2D body) {
		if (body is Player) {
			if (levelName == null) {
				GD.PrintErr("No level scene set, can't switch scene!");
				return;
			}

			var levelScene = GD.Load<PackedScene>(levelName);
			var levelInstance = levelScene.Instantiate();

			Vector2 spawnPoint = new();

			var startNode = levelInstance.FindChild("PlayerStart", true, false);
			if (startNode != null && startNode is Node2D node2d) {
				spawnPoint = node2d.Position;
			}

			var gameManager = GetNode<GameManager>("/root/GameManager");
			gameManager.CallDeferred(GameManager.MethodName.ChangeToLevel, levelInstance, spawnPoint);
		}
	}
}
