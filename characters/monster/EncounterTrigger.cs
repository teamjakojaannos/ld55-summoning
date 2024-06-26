using System.Linq;
using Godot;

public partial class EncounterTrigger : Area2D {

	[Export]
	public int MaxHp = 20;

	[Export]
	public Godot.Collections.Array<CardStats> cards = new();

	[Signal]
	public delegate void DeleteThisNodeEventHandler();

	public MonsterDrops drops;

	public override void _Ready() {
		AreaEntered += (other) => {
			if (other is not InteractArea interactArea) {
				return;
			}

			var player = interactArea.Player;
			var asList = cards.ToList();
			player.StartEncounter(this);
		};
	}

	public void EmitDeleteSignal() {
		EmitSignal(SignalName.DeleteThisNode);
	}
}
