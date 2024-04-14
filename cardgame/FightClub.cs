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

    private readonly List<ArenaPosition> fightOrder =
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
    private Card previousCard;

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

        PlayNextTurn();
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

        var (attackingCard, position) = nextTurn.Value;

        var oppositePosition = position.Opposite();
        arenaSlots.TryGetValue(oppositePosition, out InPlaySlot targetSlot);

        attackingCard.AttackAnimationFinished += CardFinishedAttackAnimation;
        previousCard = attackingCard;

        attackingCard.StartAttack(new(targetSlot.Card, this));
    }

    private void RoundOver() {
        ResetStuff();
        EmitSignal(SignalName.FightRoundEnded);
    }

    private void SomeoneDied() {
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

    public void CardAttacksTarget(Card attacker, Card target, bool IsPlayersCard) {
        if (target == null) {
            var damage = attacker.Damage;
            var character = IsPlayersCard ? enemyHp : playerHp;
            character.CurrentHp -= damage;

            if (character.IsDead) {
                hasDied = IsPlayersCard ? Who.Enemy : Who.Player;
            }
        } else {
            target.CurrentHp -= attacker.Damage;
            if (target.IsDead) {
                target.PlayDieAnimation();
            } else {
                target.PlayHurtAnimation();
            }
        }
    }

    private (Card, ArenaPosition)? FindNextCardInTurn(Dictionary<ArenaPosition, InPlaySlot> arenaSlots) {
        while (true) {
            if (turnIndex >= fightOrder.Count) {
                return null;
            }

            var position = fightOrder[turnIndex];
            if (arenaSlots[position].IsTaken) {
                return (arenaSlots[position].Card, position);
            }

            // no-one on this square, check if the next square in turn has a card
            turnIndex++;
        }
    }

    private void UnregisterPreviousCard() {
        if (previousCard != null) {
            previousCard.AttackAnimationFinished -= CardFinishedAttackAnimation;
            previousCard = null;
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
