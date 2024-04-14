using Godot;
using System.Collections.Generic;

public partial class PlayerHand : Control {
	[Export]
	[ExportCategory("Prewire")]
    public Cardgame Cardgame;

	public List<Card> Cards = new();

	public void TakeCards(List<Card> cards, float movementTime) {
		// TODO: create "card in hand" slots instead of storing the actual cards
		// -> allows separating the key labels etc.

		var totalWidth = 0.0f;
		foreach (var card in cards) {
			totalWidth += card.Size.X;
		}

		var offsetX = 0.0f;
		var startX = Position.X - totalWidth / 2.0f;
		for (int i = 0; i < cards.Count; i++) {
            var card = cards[i];
            card.MoveInstantlyTo(Cardgame.DrawPilePosition);

			var cardWidth = card.Size.X;
            var targetPosition = startX + offsetX;
			offsetX += cardWidth;

            card.StartMovingTo(new(targetPosition, Position.Y), movementTime);

            Cards.Add(card);

            card.SetNumberLabelVisible(true);
            card.IsPlayersCard = true;

            card.Visible = true;
        }
	}
}
