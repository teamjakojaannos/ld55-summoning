using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class GameManager : Node2D {
	public Player Player;

	public bool IntroSeen = false;

	[Export]
	public PackedScene PlayerTemplate;

	[Export]
	public Godot.Collections.Array<CardStats> playerDeck = new();

	public List<WhoIsSpeaking> godsBeaten = new();

	private readonly HashSet<string> AutoloadedNodes = new() { "DialogueBox", "GameManager", "CardGameLayer" };

	private bool TransitionInProgress = false;

	private Node previousLevel;

	private CardStats rewardIfYouWin;
	[Export]
	public AudioStreamPlayer CombatMusic;

	[Export]
	public AudioStreamPlayer EnterCombatMusic;


	[Export]
	public AudioStreamPlayer ExplocationMusic;

	[Export]
	public AudioStreamPlayer Kirjastomusa;

	[Export]
	public AudioStreamPlayer Krediittimusa;

	[Export]
	public CanvasLayer ScrollMenu;
	public GodTrigger CurrentlyFightingGod;

	[Export]
	public CanvasLayer Credits;

	public void StopAllMusic() {
		CombatMusic.Stop();
		EnterCombatMusic.Stop();
		ExplocationMusic.Stop();
		Kirjastomusa.Stop();
		Krediittimusa.Stop();
	}

	public override void _Process(double delta) {
		var nope = !Player.IsInCinematic && !GetNode<DialogueBox>("/root/DialogueBox/DialogueBox").IsInProgress && !Player.IsInFight;
		if (Input.IsActionJustPressed("esc") && nope) {
			ScrollMenu.Visible = !ScrollMenu.Visible;

			if (ScrollMenu.Visible) {
				ScrollMenu.GetNode<ScrollMenu>("ScrollMenu").SetScrolls(playerDeck);
			}
		}
	}


	public override void _EnterTree() {
		Credits.Visible = false;

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

			StopAllMusic();
			CallDeferred(nameof(StartMusic));
		}
	}

	public void StartMusic() {
		ExplocationMusic.Play();
	}

	public void ChangeToLevel(Node newLevel, Vector2 spawnPosition) {
		if (TransitionInProgress) {
			return;
		}

		IntroSeen = true;

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

		StopAllMusic();
		if (newLevel.Name == "WizardCellar") {
			// Wizard cellar has its own music
		} else if (newLevel.Name == "Library") {
			Kirjastomusa.Play();
		} else {
			ExplocationMusic.Play();
		}
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

		var enemyCards = enemy.cards.ToList();
		cardGame.PrepareCombat(playerCards, enemyCards, Player.MaxHp, enemy.MaxHp, enemy.GetNode<AnimatedSprite2D>("Sprite"));
		rewardIfYouWin = enemy.drops?.GenerateDrop();

		GetTree().CreateTimer(2.0f).Timeout += () => {
			cardGame.StartCombat();
		};
	}

	private void OnCardGameOver(bool playerWon) {
		Player.StartFadingToBlack();
		if (rewardIfYouWin != null && playerWon) {
			playerDeck.Add(rewardIfYouWin);
			rewardIfYouWin = null;
		}

		GetTree().CreateTimer(2.5f).Timeout += () => {
			Player.FullyFadeOut();


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

			Player.DoAfterBattleStuff(playerWon);
		};
	}

	public void WinGame() {
		var dialogue = GetNode<DialogueBox>("/root/DialogueBox/DialogueBox");

		dialogue.DialogueFinished += StartCredits;
		dialogue.Start(WhoIsSpeaking.Kyltti, new string[] {
			"Olet viineri.",
			"I mean",
			"You win!",
			"...",
			"Thanks for playing!"
		});
	}

	private void StartCredits() {
		StopAllMusic();
		Krediittimusa.Play();
		Player.StartFadingToBlack();


		GetTree().CreateTimer(2.5f).Timeout += () => {
			Player.FullyFadeOut();
			Credits.Visible = true;
		};
	}
}
