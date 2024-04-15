using System;

public partial class EnemyHand : PlayerHand {
	protected override bool AmIPlayer() {
		return false;
	}
}
