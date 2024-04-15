using Godot;

public partial class EnemyHand : PlayerHand {
	protected override void SetupSlot(InHandSlot slot, int index) {
		slot.Setup(Cardgame, index, isPlayerSlot: false);
	}
}
