using System.Collections.Generic;
using System.Collections.Immutable;
using Godot;

public partial class Cardgame : Control
{
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
    private readonly List<Card> playerCards = new();
    private readonly List<Card> enemyCards = new();

    private Vector2 playerHandPosition;
    private Vector2 enemyHandPosition;

    private CardPiles playerPiles;
    private CardPiles enemyPiles;

    private Dictionary<ArenaPosition, Card> cardsOnArena = new();

    private readonly Dictionary<ArenaPosition, Label> arenaPositionLabels = new();

    [Export]
    public Color normalFontColor;

    [Export]
    public Color dimmedFontColor;

    private readonly FightClub fight = new();

    private readonly IEnemyAI enemyAI = new RandomAI();

    private Button startFightButton;

    private HpBar playerHp;
    private HpBar enemyHp;

    public override void _Ready()
    {
        playerHandPosition = GetNode<Marker2D>("Markers/PlayerHandPosition").Position;
        enemyHandPosition = GetNode<Marker2D>("Markers/EnemyHandPosition").Position;

        playerPiles = GetNode<CardPiles>("PlayerPiles");
        playerPiles.SetDeck(CardDecks.PlayerDeck(cardScene));
        enemyPiles = GetNode<CardPiles>("EnemyPiles");
        enemyPiles.SetDeck(CardDecks.EnemyDeck(cardScene));

        startFightButton = GetNode<Button>("ButtonStart");

        var playerHitpoints = 20;
        playerHp = GetNode<HpBar>("PlayerHp");
        playerHp.MaxHp = playerHitpoints;
        playerHp.CurrentHp = playerHitpoints;

        var enemyHitpoints = 20;
        enemyHp = GetNode<HpBar>("EnemyHp");
        enemyHp.MaxHp = enemyHitpoints;
        enemyHp.CurrentHp = enemyHitpoints;

        var list = new List<(ArenaPosition, string)>()
        {
            (ArenaPosition.TopLeft, "Markers/Arena/TopLeft"),
            (ArenaPosition.TopMid, "Markers/Arena/TopMid"),
            (ArenaPosition.TopRight, "Markers/Arena/TopRight"),
            (ArenaPosition.BotLeft, "Markers/Arena/BotLeft"),
            (ArenaPosition.BotMid, "Markers/Arena/BotMid"),
            (ArenaPosition.BotRight, "Markers/Arena/BotRight"),
        };

        foreach (var (key, name) in list)
        {
            arenaPositions[key] = GetNode<Marker2D>(name).Position;
        }

        arenaPositionLabels[ArenaPosition.BotLeft] = GetNode<Label>("NumberLabels/NumberLeft");
        arenaPositionLabels[ArenaPosition.BotMid] = GetNode<Label>("NumberLabels/NumberMid");
        arenaPositionLabels[ArenaPosition.BotRight] = GetNode<Label>("NumberLabels/NumberRight");

        fight.FightRoundEnded += FightRoundEnded;
        fight.SomebodyDied += FightEnded;

        DrawCardsToHands();
        SwitchMode(Mode.SelectingCard);
        PlayAITurn();
    }

    private void DrawCardsToHands()
    {
        var playerCardsCount = 5;
        var enemyCardsCount = 5;

        var offset = new Vector2(100.0f, 0.0f);
        {
            var playerDrawnCards = playerPiles.DrawCards(playerCardsCount);

            var startPos = playerHandPosition;
            for (int i = 0; i < playerDrawnCards.Count; i++)
            {
                var card = playerDrawnCards[i];

                var position = startPos + offset * i;
                card.Position = position;

                AddChild(card);
                playerCards.Add(card);
                card.SetNumberLabelVisible(true);
                card.IsPlayersCard = true;
            }
        }

        {
            var enemyDrawnCards = enemyPiles.DrawCards(enemyCardsCount);

            var startPos = enemyHandPosition;
            for (int i = 0; i < enemyDrawnCards.Count; i++)
            {
                var card = enemyDrawnCards[i];
                var position = startPos + offset * i;
                card.Position = position;
                enemyCards.Add(card);
                AddChild(card);
            }
        }

        UpdateCardLabels();
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

        AddCardToArena(card, position);

        UpdateCardLabels();

        SwitchMode(Mode.SelectingCard);
    }

    private void AddCardToArena(Card card, ArenaPosition position)
    {
        card.SetNumberLabelVisible(false);
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
        StartRound();
    }

    private void StartRound()
    {
        startFightButton.Disabled = true;
        SwitchMode(Mode.WatchingBattle);
        fight.StartFight(cardsOnArena, playerHp, enemyHp);

        DiscardCardsInHand();
    }

    private void DiscardCardsInHand()
    {
        playerPiles.DiscardCards(playerCards);
        enemyPiles.DiscardCards(enemyCards);
    }

    public void FightRoundEnded()
    {
        SwitchMode(Mode.SelectingCard);
        startFightButton.Disabled = false;

        DrawCardsToHands();

        PlayAITurn();
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
            AddCardToArena(card, position);
            enemyCards.Remove(card);
        }
    }

    private void FightEnded(bool playerDied)
    {
        if (playerDied)
        {
            GD.Print("Player had died! Oh nooooo");
        }
        else
        {
            GD.Print("You killed the enemy, good job!");
        }
    }
}
