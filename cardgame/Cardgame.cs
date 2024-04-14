using System.Collections.Generic;
using System.Collections.Immutable;
using Godot;

public partial class Cardgame : Control {
    private readonly Dictionary<ArenaPosition, InPlaySlot> arenaSlots = new();

    enum Mode {
        WaitingForAnimation,
        WaitingForAIMoves,
        SelectingCard,
        SelectingPosition,
        WatchingBattle,
    }

    private Mode currentMode = Mode.WaitingForAnimation;
    private int? selectedCardIndex = null;

    private PackedScene cardScene = GD.Load<PackedScene>("res://cardgame/card.tscn");
    private List<Card> PlayerCards {
        get => playerHand.Cards;
    }
    private readonly List<Card> enemyCards = new();

    private PlayerHand playerHand;
    private Control enemyHand;

    private CardPiles playerPiles;
    private CardPiles enemyPiles;

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

    private Timer cardDealTimer;
    private Timer aiMoveTimer;
    private Timer cardDiscardTimer;

    [Signal]
    public delegate void PlayerSelectedCardInHandEventHandler(int cardIndex);

    [Signal]
    public delegate void PlayerDeselectedCardInHandEventHandler(int cardIndex);

    public Vector2 DrawPilePosition {
        get => playerPiles.DrawPilePosition();
    }

    public bool IsPositionOnTableTaken(ArenaPosition position) {
        return arenaSlots[position].IsTaken;
    }

    public override void _Ready() {
        playerHand = GetNode<PlayerHand>("PlayerHand");
        enemyHand = GetNode<Control>("EnemyHand");

        playerPiles = GetNode<CardPiles>("PlayerPiles");
        var pDeck = CardDecks.PlayerDeck(cardScene);
        pDeck.Shuffle();
        playerPiles.SetDeck(pDeck);
        playerPiles.AddCardsAsChildren(this);

        enemyPiles = GetNode<CardPiles>("EnemyPiles");
        var eDeck = CardDecks.EnemyDeck(cardScene);
        eDeck.Shuffle();
        enemyPiles.SetDeck(eDeck);
        enemyPiles.AddCardsAsChildren(this);

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
            (ArenaPosition.OpponentLeft, "Arena/OpponentLeft"),
            (ArenaPosition.OpponentMid, "Arena/OpponentMid"),
            (ArenaPosition.OpponentRight, "Arena/OpponentRight"),
            (ArenaPosition.PlayerLeft, "Arena/PlayerLeft"),
            (ArenaPosition.PlayerMid, "Arena/PlayerMid"),
            (ArenaPosition.PlayerRight, "Arena/PlayerRight"),
        };

        foreach (var (key, name) in list) {
            arenaSlots[key] = GetNode<InPlaySlot>(name);
        }

        fight.FightRoundEnded += FightRoundEnded;
        fight.SomebodyDied += FightEnded;

        cardDealTimer = GetNode<Timer>("CardDealTimer");
        cardDealTimer.Timeout += CardDealingDone;

        aiMoveTimer = GetNode<Timer>("AIMoveTimer");
        aiMoveTimer.Timeout += AIMovesDone;

        cardDiscardTimer = GetNode<Timer>("CardDiscardTimer");
        cardDiscardTimer.Timeout += CardDiscardDone;

