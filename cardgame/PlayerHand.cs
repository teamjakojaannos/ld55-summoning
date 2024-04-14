using Godot;
using System.Collections.Generic;

public partial class PlayerHand : Control {
	[Export]
	[ExportCategory("Prewire")]
    public Cardgame Cardgame;

	[Export]
	public PackedScene SlotTemplate;

	public List<Card> Cards = new();

	public void TakeCards(List<Card> cards, float cardMoveSpeed) {
		while (GetChildCount() > 0) {
			var child = GetChild(0);
			child.QueueFree();
			RemoveChild(child);
		}

		var totalWidth = 0.0f;
		foreach (var card in cards) {
			totalWidth += card.Size.X;
		}

		var offsetX = 0.0f;
		var startX = -totalWidth / 2.0f;
		for (int i = 0; i < cards.Count; i++) {
            var card = cards[i];

            var targetPositionX = startX + offsetX;
			var targetPosition = new Vector2(targetPositionX, 0.0f);

			var slot = SlotTemplate.Instantiate<InHandSlot>();
			slot.Position = targetPosition;
			AddChild(slot);
			slot.AddCardAsChild(card, cardMoveSpeed);

			var cardWidth = card.Size.X;
			offsetX += cardWidth;

            Cards.Add(card);

            card.SetNumberLabelVisible(true);
            card.IsPlayersCard = true;

            card.Visible = true;
        }
	}
}
