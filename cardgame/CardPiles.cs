using System.Collections.Generic;
using Godot;

public partial class CardPiles : Control
{
    private Label labelDrawPile;
    private Label labelDiscardPile;

    private Control drawPilePosition;

    private CardDeck drawPile = new();

    [Export]
    public DiscardPile discardPile;

    public override void _Ready()
    {
        labelDrawPile = GetNode<Label>("DrawPile/Label");
        labelDiscardPile = GetNode<Label>("DiscardPile/Label");

        drawPilePosition = GetNode<Control>("DrawPile");

        UpdateLabels();
    }

    public void UpdateLabels()
    {
        labelDrawPile.Text = $"Draw pile: {drawPile.cards.Count}";
        labelDiscardPile.Text = $"Discard pile: {discardPile.Cards.Count}";
    }

    public void SetDeck(CardDeck deck)
    {
        drawPile = deck;

        UpdateLabels();
    }

    public List<Card> DrawCards(int amount)
    {
        var result = drawPile.DrawFromTopOfDeck(amount);
        UpdateLabels();
        return result;
    }

    public void DiscardCards(List<Card> cards, float lerpSpeed)
    {
        foreach (var card in cards) {
            discardPile.AddCardAsChild(card, lerpSpeed);
        }

        cards.Clear();
        UpdateLabels();
    }

    public void EndDiscardAnimation()
    {
        foreach (var card in discardPile.Cards)
        {
            card.StopMovement();
            card.Visible = false;
        }
    }

    public void AddCardsAsChildren(Node parentNode)
    {
        foreach (var card in drawPile.cards)
        {
            parentNode.AddChild(card);
            card.Visible = false;
        }
    }

    public bool DrawPileEmpty()
    {
        return drawPile.cards.Count == 0;
    }

    public void RecycleDiscardPile()
    {
        // FIXME: reparent to draw pile
        drawPile.cards.AddRange(discardPile.Cards);
        discardPile.Cards.Clear();

        drawPile.Shuffle();

        UpdateLabels();
    }

    public Vector2 DrawPilePosition()
    {
        return drawPilePosition.GlobalPosition;
    }

    public Vector2 DiscardPilePosition()
    {
        return discardPile.GlobalPosition;
    }

    public void PlayRecycleAnimation()
    {
        // TODO:
    }
}
