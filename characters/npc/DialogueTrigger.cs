using Godot;
using System.Linq;

public partial class DialogueTrigger : Area2D {
	[Export]
	public Godot.Collections.Array<string> Lines = new();

	[Export]
	public WhoIsSpeaking Who = WhoIsSpeaking.OldWiz;

	[Export]
	public bool RequiresInteract = false;

	private bool playerInRange = false;
	private bool playerHasLeft = false;

	public override void _Ready() {
		AreaEntered += (other) => {
			if (other is not InteractArea interactArea) {
				return;
			}

			playerInRange = true;
			playerHasLeft = true;
			var player = interactArea.Player;

			if (RequiresInteract) {
				player.CanInteract = true;
				return;
			}


			var dialogue = GetNode<DialogueBox>("/root/DialogueBox/DialogueBox");
			dialogue.Start(Who, Lines.ToArray());

			player.IsInCinematic = true;

			dialogue.DialogueFinished += DialogueFinished;
		};

		AreaExited += (other) => {
			if (other is not InteractArea interactArea) {
				return;
			}

			playerInRange = false;
			playerHasLeft = true;
			var player = interactArea.Player;

			if (RequiresInteract) {
				player.CanInteract = false;
				return;
			}
		};
	}

	public override void _Process(double delta) {
		if (!playerHasLeft) {
			return;
		}

		var dialogue = GetNode<DialogueBox>("/root/DialogueBox/DialogueBox");
		var player = GetNode<GameManager>("/root/GameManager").Player;
		if (!playerInRange || player.IsInCinematic || player.IsInFight || dialogue.IsInProgress) {
			return;
		}

		if (RequiresInteract && Input.IsActionJustPressed("Interact")) {
			dialogue.Start(Who, Lines.ToArray());

			player.IsInCinematic = true;
			playerHasLeft = false;

			dialogue.DialogueFinished += DialogueFinished;
		}
	}

	private void DialogueFinished() {
		var dialogue = GetNode<DialogueBox>("/root/DialogueBox/DialogueBox");
		dialogue.DialogueFinished -= DialogueFinished;

		var player = GetNode<GameManager>("/root/GameManager").Player;
		player.IsInCinematic = false;
	}
}
