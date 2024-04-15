using Godot;

public partial class InHandSlot : TextureRect {
	public Card Card;

	[Export]
	[ExportCategory("Prewire")]
	public Cardgame Cardgame;

	[Export]
	public Color HighlightedColor;

	[Export]
	public Color DimmedColor;

	[Export]
	public Label KeyLabel;

	private int index = -1;

	public void AddCardAsChild(Card card, float cardMoveSpeed) {
		var oldPosition = card.GlobalPosition;
		Card = card;
		card.GetParent()?.RemoveChild(card);
		AddChild(card);
		card.MoveToNewParent(oldPosition, cardMoveSpeed);
	}

	public void SetIndex(int index) {
		KeyLabel.Text = $"{index + 1}";
	}

	public void Setup(Cardgame cardgame, int index, bool isPlayerSlot = true) {
		SetIndex(index);
		Cardgame = cardgame;
		Cardgame.PlayerSelectedCardInHand += DimOnOtherCardSelected;
		Cardgame.PlayerDeselectedCardInHand += HighlightOnOtherCardDeselect;
		Cardgame.PlayerNoMoreMoves += DimOnPlayerNoMoreMoves;

		if (!isPlayerSlot) {
			KeyLabel.GetParent<CanvasItem>().Visible = false;
		}
	}

	private void DimOnOtherCardSelected(int cardIndex) {
		if (index == cardIndex) {
			return;
		}

		KeyLabel.GetParent<TextureRect>().Modulate = DimmedColor;
	}

	private void HighlightOnOtherCardDeselect(int cardIndex) {
		if (index == cardIndex) {
			return;
		}

		KeyLabel.GetParent<TextureRect>().Modulate = HighlightedColor;
	}

	private void DimOnPlayerNoMoreMoves() {
		KeyLabel.GetParent<TextureRect>().Modulate = DimmedColor;
	}

	public override void _ExitTree() {
		if (Cardgame != null) {
			Cardgame.PlayerSelectedCardInHand -= DimOnOtherCardSelected;
			Cardgame.PlayerDeselectedCardInHand -= HighlightOnOtherCardDeselect;
			Cardgame.PlayerNoMoreMoves -= DimOnPlayerNoMoreMoves;
		}
	}
}
