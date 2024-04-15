using Godot;

public partial class WizardCellarIntro : Node2D {
	[Export]
	public AnimationPlayer Animation;

	private Player Player {
		get => GetNode<GameManager>("/root/GameManager").Player;
	}

	[Export]
	public AnimatedSprite2D Ritual;
	[Export]
	public AnimatedSprite2D Pentagrammi;

	[Export]
	public AudioStreamPlayer TransformationLoop;

	[Export]
	public AudioStreamPlayer VictoryRoyale;

	[Export]
	public AudioStreamPlayer DedTheme;

	[Export]
	public AnimatedSprite2D Ded;


	public override void _Ready() {
		var gameManager = GetNode<GameManager>("/root/GameManager");
		if (gameManager.IntroSeen) {
			Ritual.Visible = false;
			Pentagrammi.Visible = true;
			gameManager.Player.ExplocationMusic.Stop();
			DedTheme.Play();
			Ded.Play("ded_idle");
			Pentagrammi.Play("idle");
			return;
		}

		Animation.Play("kitty_idle");

		Player.IsInCinematic = true;
		Player.Sprite.Visible = false;
		Pentagrammi.Visible = false;

		Player.ExplocationMusic.Play();
		Player.CombatMusic.Stop();
		Player.EnterCombatMusic.Stop();

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

		Dialogue.Start(WhoIsSpeaking.Kitty, new string[] {
			"Meow",
			"Mjäy :3"
		});
		Dialogue.DialogueFinished += KittyDialogueFinished;
	}

	private void KittyDialogueFinished() {
		Dialogue.DialogueFinished -= KittyDialogueFinished;
		Dialogue.DialogueFinished += FirstDialogueFinished;
		Dialogue.Start(WhoIsSpeaking.OldWiz, new string[] {
			"This is my last solution.",
			"Maybe with you, my furry friend...",
			"I dont need to use my precious time with all this nonsense.",
			"All these years Ive dealt with you, fed you ...",
			"and and...",
			"well thats already kind of a lot!",
			"...And now you gonna be useful for once!"
		});
	}

	[Export]
	public float DramaticPauseDuration = 1.5f;

	private void FirstDialogueFinished() {
		Dialogue.DialogueFinished -= FirstDialogueFinished;

		Player.FadeInIntro();

		GetTree().CreateTimer(3.0f).Timeout += () => {
			Pentagrammi.Visible = true;
			Pentagrammi.Play("idle");

			GetTree().CreateTimer(3.0f).Timeout += () => {
				StartSecondDialogue();
			};
		};
	}

	private void StartSecondDialogue() {
		Dialogue.DialogueFinished += SecondDialogueSecondPartFinished;

		Dialogue.Start(WhoIsSpeaking.OldWiz, new string[] {
			"Oh mighty catgod, Nyazathoth",
			"Whoops, no, not that god... Erm, I mean...",
			"Oh mighty catgod, sacred Katti Matikainen",
			"Im calling your name, favor my request!",
			"Forgive my pride and shame",
			"Trough my hand and my might, shall your enchantment flow..."
		});
	}

	private void SecondDialogueSecondPartFinished() {
		Dialogue.DialogueFinished -= SecondDialogueSecondPartFinished;

		GetTree().CreateTimer(DramaticPauseDuration).Timeout += () => {
			Dialogue.DialogueFinished += SecondDialogueThirdPartFinished;
			Dialogue.Start(WhoIsSpeaking.OldWiz, new string[] {
				"Häeeeaah... Ja..Jatsatsaa barillas dilla lapadeian dullan deian doo, Siel oli lystiä soiton jäläkeen",
				"sain minä kerran tytsysen Nähäkö ko sain ma vaimoseni ni mirripä kissaksi palajastui Katti se ei",
				"tytöstä kelepaa Tahdon mä iha oikeeta akkaa Salivili tipput tupput täppyt äppyt tipput hilijalleen..."
			});
		};
	}

	private void SecondDialogueThirdPartFinished() {
		Dialogue.DialogueFinished -= SecondDialogueThirdPartFinished;

		GetTree().CreateTimer(DramaticPauseDuration).Timeout += () => {
			Dialogue.DialogueFinished += SecondDialogueFinished;
			Dialogue.Start(WhoIsSpeaking.Kitty, new string[] {
				"MJÄY!?"
			});
		};
	}

	private void SecondDialogueFinished() {
		Dialogue.DialogueFinished -= SecondDialogueFinished;

		Player.ExplocationMusic.Stop();
		Player.CombatMusic.Stop();
		Player.EnterCombatMusic.Stop();

		GetTree().CreateTimer(1.5f).Timeout += () => {
			Animation.Play("transformation");
			TransformationLoop.Play();
			Animation.AnimationFinished += TransformationFinished;
		};
	}

	private void TransformationFinished(StringName animation) {
		Animation.AnimationFinished -= TransformationFinished;
		Player.Sprite.Visible = true;
		Ritual.Visible = false;

		Player.ExplocationMusic.Stop();
		Player.CombatMusic.Stop();
		Player.EnterCombatMusic.Stop();
		TransformationLoop.Stop();
		DedTheme.Play();

		Dialogue.DialogueFinished += PajuFinishedSpeakingAfterTransformationFinished;
		Dialogue.Start(WhoIsSpeaking.Paju, new string[] {
			"Aha! Much better! =^w^="
		});
	}

	private void PajuFinishedSpeakingAfterTransformationFinished() {
		Dialogue.DialogueFinished -= PajuFinishedSpeakingAfterTransformationFinished;

		Dialogue.DialogueFinished += OldWizFinishedTalkingAfterTransformation;
		Dialogue.Start(WhoIsSpeaking.OldWiz, new string[] {
			"Since you couldnt be the cat and chase the mice",
			"its your time to be the wizard and chase the sons of devils outside!",
			"Nerves.. The summoons keep eating my precious mushrooms..",
			"how can an educated intellectual prime man like me enjoy life... without mushrooms?",
			"Now, go off and banish those devils from my forests!",
			"Here, take this scroll with you.",
			"Hope you learned something while walking on wet ink...",
			"and spilling my whisk-.. ink..",
			"...",
			"It was expensive bottle.",
			"Uhg Im way too old for this crap!",
			"...",
			"aah- I dont feel so well.. ugh..."
		});
	}

	private void OldWizFinishedTalkingAfterTransformation() {
		Dialogue.DialogueFinished -= OldWizFinishedTalkingAfterTransformation;

		Ded.Play("ded");
		Ded.AnimationFinished += OldWizDedAfterTalking;
	}

	private void OldWizDedAfterTalking() {
		Ded.AnimationFinished -= OldWizDedAfterTalking;

		Dialogue.Start(WhoIsSpeaking.Paju, new string[] {
			"Oh no, my servant!",
			"Oh well...",
			"I will find a new one. =^w^'="
		});
		Dialogue.DialogueFinished += IntroFinished;
	}

	private void IntroFinished() {
		Dialogue.DialogueFinished -= IntroFinished;

		GetTree().CreateTimer(0.5f).Timeout += () => {
			Player.IsInCinematic = false;
		};
	}

	public void RemoveBlackout() {
		TransformationLoop.Stop();
		VictoryRoyale.Play();
		Player.FullyFadeOut();
	}
}
