using System.Collections.Generic;
using Godot;

public partial class CardPiles : Control
{
    private Label labelDrawPile;
    private Label labelDiscardPile;

    private CardDeck drawPile = new();

    private readonly List<Card> discardPile = new();

    public override void _Ready()
    {
        labelDrawPile = GetNode<Label>("LabelDrawPile");
        labelDiscardPile = GetNode<Label>("LabelDiscardPile");

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

    public void DiscardCards(List<Card> cards)
    {
        foreach (var card in cards)
        {
            discardPile.Add(card);

            card.Position = new(0, 0);
            card.Visible = false;
        }

        cards.Clear();
        UpdateLabels();
    }
}
