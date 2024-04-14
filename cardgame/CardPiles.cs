using System.Collections.Generic;
using Godot;

public partial class CardPiles : Control
{
    private Label labelDrawPile;
    private Label labelDiscardPile;

    private Vector2 positionDrawPile;
    private Vector2 positionDiscardPile;

    private CardDeck drawPile = new();

    private readonly List<Card> discardPile = new();

    public override void _Ready()
    {
        labelDrawPile = GetNode<Label>("LabelDrawPile");
        labelDiscardPile = GetNode<Label>("LabelDiscardPile");

        positionDrawPile = GetNode<Marker2D>("DrawPilePosition").Position;
        positionDiscardPile = GetNode<Marker2D>("DiscardPilePosition").Position;

        UpdateLabels();
    }

    public void UpdateLabels()
    {
        labelDrawPile.Text = $"Draw pile: {drawPile.cards.Count}";
        labelDiscardPile.Text = $"Discard pile: {discardPile.Count}";
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

    public void DiscardCards(List<Card> cards, float animationTime)
    {
        foreach (var card in cards)
        {
            discardPile.Add(card);
            // TODO: play animation on card
            card.StartMovingTo(DiscardPilePosition(), animationTime);
        }

        cards.Clear();
        UpdateLabels();
    }

    public void EndDiscardAnimation()
    {
        foreach (var card in discardPile)
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
        foreach (var card in discardPile)
        {
            drawPile.cards.Add(card);
        }

        discardPile.Clear();

        drawPile.Shuffle();

        UpdateLabels();
    }

    public Vector2 DrawPilePosition()
    {
        return Position + positionDrawPile;
    }

    public Vector2 DiscardPilePosition()
    {
        return Position + positionDiscardPile;
    }

    public void PlayRecycleAnimation()
    {
        // TODO:
    }
}
