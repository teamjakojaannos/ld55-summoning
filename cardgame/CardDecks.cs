using System.Collections.Generic;
using Godot;

public class CardDecks
{
    public static CardDeck PlayerDeck(PackedScene cardScene)
    {
        var cards = new List<Card>();
        for (int i = 0; i < 8; i++)
        {
            cards.Add(CreateCard(cardScene, i + 1, i + 1));
        }

        return new CardDeck(cards);
    }

    public static CardDeck EnemyDeck(PackedScene cardScene)
    {
        var cards = new List<Card>()
        {
            CreateCard(cardScene, 12, 7),
            CreateCard(cardScene, 34, 8),
            CreateCard(cardScene, 56, 9),
            CreateCard(cardScene, 78, 1),
            CreateCard(cardScene, 99, 2),
            CreateCard(cardScene, 87, 3),
            CreateCard(cardScene, 65, 2),
            CreateCard(cardScene, 43, 1),
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
