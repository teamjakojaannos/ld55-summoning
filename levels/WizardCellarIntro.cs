using Godot;
using System;

public partial class WizardCellarIntro : Node2D {
    [Export]
    public AnimationPlayer Animation;

    private Player Player {
		get => GetNode<Player>("/root/Player");
	}

    [Export]
    public AnimatedSprite2D Ritual;
	[Export]
    public AnimatedSprite2D Pentagrammi;


    public override void _Ready() {
        Animation.Play("kitty_idle");
		
        Player.IsInCinematic = true;
        Player.Sprite.Visible = false;
		Pentagrammi.Visible = false;

		GetTree().CreateTimer(2.0f).Timeout += () => {
			StartIntro();
		};
    }

	private DialogueBox Dialogue {
		get => GetNode<DialogueBox>("/root/DialogueBox/DialogueBox");
	}

    public void StartIntro() {
		Player.IsInCinematic = true;
        Player.Sprite.Visible = false;
		Pentagrammi.Visible = false;

		Animation.Play("kitty_idle");

		Dialogue.Start(WhoIsSpeaking.OldWiz, new string[] {
			"foo foo foo foo foo foo foo foo foo foo foo foo foo foo foo foo foo foo",
			"bar",
			"baz"
		});
		Dialogue.DialogueFinished += FirstDialogueFinished;
	}

	private void FirstDialogueFinished() {
		Dialogue.DialogueFinished -= FirstDialogueFinished;

		Player.StartEncounter(null); // HACK: null encounter skips level transition

		GetTree().CreateTimer(3.0f).Timeout += () => {
			Pentagrammi.Visible = true;
			Pentagrammi.Play("idle");

			GetTree().CreateTimer(1.5f).Timeout += () => {
				StartSecondDialogue();
			};
		};
	}

	private void StartSecondDialogue() {
		Dialogue.DialogueFinished += SecondDialogueFinished;

		Dialogue.Start(WhoIsSpeaking.OldWiz, new string[] {
			"first",
			"second",
			"fourth",
			"[object Object]"
		});
	}

	private void SecondDialogueFinished() {
		Dialogue.DialogueFinished -= SecondDialogueFinished;

		GetTree().CreateTimer(1.5f).Timeout += () => {
			Animation.Play("transformation");
			Animation.AnimationFinished += TransformationFinished;
		};
	}

	private void TransformationFinished(StringName animation) {
		Animation.AnimationFinished -= TransformationFinished;
		Player.Sprite.Visible = true;
		Ritual.Visible = false;

		Dialogue.DialogueFinished += IntroFinished;
		Dialogue.Start(WhoIsSpeaking.Paju, new string[] {
			"Aha! Much better! =^w^="
		});
	}

	private void IntroFinished() {
		Dialogue.DialogueFinished -= IntroFinished;
		Player.IsInCinematic = false;
	}
}
