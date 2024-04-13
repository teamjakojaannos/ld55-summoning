using Godot;
using System.Linq;

public partial class DialogueTrigger : Area2D {
	[Export]
	public Godot.Collections.Array<string> Lines = new();

	[Export]
	public WhoIsSpeaking Who = WhoIsSpeaking.OldWiz;

    public override void _Ready() {
        BodyEntered += (other) => {
            if (other is not Player player) {
                return;
            }

            var dialogue = GetNode<DialogueBox>("/root/DialogueBox/DialogueBox");
			dialogue.Start(Who, Lines.ToArray());

			player.IsInCinematic = true;

			dialogue.DialogueFinished += DialogueFinished;
        };
    }

	private void DialogueFinished() {
		var dialogue = GetNode<DialogueBox>("/root/DialogueBox/DialogueBox");
		dialogue.DialogueFinished += DialogueFinished;

		var player = GetNode<Player>("/root/Player");
		player.IsInCinematic = false;
	}
}
