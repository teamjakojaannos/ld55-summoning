using Godot;

public partial class GodTrigger : Node2D
{
	[Export]
	public DialogueTrigger PreCombatDialogue;

	[Export]
	public DialogueTrigger PostCombatDialogue;

	[Export]
	public EncounterTrigger DummyEncounterTrigger;

	public override void _Ready() {
		PreCombatDialogue.DialogueFinished += () => {
			var player = GetNode<GameManager>("/root/GameManager").Player;
			player.StartEncounter(DummyEncounterTrigger, this);
		};
	}
}
