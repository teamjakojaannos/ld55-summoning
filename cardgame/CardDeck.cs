using System.Collections.Generic;
using Godot;

public class CardDeck
{
    public List<Card> cards;

    private RandomNumberGenerator rng;

    public CardDeck(List<Card> cards)
    {
        this.rng = new RandomNumberGenerator();
        this.cards = cards;
    }

    public List<Card> Draw(int n)
    {
        List<Card> result = new();
        for (int i = 0; i < n; i++)
        {
            if (cards.Count == 0)
            {
                return result;
            }

            var index = rng.RandiRange(0, cards.Count - 1);
            var card = cards[index];
            result.Add(card);
            cards.RemoveAt(index);
        }

        return result;
    }
}
