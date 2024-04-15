using Godot;
using System.Collections.Generic;

public partial class DiscardPile : TextureRect {
	public List<Card> Cards = new();

	public void AddCardAsChild(Card card, float cardMoveSpeed) {
		var oldPosition = card.GlobalPosition;
		Cards.Add(card);
		card.GetParent()?.RemoveChild(card);
		AddChild(card);
		card.MoveToNewParent(oldPosition, cardMoveSpeed);
	}

	public void Clear() {
		foreach (var card in Cards) {
			card.QueueFree();
		}

		Cards.Clear();
	}
}
