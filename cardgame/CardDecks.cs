using System.Collections.Generic;
using Godot;

public class CardDecks
{
    public static CardDeck PlayerDeck(PackedScene cardScene)
    {
        var cards = new List<Card>();
        for (int i = 0; i < 4; i++)
        {
            cards.Add(CreateCard(cardScene, 10, 3));
        }
        for (int i = 0; i < 2; i++)
        {
            cards.Add(CreateCard(cardScene, 2, 5));
        }
        cards.Add(CreateCard(cardScene, 200, 500));

        return new CardDeck(cards);
    }

    public static CardDeck EnemyDeck(PackedScene cardScene)
    {
        var cards = new List<Card>()
        {
            CreateCard(cardScene, 12, 7),
            CreateCard(cardScene, 34, 8),
            CreateCard(cardScene, 56, 9),
        };
        return new CardDeck(cards);
    }

    private static Card CreateCard(PackedScene cardScene, int hp, int damage)
    {
        var card = cardScene.Instantiate<Card>();
        card.MaxHp = hp;
        card.CurrentHp = hp;
        card.Damage = damage;

        return card;
    }
}
