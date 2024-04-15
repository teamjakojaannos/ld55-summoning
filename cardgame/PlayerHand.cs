using Godot;
using System.Collections.Generic;

public partial class PlayerHand : Control {
	[Export]
	[ExportCategory("Prewire")]
	public Cardgame Cardgame;

	[Export]
	public PackedScene SlotTemplate;

	public List<Card> Cards = new();

	public void PlayCard(int index) {
		var playedSlot = GetChild(index);
		RemoveChild(playedSlot);
		playedSlot.QueueFree();

		var totalWidth = 0.0f;
		for (int i = 0; i < GetChildCount(); ++i) {
			totalWidth += GetChild<InHandSlot>(i).Size.X;
		}

		var offsetX = 0.0f;
		var startX = -totalWidth / 2.0f;
		for (int i = 0; i < GetChildCount(); ++i) {
			var slot = GetChild<InHandSlot>(i);
			slot.SetIndex(i);

			var targetPositionX = startX + offsetX;
			var targetPosition = new Vector2(targetPositionX, 0.0f);

			slot.Position = targetPosition;
			var cardWidth = slot.Size.X;
			offsetX += cardWidth;
		}
	}

	public void Reset() {
		while (GetChildCount() > 0) {
			var child = GetChild(0);
			child.QueueFree();
			RemoveChild(child);
		}

		Cards.Clear();
	}

	public void AddCards(List<Card> cards, float cardMoveSpeed) {
		Reset();
		bool playersCard = AmIPlayer();

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
			SetupSlot(slot, i);
			slot.Position = targetPosition;
			AddChild(slot);
			slot.AddCardAsChild(card, cardMoveSpeed);

			var cardWidth = card.Size.X;
			offsetX += cardWidth;

			Cards.Add(card);

			card.IsPlayersCard = playersCard;
			card.SetInfoVisible(playersCard);
			card.Visible = true;
		}
	}

	protected virtual void SetupSlot(InHandSlot slot, int index) {
		slot.Setup(Cardgame, index);
	}

	protected virtual bool AmIPlayer() {
		return true;
	}
}
