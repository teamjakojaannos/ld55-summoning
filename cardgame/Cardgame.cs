using System.Collections.Generic;
using System.Collections.Immutable;
using Godot;

public partial class Cardgame : Control
{
    private readonly bool DEBUG_MODE = false;

    private readonly Dictionary<ArenaPosition, Vector2> arenaPositions = new();

    enum Mode
    {
        SelectingCard,
        SelectingPosition,
        WatchingBattle,
    }

    private Mode currentMode = Mode.SelectingCard;
    private int? selectedCardIndex = null;

    private PackedScene cardScene = GD.Load<PackedScene>("res://cardgame/card.tscn");
    private Control playerHand;
    private Control enemyHand;

    private Control arena;

    private readonly List<Card> playerCards = new();
    private readonly List<Card> enemyCards = new();

    private CardDeck playerDeck;
    private CardDeck enemyDeck;

    private Dictionary<ArenaPosition, Card> cardsOnArena = new();

    private readonly Dictionary<ArenaPosition, Label> arenaPositionLabels = new();

    [Export]
    public Color normalFontColor;

    [Export]
    public Color dimmedFontColor;

    private readonly FightClub fight = new();

    private readonly IEnemyAI enemyAI = new RandomAI();

    private Button startFightButton;

    public override void _Ready()
    {
        playerDeck = CardDecks.PlayerDeck(cardScene);
        enemyDeck = CardDecks.EnemyDeck(cardScene);

        playerHand = GetNode<Control>("Hand");
        enemyHand = GetNode<Control>("EnemyHand");
        arena = GetNode<Control>("Arena");

        startFightButton = GetNode<Button>("ButtonStart");

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

        arenaPositionLabels[ArenaPosition.BotLeft] = GetNode<Label>(
            "Arena/NumberLabels/NumberLeft"
        );
        arenaPositionLabels[ArenaPosition.BotMid] = GetNode<Label>("Arena/NumberLabels/NumberMid");
        arenaPositionLabels[ArenaPosition.BotRight] = GetNode<Label>(
            "Arena/NumberLabels/NumberRight"
        );

        // tried connecting this in editor, didn't work for some reason. Let's do it manually then
        fight.FightRoundEnded += FightRoundEnded;

        PlaceCardsInHands();
        UpdateCardLabels();
        SwitchMode(Mode.SelectingCard);
        PlayAITurn();
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
        var startPos = new Vector2(400.0f, 0);
        var offset = new Vector2(100.0f, 0.0f);

        var playerDrawnCards = playerDeck.Draw(playerCardsCount);

        for (int i = 0; i < playerDrawnCards.Count; i++)
        {
            var card = playerDrawnCards[i];

            var position = startPos + offset * i;
            card.Position = position;

            playerHand.AddChild(card);
            playerCards.Add(card);
            card.SetNumberLabelVisible(true);
        }

        var enemyDrawnCards = enemyDeck.Draw(enemyCardsCount);

        for (int i = 0; i < enemyDrawnCards.Count; i++)
        {
            var card = enemyDrawnCards[i];
            var position = startPos + offset * i;
            card.Position = position;
            enemyCards.Add(card);
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
        if (inputEvent.IsActionPressed("esc"))
        {
            if (currentMode == Mode.SelectingPosition)
            {
                SwitchMode(Mode.SelectingCard);
                return;
            }
        }

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

        SwitchMode(Mode.SelectingPosition);

        selectedCardIndex = cardIndex;
        HighlightSelectedCard();
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

        if (wrongMode || tooHighIndex)
        {
            return;
        }

        var position = positions[positionIndex];
        var positionTaken = cardsOnArena.ContainsKey(position);
        if (positionTaken)
        {
            return;
        }

        if (selectedCardIndex == null || selectedCardIndex.Value >= playerCards.Count)
        {
            // we should never end up in here
            SwitchMode(Mode.SelectingCard);
            return;
        }

        ClearSelectedCardHighlight();
        var cardIndex = selectedCardIndex.Value;
        var card = playerCards[cardIndex];
        playerCards.RemoveAt(cardIndex);

        AddCardToArena(card, position, playerHand);

        UpdateCardLabels();

        SwitchMode(Mode.SelectingCard);
    }

    private void AddCardToArena(Card card, ArenaPosition position, Control previousParent)
    {
        card.SetNumberLabelVisible(false);
        previousParent.RemoveChild(card);
        arena.AddChild(card);
        card.Position = arenaPositions[position];
        cardsOnArena[position] = card;
    }

    private void SwitchMode(Mode newMode)
    {
        ClearSelectedCardHighlight();
        currentMode = newMode;

        switch (newMode)
        {
            case Mode.SelectingCard:
                SetHighlights(toCards: true, toPositions: false);
                break;
            case Mode.SelectingPosition:
                SetHighlights(toCards: false, toPositions: true);
                break;
            case Mode.WatchingBattle:
                SetHighlights(toCards: false, toPositions: false);
                break;
        }
    }

    private void HighlightSelectedCard()
    {
        if (selectedCardIndex == null || selectedCardIndex.Value >= playerCards.Count)
        {
            return;
        }

        var card = playerCards[selectedCardIndex.Value];
        card.SetHighlighted(true);
    }

    private void ClearSelectedCardHighlight()
    {
        if (selectedCardIndex == null || selectedCardIndex.Value >= playerCards.Count)
        {
            return;
        }

        var card = playerCards[selectedCardIndex.Value];
        card.SetHighlighted(false);
    }

    private void SetHighlights(bool toCards, bool toPositions)
    {
        var cardFontColor = toCards ? normalFontColor : dimmedFontColor;
        foreach (var card in playerCards)
        {
            card.SetNumberLabelColor(cardFontColor);
        }

        foreach (var (position, label) in arenaPositionLabels)
        {
            var positionTaken = cardsOnArena.ContainsKey(position);
            var useHighlight = toPositions && !positionTaken;
            var positionColor = useHighlight ? normalFontColor : dimmedFontColor;
            label.LabelSettings.FontColor = positionColor;
        }
    }

    private void StartButtonPressed()
    {
        SwitchMode(Mode.WatchingBattle);
        fight.StartFight(cardsOnArena);
        startFightButton.Disabled = true;
    }

    public void FightRoundEnded()
    {
        SwitchMode(Mode.SelectingCard);
        startFightButton.Disabled = false;
    }

    private void PlayAITurn()
    {
        var newList = new List<Card>(enemyCards);

        var enemyMoves = enemyAI.GetCardsPlacement(
            cardsOnArena.ToImmutableDictionary(),
            newList,
            new() { ArenaPosition.TopLeft, ArenaPosition.TopMid, ArenaPosition.TopRight }
        );

        foreach (var (position, card) in enemyMoves)
        {
            AddCardToArena(card, position, enemyHand);
            enemyCards.Remove(card);
        }
    }
}
