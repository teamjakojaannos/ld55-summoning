using Godot;

public partial class EncounterTrigger : Area2D {
    public override void _Ready() {
        AreaEntered += (other) => {
            if (other is not InteractArea interactArea) {
                return;
            }

            var player = interactArea.Player;
            player.StartEncounter(this);
        };
    }
}
