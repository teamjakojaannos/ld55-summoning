using System.Collections.Generic;
using Godot;

public class CardDeck
{
    public List<Card> cards;

    private readonly RandomNumberGenerator rng;

    public CardDeck()
    {
        rng = new RandomNumberGenerator();
        cards = new();
    }

    public CardDeck(List<Card> cards)
    {
        rng = new RandomNumberGenerator();
        this.cards = cards;
    }

    public void Shuffle()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            var first = rng.RandiRange(0, cards.Count - 1);
            var second = rng.RandiRange(0, cards.Count - 1);

            (cards[second], cards[first]) = (cards[first], cards[second]);
        }
    }

    public List<Card> DrawFromTopOfDeck(int n)
    {
        List<Card> result = new();
        for (int i = 0; i < n; i++)
        {
            if (cards.Count == 0)
            {
                return result;
            }

            var card = cards[0];
            result.Add(card);
            cards.RemoveAt(0);
        }

        return result;
    }
}
