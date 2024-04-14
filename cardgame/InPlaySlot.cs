using Godot;

public partial class InPlaySlot : TextureRect {
	public Card Card;

	public bool IsTaken {
		get => Card != null;
	}

	public bool IsFree {
		get => !IsTaken;
	}

	public void AddCardAsChild(Card card, float cardMoveSpeed) {
		if (IsTaken) {
			GD.PrintErr($"Slot {Name} is already taken!");
			return;
		}

		var oldPosition = card.GlobalPosition;
		card.GetParent()?.RemoveChild(card);
		Card = card;
		AddChild(card);
		card.MoveToNewParent(oldPosition, cardMoveSpeed);
	}

    public void Clear() {
        if (IsFree) {
			return;
		}

		Card = null;
    }
}
