using System.Collections.Generic;
using Godot;

public partial class FightClub : Control {
	enum Who {
		Nobody,
		Player,
		Enemy,
	}

	[Signal]
	public delegate void FightRoundEndedEventHandler();

	[Signal]
	public delegate void SomebodyDiedEventHandler(bool playerDied);

	private List<ArenaPosition> fightOrder =
			new()
			{
						ArenaPosition.PlayerLeft,
						ArenaPosition.PlayerMid,
						ArenaPosition.PlayerRight,
						ArenaPosition.OpponentLeft,
						ArenaPosition.OpponentMid,
						ArenaPosition.OpponentRight,
			};
	private int turnIndex = 0;

	private bool fightInProgress = false;
	private Dictionary<ArenaPosition, InPlaySlot> arenaSlots;
	private Who hasDied = Who.Nobody;

	private HpBar playerHp;
	private HpBar enemyHp;
	private InPlaySlot previousSlot;

	private RandomNumberGenerator rng = new();

	public void StartFight(
			Dictionary<ArenaPosition, InPlaySlot> arenaSlots,
			HpBar playerHp,
			HpBar enemyHp
	) {
		if (fightInProgress) {
			return;
		}

		ResetStuff();
		fightInProgress = true;
		turnIndex = 0;
		this.arenaSlots = arenaSlots;
		this.playerHp = playerHp;
		this.enemyHp = enemyHp;

		fightOrder = CalculateTurnOrder();

		PlayNextTurn();
	}

	private List<ArenaPosition> CalculateTurnOrder() {
		var result = new List<ArenaPosition>();

		var lanes = new List<(ArenaPosition, ArenaPosition)>(){
			(ArenaPosition.OpponentLeft,ArenaPosition.PlayerLeft),
			(ArenaPosition.OpponentMid,ArenaPosition.PlayerMid),
			(ArenaPosition.OpponentRight,ArenaPosition.PlayerRight),
		};

		foreach (var (a, b) in lanes) {
			var slot_a = arenaSlots[a];
			var slot_b = arenaSlots[b];

			var order_a = slot_a.IsTaken ? slot_a.Card.Element.TurnOrder() : 0;
			var order_b = slot_b.IsTaken ? slot_b.Card.Element.TurnOrder() : 0;

			bool a_first;

			if (order_a > order_b) {
				a_first = true;
			} else if (order_a < order_b) {
				a_first = false;
			} else {
				a_first = rng.RandiRange(0, 1) == 0;
			}

			if (a_first) {
				result.Add(a);
				result.Add(b);
			} else {
				result.Add(b);
				result.Add(a);
			}
		}

		return result;
	}

	private void PlayNextTurn() {
		UnregisterPreviousCard();
		CleanupDeadCards();

		if (hasDied != Who.Nobody) {
			SomeoneDied();
			return;
		}

		var nextTurn = FindNextCardInTurn(arenaSlots);
		if (nextTurn == null) {
			RoundOver();

			return;
		}

		var (attackingSlot, position) = nextTurn.Value;

		var oppositePosition = position.Opposite();
		arenaSlots.TryGetValue(oppositePosition, out InPlaySlot targetSlot);

		attackingSlot.AttackAnimationFinished += CardFinishedAttackAnimation;
		previousSlot = attackingSlot;

		attackingSlot.StartAttack(new(targetSlot, this));
	}

	private void RoundOver() {
		ResetStuff();
		EmitSignal(SignalName.FightRoundEnded);
	}

	private void SomeoneDied() {
		foreach (var (position, slot) in arenaSlots) {
			if (slot.IsFree) {
				continue;
			}

			var card = slot.Card;
			arenaSlots[position].Clear();
			card.QueueFree();
		}

		var playerDead = hasDied == Who.Player;
		ResetStuff();
		EmitSignal(SignalName.SomebodyDied, playerDead);
	}

	private void ResetStuff() {
		fightInProgress = false;
		arenaSlots = null;
		playerHp = null;
		enemyHp = null;
		hasDied = Who.Nobody;
	}

	public void CardAttacksTarget(InPlaySlot attacker, InPlaySlot target, bool IsPlayersCard) {
		if (target.IsFree) {
			var damage = attacker.Card.Damage;
			var character = IsPlayersCard ? enemyHp : playerHp;
			character.CurrentHp -= damage;

			if (character.IsDead) {
				hasDied = IsPlayersCard ? Who.Enemy : Who.Player;
			}
		} else {
			var damage = CalculateDamage(attacker.Card, target.Card);

			target.Card.CurrentHp -= damage;
			if (target.Card.IsDead) {
				target.PlayDieAnimation();
			} else {
				target.PlayHurtAnimation();
			}
		}
	}

	private static int CalculateDamage(Card attacker, Card target) {
		int baseDamage = attacker.Damage;
		float elementMultiplier = attacker.Element.DamageMultiplier(target.Element);

		int result = Mathf.FloorToInt(baseDamage * elementMultiplier);
		return Mathf.Max(result, 1);
	}

	private (InPlaySlot, ArenaPosition)? FindNextCardInTurn(Dictionary<ArenaPosition, InPlaySlot> arenaSlots) {
		while (true) {
			if (turnIndex >= fightOrder.Count) {
				return null;
			}

			var position = fightOrder[turnIndex];
			if (arenaSlots[position].IsTaken) {
				return (arenaSlots[position], position);
			}

			// no-one on this square, check if the next square in turn has a card
			turnIndex++;
		}
	}

	private void UnregisterPreviousCard() {
		if (previousSlot != null) {
			previousSlot.AttackAnimationFinished -= CardFinishedAttackAnimation;
			previousSlot = null;
		}
	}

	public void CardFinishedAttackAnimation() {
		turnIndex++;
		PlayNextTurn();
	}

	private void CleanupDeadCards() {
		foreach (var (position, slot) in arenaSlots) {
			if (slot.IsFree) {
				continue;
			}

			var card = slot.Card;
			if ((bool)(card?.IsDead)) {
				arenaSlots[position].Clear();
				card.QueueFree();
			}
		}
	}
}
