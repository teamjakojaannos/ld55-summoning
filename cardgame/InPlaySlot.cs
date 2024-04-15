using Godot;

public partial class InPlaySlot : TextureRect {
	[Export]
	public bool IsPlayerSlot;

	public Card Card;

	public bool IsTaken {
		get => Card != null;
	}

	public bool IsFree {
		get => !IsTaken;
	}


	[Export]
	public ArenaPosition PositionOnArena;

	[Export]
	[ExportCategory("Prewire")]
	public AnimationPlayer Animation;

	[Export]
	public Cardgame Cardgame;

	[Export]
	public Control ArenaPosKey;

	[Export]
	public Color HighlightedColor;

	[Export]
	public Color DimmedColor;

	private string attackAnimation = "attack_animation";
	private string attackAnimationUpward = "attack_animation_upward";

	[Signal]
	public delegate void AttackAnimationFinishedEventHandler();

	private AttackInfo attackInfo;

	public override void _Ready() {
		ArenaPosKey.Visible = IsPlayerSlot;

		Animation.AnimationFinished += (name) => {
			if (name == attackAnimation || name == attackAnimationUpward) {
				EmitSignal(SignalName.AttackAnimationFinished);
			}
		};

		ArenaPosKey.Modulate = DimmedColor;

        Cardgame.PlayerSelectedCardInHand += (idx) => {
			var positionTaken = Cardgame.IsPositionOnTableTaken(PositionOnArena);
            var useHighlight = !positionTaken;
            var color = useHighlight ? HighlightedColor : DimmedColor;

            ArenaPosKey.Modulate = color;
        };

		Cardgame.PlayerDeselectedCardInHand += (idx) => {
			ArenaPosKey.Modulate = DimmedColor;
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

		var animationName = IsPlayerSlot ? attackAnimationUpward : attackAnimation;
		Animation.Play(animationName);
	}

	public void CardAttacks() {
		if (Card == null || attackInfo == null) {
			return;
		}

		attackInfo.fightManager.CardAttacksTarget(this, attackInfo.target, IsPlayerSlot);
		Cardgame.CardHit.Play();
		attackInfo = null;
	}

	public void PlayHurtAnimation() {
		Animation.Play("take_damage");
	}

	public void PlayDieAnimation() {
		Animation.Play("die");
		Cardgame.CardDie.Play();
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
