using System.Collections.Generic;
using Godot;

public partial class Cardgame : Control
{
    private readonly bool DEBUG_MODE = false;

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
    private HBoxContainer playerHand;
    private HBoxContainer enemyHand;

    private Control arena;

    private readonly List<Card> playerCards = new();

    private CardDeck playerDeck;
    private CardDeck enemyDeck;

    public override void _Ready()
    {
        playerDeck = CardDecks.PlayerDeck(cardScene);
        enemyDeck = CardDecks.EnemyDeck(cardScene);

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

        if (DEBUG_MODE)
        {
            GD.Print("*********************");
            GD.Print("* Debug mode is ON! *");
            GD.Print("*********************");
        }
        else
        {
            HideDebug(this);
        }

        PlaceCardsInHands();
        UpdateCardLabels();
    }

    private static void HideDebug(Node node)
    {
        var children = node.GetChildren(true);

        foreach (var child in children)
        {
            var name = child.Name.ToString();

            if (name.StartsWith("Debug") && child is CanvasItem item)
            {
                item.Visible = false;
            }

            HideDebug(child);
        }
    }

    private void PlaceCardsInHands()
    {
        var playerCardsCount = 5;
        var enemyCardsCount = 5;

        var playerDrawnCards = playerDeck.Draw(playerCardsCount);

        foreach (var card in playerDrawnCards)
        {
            playerHand.AddChild(card);
            playerCards.Add(card);
            card.SetNumberLabelVisible(true);
        }

        var enemyDrawnCards = enemyDeck.Draw(enemyCardsCount);

        foreach (var card in enemyDrawnCards)
        {
            enemyHand.AddChild(card);
        }
    }

    private void UpdateCardLabels()
    {
        for (int i = 0; i < playerCards.Count; i++)
        {
            var card = playerCards[i];
            var cardNumber = i + 1;
            card.SetNumberLabelText(cardNumber.ToString());
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

        AddCardToArena(card, pos[temp % pos.Count]);
        temp++;

        UpdateCardLabels();
    }

    private void AddCardToArena(Card card, ArenaPosition position)
    {
        card.SetNumberLabelVisible(false);
        playerHand.RemoveChild(card);
        arena.AddChild(card);
        card.Position = arenaPositions[position];
    }
}