        SwitchMode(Mode.WaitingForAnimation);
    }

    public void StartCombat() {
        // TODO: check if already started?
        DealCards();
    }

    private void DealCards() {
        cardDealTimer.Start();

        var playerCardsCount = 5;
        var enemyCardsCount = 5;
        var movementTime = (float)cardDealTimer.WaitTime;

        var offset = new Vector2(100.0f, 0.0f);
        var playerDrawnCards = playerPiles.DrawCards(playerCardsCount);

        playerHand.TakeCards(playerDrawnCards, movementTime);

        var enemyDrawnCards = enemyPiles.DrawCards(enemyCardsCount);

        for (int i = 0; i < enemyDrawnCards.Count; i++) {
            var card = enemyDrawnCards[i];
            card.MoveInstantlyTo(enemyPiles.DrawPilePosition());
            var targetPosition = enemyHand.Position + offset * i;
            card.StartMovingTo(targetPosition, movementTime);

            enemyCards.Add(card);

            card.Visible = true;
        }

        UpdateCardLabels();
    }

    private void CardDealingDone() {
        foreach (var card in PlayerCards) {
            card.StopMovement();
        }

        foreach (var card in enemyCards) {
            card.StopMovement();
        }

        if (playerPiles.DrawPileEmpty()) {
            playerPiles.RecycleDiscardPile();
            playerPiles.PlayRecycleAnimation();
        }

        if (enemyPiles.DrawPileEmpty()) {
            enemyPiles.RecycleDiscardPile();
            enemyPiles.PlayRecycleAnimation();
        }

        PlayAITurn();
    }

    private void UpdateCardLabels() {
        for (int i = 0; i < PlayerCards.Count; i++) {
            var card = PlayerCards[i];
            var cardNumber = i + 1;
            card.SetNumberLabelText(cardNumber.ToString());
        }
    }

    public override void _Input(InputEvent inputEvent) {
        if (inputEvent.IsActionPressed("esc")) {
            if (currentMode == Mode.SelectingPosition) {
                SwitchMode(Mode.SelectingCard);
                if (selectedCardIndex != null) {
                    EmitSignal(nameof(PlayerDeselectedCardInHand), selectedCardIndex.Value);
                }
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

        for (int i = 0; i < num_keys.Count; i++) {
            var key = num_keys[i];

            if (inputEvent.IsActionPressed(key)) {
                if (currentMode == Mode.SelectingCard) {
                    TrySelectCard(i);
                } else if (currentMode == Mode.SelectingPosition) {
                    TryPlayCard(i);
                }

                return;
            }
        }
    }

    private void TrySelectCard(int cardIndex) {
        if (cardIndex >= PlayerCards.Count) {
            return;
        }

        SwitchMode(Mode.SelectingPosition);

        selectedCardIndex = cardIndex;
        HighlightSelectedCard();

        EmitSignal(nameof(PlayerSelectedCardInHand), cardIndex);
    }

    private void TryPlayCard(int positionIndex) {
        var positions = new List<ArenaPosition>()
        {
            ArenaPosition.PlayerLeft,
            ArenaPosition.PlayerMid,
            ArenaPosition.PlayerRight
        };

        var wrongMode = currentMode != Mode.SelectingPosition;
        var tooHighIndex = positionIndex >= positions.Count;

        if (wrongMode || tooHighIndex) {
            return;
        }

        var position = positions[positionIndex];
        var positionTaken = arenaSlots[position].Card != null;
        if (positionTaken) {
            return;
        }

        if (selectedCardIndex == null || selectedCardIndex.Value >= PlayerCards.Count) {
            // we should never end up in here
            SwitchMode(Mode.SelectingCard);
            return;
        }

        var cardIndex = selectedCardIndex.Value;
        EmitSignal(nameof(PlayerDeselectedCardInHand), cardIndex);
        ClearSelectedCardHighlight();
        var card = PlayerCards[cardIndex];
        PlayerCards.RemoveAt(cardIndex);

        AddCardToArena(card, position);

        UpdateCardLabels();

        SwitchMode(Mode.SelectingCard);
    }

    private void AddCardToArena(Card card, ArenaPosition position) {
        var cardMoveTime = 0.5f;
        card.SetNumberLabelVisible(false);

        var arenaSlot = arenaSlots[position];
        card.StartMovingTo(arenaSlot.Position, cardMoveTime);
        arenaSlot.Card = card;
    }

    private void SwitchMode(Mode newMode) {
        ClearSelectedCardHighlight();
        currentMode = newMode;

        switch (newMode) {
            case Mode.SelectingCard:
                SetHighlights(toCards: true, toPositions: false);
                break;
            case Mode.SelectingPosition:
                SetHighlights(toCards: false, toPositions: true);
                break;
            case Mode.WatchingBattle:
            case Mode.WaitingForAnimation:
            case Mode.WaitingForAIMoves:
                SetHighlights(toCards: false, toPositions: false);
                break;
        }
    }

    private void HighlightSelectedCard() {
        if (selectedCardIndex == null || selectedCardIndex.Value >= PlayerCards.Count) {
            return;
        }

        var card = PlayerCards[selectedCardIndex.Value];
        card.SetHighlighted(true);
    }

    private void ClearSelectedCardHighlight() {
        if (selectedCardIndex == null || selectedCardIndex.Value >= PlayerCards.Count) {
            return;
        }

        var card = PlayerCards[selectedCardIndex.Value];
        card.SetHighlighted(false);
    }

    private void SetHighlights(bool toCards, bool toPositions) {
        var cardFontColor = toCards ? normalFontColor : dimmedFontColor;
        foreach (var card in PlayerCards) {
            card.SetNumberLabelColor(cardFontColor);
        }

        foreach (var (position, label) in arenaPositionLabels) {
            var positionTaken = IsPositionOnTableTaken(position);
            var useHighlight = toPositions && !positionTaken;
            var positionColor = useHighlight ? normalFontColor : dimmedFontColor;
            label.LabelSettings.FontColor = positionColor;
        }
    }

    private void StartButtonPressed() {
        StartRound();
    }

    private void StartRound() {
        startFightButton.Disabled = true;
        SwitchMode(Mode.WaitingForAnimation);

        // force cards into place if still in transit
        foreach (var (_, slot) in arenaSlots) {
            slot.Card?.StopMovement();
        }

        var time = (float)cardDiscardTimer.WaitTime;

        playerPiles.DiscardCards(PlayerCards, time);
        enemyPiles.DiscardCards(enemyCards, time);

        cardDiscardTimer.Start();
    }

    private void CardDiscardDone() {
        // stop movement if they are still in transit
        playerPiles.EndDiscardAnimation();
        enemyPiles.EndDiscardAnimation();

        fight.StartFight(arenaSlots, playerHp, enemyHp);
        SwitchMode(Mode.WatchingBattle);
    }

    public void FightRoundEnded() {
        DealCards();
    }

    private void PlayAITurn() {
        SwitchMode(Mode.WaitingForAIMoves);

        var newList = new List<Card>(enemyCards);

        var enemyMoves = enemyAI.GetCardsPlacement(
            arenaSlots.ToImmutableDictionary(),
            newList,
            new() { ArenaPosition.OpponentLeft, ArenaPosition.OpponentMid, ArenaPosition.OpponentRight }
        );

        foreach (var (position, card) in enemyMoves) {
            AddCardToArena(card, position);
            enemyCards.Remove(card);
        }

        aiMoveTimer.Start();
    }

    private void AIMovesDone() {
        SwitchMode(Mode.SelectingCard);
        startFightButton.Disabled = false;
    }

    private void FightEnded(bool playerDied) {
        if (playerDied) {
            GD.Print("Player had died! Oh nooooo");
        } else {
            GD.Print("You killed the enemy, good job!");
        }
    }
}
