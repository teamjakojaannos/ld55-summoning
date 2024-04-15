using Godot;

public partial class InPlaySlot : TextureRect {
	public Card Card;

	public bool IsTaken {
		get => Card != null;
	}

	public bool IsFree {
		get => !IsTaken;
	}

	[Export]
	[ExportCategory("Prewire")]
	public AnimationPlayer Animation;

	private string attackAnimation = "attack_animation";
	private string attackAnimationUpward = "attack_animation_upward";

	[Signal]
	public delegate void AttackAnimationFinishedEventHandler();

	private AttackInfo attackInfo;

	public override void _Ready() {
		Animation.AnimationFinished += (name) => {
			if (name == attackAnimation || name == attackAnimationUpward) {
				EmitSignal(SignalName.AttackAnimationFinished);
			}
		};
	}

	public void AddCardAsChild(Card card, float cardMoveSpeed) {
		if (IsTaken) {
			GD.PrintErr($"Slot {Name} is already taken!");
			return;
		}

		var oldPosition = card.GlobalPosition;
		card.GetParent()?.RemoveChild(card);
		Card = card;
		GetNode("./CardRoot").AddChild(card);
		card.MoveToNewParent(oldPosition, cardMoveSpeed);
	}

	public void StartAttack(AttackInfo attack) {
		attackInfo = attack;

		var animationName = Card.IsPlayersCard ? attackAnimationUpward : attackAnimation;
		Animation.Play(animationName);
	}

	public void CardAttacks() {
		if (Card == null || attackInfo == null) {
			return;
		}

		attackInfo.fightManager.CardAttacksTarget(this, attackInfo.target, Card.IsPlayersCard);
		attackInfo = null;
	}

	public void PlayHurtAnimation() {
		Animation.Play("take_damage");
	}

	public void PlayDieAnimation() {
		Animation.Play("die");
	}

    public void Clear() {
        if (IsFree) {
			return;
		}

		Animation.Play("RESET");
		var cardRoot = GetNode("./CardRoot");
		cardRoot.RemoveChild(Card);
		Card = null;
    }
}
