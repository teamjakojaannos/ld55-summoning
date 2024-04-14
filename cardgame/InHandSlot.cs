using Godot;

public partial class InHandSlot : TextureRect {
    public Card Card;

	public void AddCardAsChild(Card card, float cardMoveSpeed) {
		var oldPosition = card.GlobalPosition;
		Card = card;
		card.GetParent()?.RemoveChild(card);
		AddChild(card);
		card.MoveToNewParent(oldPosition, cardMoveSpeed);
	}
}
