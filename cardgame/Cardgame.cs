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

	private PackedScene defaultCardScene = GD.Load<PackedScene>("res://cardgame/card.tscn");
	private readonly Dictionary<MonsterVariant, PackedScene> cardScenes = new(){
			{ MonsterVariant.Air1, GD.Load<PackedScene>("res://cardgame/cards/air_card_1.tscn")},
			{ MonsterVariant.Air2, GD.Load<PackedScene>("res://cardgame/cards/air_card_2.tscn")},
			{ MonsterVariant.Earth1, GD.Load<PackedScene>("res://cardgame/cards/earth_card_1.tscn")},
			{ MonsterVariant.Earth2, GD.Load<PackedScene>("res://cardgame/cards/earth_card_2.tscn")},
			{ MonsterVariant.Fire1, GD.Load<PackedScene>("res://cardgame/cards/fire_card_1.tscn")},
			{ MonsterVariant.Fire2, GD.Load<PackedScene>("res://cardgame/cards/fire_card_2.tscn")},
			{ MonsterVariant.Water1, GD.Load<PackedScene>("res://cardgame/cards/water_card_1.tscn")},
			{ MonsterVariant.Water2, GD.Load<PackedScene>("res://cardgame/cards/water_card_2.tscn")},
	};

	private List<Card> PlayerCards {
		get => playerHand.Cards;
	}
	private List<Card> EnemyCards {
		get => enemyHand.Cards;
	}

	private PlayerHand playerHand;
	private EnemyHand enemyHand;

	private CardPiles playerPiles;
	private CardPiles enemyPiles;

	[Export]
	public Color normalFontColor;

	[Export]
	public Color dimmedFontColor;

	[Export]
	public float CardDiscardSpeed = 20.0f;

	[Export]
	public float PlayedCardSpeed = 15.0f;

	[Export]
	public float CardDealSpeed = 15.0f;

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

	[Signal]
	public delegate void PlayerNoMoreMovesEventHandler();

	[Signal]
	public delegate void CardgameOverEventHandler(bool playerWon);

	public Vector2 DrawPilePosition {
		get => playerPiles.DrawPilePosition();
	}

	public bool IsPositionOnTableTaken(ArenaPosition position) {
		return arenaSlots[position].IsTaken;
	}

	public bool IsPlayerTableFull() {
		return arenaSlots[ArenaPosition.PlayerLeft].IsTaken
			&& arenaSlots[ArenaPosition.PlayerMid].IsTaken
			&& arenaSlots[ArenaPosition.PlayerRight].IsTaken;
	}

	public override void _Ready() {
		playerHand = GetNode<PlayerHand>("PlayerHand");
		enemyHand = GetNode<EnemyHand>("EnemyHand");

		playerPiles = GetNode<CardPiles>("PlayerPiles");
		enemyPiles = GetNode<CardPiles>("EnemyPiles");

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

		Reset();

		SwitchMode(Mode.WaitingForAnimation);
	}

	public void Reset() {
		playerHand.Reset();
		enemyHand.Reset();
	}

	private CardDeck CreateDeck(List<CardStats> cardStats) {
		var cards = new List<Card>();

		foreach (var stats in cardStats) {
			PackedScene cardScene;

			if (cardScenes.ContainsKey(stats.MonsterVariant)) {
				cardScene = cardScenes[stats.MonsterVariant];
			} else {
				GD.PrintErr($"Can't find card scene for monster variant {stats.MonsterVariant}");
				cardScene = defaultCardScene;
			}

			var card = cardScene.Instantiate<Card>();
			card.SetStats(stats);

			card.Visible = false;
			AddChild(card);

			cards.Add(card);
		}

		return new CardDeck(cards);
	}

	public void StartCombat(
				List<CardStats> playerCardStats,
				List<CardStats> enemyCardStats,
				bool shufflePlayerDeck = true,
				bool shuffleEnemyDeck = true
		) {
		var playerDeck = CreateDeck(playerCardStats);
		if (shufflePlayerDeck) {
			playerDeck.Shuffle();
		}
		playerPiles.SetDeck(playerDeck);

		var enemyDeck = CreateDeck(enemyCardStats);
		if (shuffleEnemyDeck) {
			enemyDeck.Shuffle();
		}
		enemyPiles.SetDeck(enemyDeck);

		DealCards();
	}

	private void DealCards() {
		cardDealTimer.Start();

		var playerCardsCount = 5;
		var enemyCardsCount = 5;

		var playerDrawnCards = playerPiles.DrawCards(playerCardsCount);

		playerHand.AddCards(playerDrawnCards, CardDealSpeed);

		var enemyDrawnCards = enemyPiles.DrawCards(enemyCardsCount);

		enemyHand.AddCards(enemyDrawnCards, CardDealSpeed);

		if (IsPlayerTableFull()) {
			EmitSignal(nameof(PlayerNoMoreMoves));
		}
	}

	private void CardDealingDone() {
		foreach (var card in PlayerCards) {
			card.StopMovement();
		}

		foreach (var card in EnemyCards) {
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

		playerHand.PlayCard(cardIndex);

		SwitchMode(Mode.SelectingCard);

		if (IsPlayerTableFull()) {
			EmitSignal(nameof(PlayerNoMoreMoves));
		}
	}

	private void AddCardToArena(Card card, ArenaPosition position) {
		var arenaSlot = arenaSlots[position];
		arenaSlot.AddCardAsChild(card, PlayedCardSpeed);
		card.SetInfoVisible(true);
	}

	private void SwitchMode(Mode newMode) {
		ClearSelectedCardHighlight();
		currentMode = newMode;

		switch (newMode) {
			case Mode.SelectingCard:
				//SetHighlights(toCards: true, toPositions: false);
				break;
			case Mode.SelectingPosition:
				//SetHighlights(toCards: false, toPositions: true);
				break;
			case Mode.WatchingBattle:
			case Mode.WaitingForAnimation:
			case Mode.WaitingForAIMoves:
				//SetHighlights(toCards: false, toPositions: false);
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

		playerPiles.DiscardCards(PlayerCards, CardDiscardSpeed);
		enemyPiles.DiscardCards(EnemyCards, CardDiscardSpeed);
		playerHand.Reset();
		enemyHand.Reset();

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

		var newList = new List<Card>(EnemyCards);

		var enemyMoves = enemyAI.GetCardsPlacement(
				arenaSlots.ToImmutableDictionary(),
				newList,
				new() { ArenaPosition.OpponentLeft, ArenaPosition.OpponentMid, ArenaPosition.OpponentRight }
		);

		foreach (var (position, card) in enemyMoves) {
			AddCardToArena(card, position);

			// FIXME: is this the same as player playing a card?
			EnemyCards.Remove(card);
		}

		aiMoveTimer.Start();
	}

	private void AIMovesDone() {
		SwitchMode(Mode.SelectingCard);
		startFightButton.Disabled = false;
	}

	private void FightEnded(bool playerDied) {
		var playerWon = !playerDied;
		EmitSignal(SignalName.CardgameOver, playerWon);
	}
}
