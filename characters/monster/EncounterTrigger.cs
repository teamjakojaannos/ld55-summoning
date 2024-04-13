using Godot;
using System;

public partial class EncounterTrigger : Area2D {
    public override void _Ready() {
		BodyEntered += (other) => {
            if (other is not Player player) {
                return;
            }

            player.StartEncounter(this);
		};
    }
}
