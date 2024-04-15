using System.Linq;
using Godot;
using Godot.Collections;

public partial class ScrollMenu : Control
{
	[Export]
	public GridContainer Grid;

	public void SetScrolls(Array<CardStats> playerDeck) {
		for (var i = 0; i < Grid.GetChildCount(); ++i) {
			Grid.GetChild(i).QueueFree();
		}

		var cardGame = GetNode<Cardgame>("/root/CardGameLayer/Cardgame");
		var deck = cardGame.CreateDeck(playerDeck.ToList(), skipTreeModification: true);
		foreach (var card in deck.cards) {
			var cardWrapper = new Control {
				CustomMinimumSize = new(96, 128)
			};
			cardWrapper.AddChild(card);
			card.HighlightZoomFactor = 1.1f;
			card.Visible = true;

			Grid.AddChild(cardWrapper);
		}
	}
}
