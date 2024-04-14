using System.Collections.Generic;
using Godot;

public partial class GameManager : Node2D
{
	public Player Player;

	[Export]
	public PackedScene PlayerTemplate;

	private readonly HashSet<string> AutoloadedNodes = new() { "DialogueBox", "GameManager" };

	private bool TransitionInProgress = false;

    public override void _EnterTree() {
        var playerNode = GetTree().Root.FindChild("Player", true, false);
		if (playerNode is Player player) {
			Player = player;
		} else {
			Player = PlayerTemplate.Instantiate<Player>();

			var startNode = GetTree().Root.FindChild("PlayerStart", true, false);
			if (startNode is Marker2D startMarker) {
				var newParent = startMarker.GetParent();

				newParent.AddChild(Player);
				Player.GlobalPosition = startMarker.GlobalPosition;
			} else {
				GD.PushError("Could not find PlayerStart");
			}
		}
    }

	public void ChangeToLevel(Node newLevel, Vector2 spawnPosition) {
		if (TransitionInProgress) {
			return;
		}

		var root = GetTree().Root;
		var children = root.GetChildren();
		var expectedCount = AutoloadedNodes.Count + 1;
		if (children.Count != expectedCount) {
			GD.Print($"Expected root node to have {expectedCount} children, found {children.Count}. Dragons ahead...");
		}

		var list = new List<Node>(children);
		list.RemoveAll((node) => AutoloadedNodes.Contains(node.Name));

		if (list.Count == 0) {
			GD.PrintErr("Can't find level node in root, canceling level transition.");
			return;
		}

		TransitionInProgress = true;

		var oldLevel = list[0];
		oldLevel.RemoveChild(Player);
		Player.Position = spawnPosition;
		newLevel.AddChild(Player);

		root.RemoveChild(oldLevel);
		oldLevel.QueueFree();
		root.AddChild(newLevel);

		TransitionInProgress = false;
	}
}
