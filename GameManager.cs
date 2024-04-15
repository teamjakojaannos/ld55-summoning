using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class GameManager : Node2D {
	public Player Player;

	[Export]
	public PackedScene PlayerTemplate;

	private readonly HashSet<string> AutoloadedNodes = new() { "DialogueBox", "GameManager", "CardGameLayer" };

	private bool TransitionInProgress = false;

	private Node previousLevel;

	public override void _EnterTree() {
		var playerNode = GetTree().Root.FindChild("Player", true, false);

		var cardGameLayer = GetTree().Root.GetNode<CanvasLayer>("CardGameLayer");
		cardGameLayer.Visible = false;
		var cardGame = cardGameLayer.GetNode<Cardgame>("Cardgame");
		cardGame.CardgameOver += OnCardGameOver;


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

	public void StartFight(List<CardStats> playerCards, EncounterTrigger enemy) {
		var root = GetTree().Root;
		var children = root.GetChildren();
		var expectedCount = AutoloadedNodes.Count + 1;
		if (children.Count != expectedCount) {
			GD.Print($"Expected root node to have {expectedCount} children, found {children.Count}. Dragons ahead...");
		}

		var list = new List<Node>(children);
		list.RemoveAll((node) => AutoloadedNodes.Contains(node.Name));

		if (list.Count == 0) {
			GD.PrintErr("Can't find level node in root.");
			return;
		}

		enemy.EmitDeleteSignal();

		previousLevel = list[0];
		previousLevel.RemoveChild(Player);
		root.RemoveChild(previousLevel);

		var cardGameLayer = GetTree().Root.GetNode<CanvasLayer>("CardGameLayer");
		var cardGame = cardGameLayer.GetNode<Cardgame>("Cardgame");

		cardGameLayer.Visible = true;
		cardGame.AddChild(Player);

		GetTree().CreateTimer(2.0f).Timeout += () => {
			var enemyCards = enemy.cards.ToList();
			cardGame.StartCombat(playerCards, enemyCards, Player.MaxHp, enemy.MaxHp);
		};
	}

	private void OnCardGameOver(bool playerWon) {

		var cardGameLayer = GetTree().Root.GetNode<CanvasLayer>("CardGameLayer");
		var cardGame = cardGameLayer.GetNode<Cardgame>("Cardgame");

		cardGameLayer.Visible = false;
		cardGame.RemoveChild(Player);

		if (previousLevel == null) {
			GD.Print("Something went wrong, level reference is null.");
			return;
		}

		var root = GetTree().Root;
		root.AddChild(previousLevel);
		previousLevel.AddChild(Player);

		previousLevel = null;

		Player.DoAfterBattleStuff();

		if (!playerWon) {
			// TODO: teleport to graveyard or something
			GD.Print("You lost!");
		}
	}
}
