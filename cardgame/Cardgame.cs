using System.Collections.Generic;
using Godot;

public partial class Cardgame : Control
{
    enum ArenaPosition
    {
        TopLeft,
        TopMid,
        TopRight,
        BotLeft,
        BotMid,
        BotRight,
    }

    private readonly Dictionary<ArenaPosition, Vector2> arenaPositions = new();

    private PackedScene cardScene = GD.Load<PackedScene>("res://cardgame/card.tscn");
    private PackedScene cardInHandScene = GD.Load<PackedScene>("res://cardgame/card_in_hand.tscn");

    private HBoxContainer playerHand;
    private HBoxContainer enemyHand;

    private Control arena;

    private readonly List<CardInHand> playerCards = new();

    public override void _Ready()
    {
        playerHand = GetNode<HBoxContainer>("Hand");
        enemyHand = GetNode<HBoxContainer>("EnemyHand");
        arena = GetNode<Control>("Arena");

        var list = new List<(ArenaPosition, string)>()
        {
            (ArenaPosition.TopLeft, "Arena/TopLeft"),
            (ArenaPosition.TopMid, "Arena/TopMid"),
            (ArenaPosition.TopRight, "Arena/TopRight"),
            (ArenaPosition.BotLeft, "Arena/BotLeft"),
            (ArenaPosition.BotMid, "Arena/BotMid"),
            (ArenaPosition.BotRight, "Arena/BotRight"),
        };

        foreach (var (key, name) in list)
        {
            arenaPositions[key] = GetNode<Control>(name).Position;
        }

        PlaceCardsInHands();
        UpdateCardLabels();
    }

    private void PlaceCardsInHands()
    {
        var playerCardsCount = 5;
        var enemyCardsCount = 5;

        for (int i = 0; i < playerCardsCount; i++)
        {
            var card = cardInHandScene.Instantiate<CardInHand>();
            playerHand.AddChild(card);
            playerCards.Add(card);
        }

        for (int i = 0; i < enemyCardsCount; i++)
        {
            var card = cardScene.Instantiate<Control>();
            enemyHand.AddChild(card);
        }
    }

    private void UpdateCardLabels()
    {
        for (int i = 0; i < playerCards.Count; i++)
        {
            var card = playerCards[i];
            var cardNumber = i + 1;
            card.SetText(cardNumber.ToString());
        }
    }

    public override void _Input(InputEvent inputEvent)
    {
        var num_keys = new List<string>()
        {
            "num_1",
            "num_2",
            "num_3",
            "num_4",
            "num_5",
            "num_6",
            "num_7",
            "num_8",
            "num_9",
        };

        for (int i = 0; i < num_keys.Count; i++)
        {
            var key = num_keys[i];

            if (inputEvent.IsActionPressed(key))
            {
                PlayCard(i);
                return;
            }
        }
    }

    private int temp = 0;

    private void PlayCard(int index)
    {
        if (index >= playerCards.Count)
        {
            return;
        }

        var card = playerCards[index];
        playerCards.RemoveAt(index);

        // TODO: figure out what slot is free
        var pos = new List<ArenaPosition>()
        {
            ArenaPosition.BotLeft,
            ArenaPosition.BotMid,
            ArenaPosition.BotRight
        };

        AddCardToArena(pos[temp % pos.Count]);
        temp++;

        card.QueueFree();
        UpdateCardLabels();
    }

    private void AddCardToArena(ArenaPosition position)
    {
        var card = cardScene.Instantiate<Control>();
        arena.AddChild(card);
        card.Position = arenaPositions[position];
    }
}
