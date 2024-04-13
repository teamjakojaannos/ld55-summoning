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

    enum Mode
    {
        SelectingCard,
        SelectingPosition
    }

    private Mode currentMode = Mode.SelectingCard;
    private int? selectedCardIndex = null;

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
                if (currentMode == Mode.SelectingCard)
                {
                    TrySelectCard(i);
                }
                else if (currentMode == Mode.SelectingPosition)
                {
                    TryPlayCard(i);
                }

                return;
            }
        }
    }

    private void TrySelectCard(int cardIndex)
    {
        if (cardIndex >= playerCards.Count)
        {
            return;
        }

        selectedCardIndex = cardIndex;
        currentMode = Mode.SelectingPosition;
        SetHighlights(toCards: false, toPositions: true);
    }

    private void TryPlayCard(int positionIndex)
    {
        var positions = new List<ArenaPosition>()
        {
            ArenaPosition.BotLeft,
            ArenaPosition.BotMid,
            ArenaPosition.BotRight
        };

        var wrongMode = currentMode != Mode.SelectingPosition;
        var tooHighIndex = positionIndex >= positions.Count;
        var positionTaken = false; // TODO

        if (wrongMode || tooHighIndex || positionTaken)
        {
            return;
        }

        if (selectedCardIndex == null || selectedCardIndex.Value >= playerCards.Count)
        {
            // we should never end up in here
            currentMode = Mode.SelectingCard;
            SetHighlights(toCards: true, toPositions: false);
            return;
        }

        var cardIndex = selectedCardIndex.Value;
        var card = playerCards[cardIndex];
        playerCards.RemoveAt(cardIndex);

        var position = positions[positionIndex];
        AddCardToArena(card, position);

        UpdateCardLabels();

        currentMode = Mode.SelectingCard;
        SetHighlights(toCards: true, toPositions: false);
    }

    private void AddCardToArena(Card card, ArenaPosition position)
    {
        card.SetNumberLabelVisible(false);
        playerHand.RemoveChild(card);
        arena.AddChild(card);
        card.Position = arenaPositions[position];
    }

    private void SetHighlights(bool toCards, bool toPositions)
    {
        // TODO:
    }
}
