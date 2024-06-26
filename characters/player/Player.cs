using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Player : CharacterBody2D {
	[Export]
	public float MoveSpeed = 32.0f;

	[Export]
	public AnimatedSprite2D Sprite;

	[Export]
	public Overlay Blackout;

	[Export]
	public float FadeSmoothIn = 0.05f;

	[Export]
	public float FadeSmoothOut = 0.1f;

	[Export]
	public float FadeSmoothInTime = 3.0f;

	[Export]
	public float MaxFade = 200.0f;
	[Export]
	public float MaxFactor = 10.0f;

	[Export]
	public float HalfFade = 75.0f;
	[Export]
	public float HalfFactor = 0.33f;

	private bool _canInteract = false;
	public bool CanInteract {
		get => _canInteract;
		set {
			if (InteractIndicator != null) {
				InteractIndicator.Visible = value;
			}
			_canInteract = value;
		}
	}

	[Export]
	public Node2D InteractIndicator;

	private bool _isInCinematic = false;
	public bool IsInCinematic {
		get => _isInCinematic;
		set {
			_isInCinematic = value;
		}
	}

	private List<InputDirection> inputStack = new();


	private float fadeSmoothness = 1.0f;
	private float fadeDistance = 25.0f;
	private float darkDistanceFactor = 25.0f;
	private float targetFade = 25.0f;
	private float targetDarkDistanceFactor = 25.0f;

	public bool isTransitioning;

	public bool IsInFight = false;

	public bool Frozen = false;

	[Export]
	public int MaxHp = 20;

	[Export]
	public int lives = 3;


	private AnimationPlayer animationPlayer;
	private bool isInVictoryPose = false;

	public override void _Ready() {
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

		fadeDistance = MaxFade;
		darkDistanceFactor = MaxFactor;
		targetFade = fadeDistance;
		targetDarkDistanceFactor = darkDistanceFactor;

		fadeSmoothness = FadeSmoothIn;
		isTransitioning = false;
	}

	public override void _EnterTree() {
	}

	private void SwapParent(Node newParent) {
		GetParent().RemoveChild(this);
		newParent.AddChild(this);
	}

	public override void _Process(double delta) {
		var dialogue = GetNode<DialogueBox>("/root/DialogueBox/DialogueBox");
		if (InteractIndicator != null && (IsInCinematic || IsInFight || dialogue.IsInProgress)) {
			InteractIndicator.Visible = false;
		}

		if (Blackout != null) {
			fadeDistance = Mathf.Lerp(fadeDistance, targetFade, fadeSmoothness);
			darkDistanceFactor = Mathf.Lerp(darkDistanceFactor, targetDarkDistanceFactor, fadeSmoothness);
			Blackout.Visible = true;

			(Blackout.Material as ShaderMaterial)?.SetShaderParameter("bright_dist", fadeDistance);
			(Blackout.Material as ShaderMaterial)?.SetShaderParameter("darkness_dist_factor", darkDistanceFactor);
		}

		if (isInVictoryPose) {
			return;
		}

		Vector2I inputDirection = inputStack.Count == 0 ? Vector2I.Zero : InputDirectionAsVector(inputStack[0]);

		CheckInput("move_left", InputDirection.Left);
		CheckInput("move_right", InputDirection.Right);
		CheckInput("move_up", InputDirection.Up);
		CheckInput("move_down", InputDirection.Down);

		if (isTransitioning || IsInCinematic) {
			Velocity = Vector2.Zero;
			Sprite.Play("idle");
			return;
		}

		Velocity = MoveSpeed * new Vector2(inputDirection.X, inputDirection.Y);

		if (Mathf.Abs(Velocity.X) > 0.001f && Sprite != null) {
			Sprite.FlipH = Velocity.X < 0.0f;
		}

		if (Mathf.Abs(Velocity.LengthSquared()) > 0.001f) {
			if (Mathf.Abs(Velocity.X) > Mathf.Abs(Velocity.Y)) {
				Sprite.Play("walk_side");
			} else if (Velocity.Y < 0.0f) {
				Sprite.Play("walk_up");
			} else {
				Sprite.Play("walk_down");
			}
		} else {
			Sprite.Play("idle");
		}

		if (!Frozen) {
			MoveAndSlide();
		}
	}

	public void FadeInIntro() {
		targetFade = 0.0f;
		targetDarkDistanceFactor = 0.0f;
		fadeSmoothness = FadeSmoothIn;

		var gameManager = GetNode<GameManager>("/root/GameManager");
		gameManager.StopAllMusic();
		gameManager.EnterCombatMusic.Play();

		isTransitioning = true;

		GetTree().CreateTimer(FadeSmoothInTime).Timeout += () => {
			targetFade = HalfFade;
			targetDarkDistanceFactor = HalfFactor;
			fadeSmoothness = FadeSmoothIn;

			GetTree().CreateTimer(FadeSmoothInTime / 2.0f).Timeout += () => {
				isTransitioning = false;
			};
		};
	}

	public void StartFadingToBlack() {
		targetFade = 0.0f;
		targetDarkDistanceFactor = 0.0f;
		fadeSmoothness = FadeSmoothIn;
	}

	public void FullyFadeOut() {
		targetFade = MaxFade;
		targetDarkDistanceFactor = MaxFactor;
		fadeSmoothness = FadeSmoothOut;
	}

	// Pass null target to perform only the fade-in-out without any music or level transitions
	public void StartEncounter(EncounterTrigger target, GodTrigger god = null) {
		if (IsInFight) {
			return;
		}

		Sprite?.Play("blink");
		targetFade = 0.0f;
		targetDarkDistanceFactor = 0.0f;
		fadeSmoothness = FadeSmoothIn;
		inputStack.Clear();
		Velocity = Vector2.Zero;

		var gameManager = GetNode<GameManager>("/root/GameManager");
		if (target != null) {
			gameManager.StopAllMusic();
			gameManager.EnterCombatMusic.Play();
			IsInFight = true;
		}

		isTransitioning = true; // trans rights are human rights

		GetTree().CreateTimer(FadeSmoothInTime).Timeout += () => {
			targetFade = MaxFade;
			targetDarkDistanceFactor = MaxFactor;
			fadeSmoothness = FadeSmoothOut;

			if (target != null) {
				gameManager.StopAllMusic();
				gameManager.CombatMusic.Play();

				var playerCards = gameManager.playerDeck;
				gameManager.CurrentlyFightingGod = god;
				gameManager.StartFight(playerCards.ToList(), target);
				Sprite.Visible = false;
				Frozen = true;
			}

			GetTree().CreateTimer(FadeSmoothInTime / 2.0f).Timeout += () => {
				isTransitioning = false;
			};
		};
	}

	public void DoAfterBattleStuff(bool winner) {
		Sprite.Visible = true;

		var gameManager = GetNode<GameManager>("/root/GameManager");
		gameManager.StopAllMusic();
		gameManager.ExplocationMusic.Play();

		isInVictoryPose = true;

		if (winner) {
			if (gameManager.CurrentlyFightingGod == null) {
				animationPlayer.Play("battle_victory");
			} else {
				Unfreeze();
				var god = gameManager.CurrentlyFightingGod;
				god.PreCombatDialogue.Visible = false;
				god.PostCombatDialogue.Visible = true;
				gameManager.godsBeaten.Add(god.PreCombatDialogue.Who);

				GD.Print($"Won god {god.PreCombatDialogue.Who}");
			}

			GetTree().CreateTimer(1.5f).Timeout += () => {
				if (gameManager.CurrentlyFightingGod != null) {
					
					gameManager.CurrentlyFightingGod = null;
					if (gameManager.godsBeaten.Count >= 4) {
						gameManager.WinGame();
						return;
					}
				} else {
					var dialogue = GetNode<DialogueBox>("/root/DialogueBox/DialogueBox");
					dialogue.Start(WhoIsSpeaking.Paju, new string[] { "You found a new scroll!" });
				}
			};
		} else {
			lives--;
			if (lives <= 0) {
				gameManager.StopAllMusic();
				animationPlayer.Play("die");
				return;
			}
			animationPlayer.Play("battle_lost");
			GetTree().CreateTimer(1.0f).Timeout += () => {
				var dialogue = GetNode<DialogueBox>("/root/DialogueBox/DialogueBox");
				var line = lives == 1
				? "Ouch, I can't take much more hits"
				: "Ouch, that hurt";
				dialogue.Start(WhoIsSpeaking.Paju, new string[] { line });
			};

		}
	}

	private void Unfreeze() {
		isInVictoryPose = false;
		IsInFight = false;
		Frozen = false;
		inputStack.Clear();
	}

	private void TeleportToShadowRealm() {
		StartFadingToBlack();
	}

	private void CheckInput(StringName action, InputDirection direction) {
		if (Input.IsActionJustPressed(action)) {
			inputStack = inputStack.Prepend(direction).ToList();
		}

		if (Input.IsActionJustReleased(action)) {
			inputStack.Remove(direction);
		}
	}

	private static Vector2I InputDirectionAsVector(InputDirection direction) {
		return direction switch {
			InputDirection.Left => Vector2I.Left,
			InputDirection.Right => Vector2I.Right,
			InputDirection.Up => Vector2I.Up,
			InputDirection.Down => Vector2I.Down,
			_ => throw new NotSupportedException("The above cases are exhaustive. There be dragons if this is ever reached."),
		};
	}

	private enum InputDirection {
		Left,
		Right,
		Up,
		Down,
	}
}
