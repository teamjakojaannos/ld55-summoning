using System.Collections.Generic;
using Godot;

public class CardDecks
{
    public static CardDeck PlayerDeck(PackedScene cardScene)
    {
        var stats = new List<(int hp, int damage)>();
        for (int i = 0; i < 4; i++)
        {
            stats.Add((hp: 10, damage: 3));
        }
        for (int i = 0; i < 2; i++)
        {
            stats.Add((hp: 2, damage: 5));
        }
        stats.Add((hp: 20, damage: 10));

        var cards = new List<Card>();
        foreach (var (hp, dmg) in stats)
        {
            cards.Add(CreateCard(cardScene, hp, dmg));
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
