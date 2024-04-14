using System.Collections.Generic;
using Godot;

public partial class FightClub : Control
{
    enum Who
    {
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
            ArenaPosition.OpponentLeft,
            ArenaPosition.OpponentMid,
            ArenaPosition.OpponentRight,
            ArenaPosition.PlayerLeft,
            ArenaPosition.PlayerMid,
            ArenaPosition.PlayerRight,
        };
    private int turnIndex = 0;

    private bool fightInProgress = false;
    private Dictionary<ArenaPosition, Card> cardsOnArena;
    private Who hasDied = Who.Nobody;

    private HpBar playerHp;
    private HpBar enemyHp;
    private Card previousCard;

    public void StartFight(
        Dictionary<ArenaPosition, Card> cardsOnArena,
        HpBar playerHp,
        HpBar enemyHp
    )
    {
        if (fightInProgress)
        {
            return;
        }

        ResetStuff();
        fightInProgress = true;
        turnIndex = 0;
        this.cardsOnArena = cardsOnArena;
        this.playerHp = playerHp;
        this.enemyHp = enemyHp;

        PlayNextTurn();
    }

    private void PlayNextTurn()
    {
        UnregisterPreviousCard();
        CleanupDeadCards();

        if (hasDied != Who.Nobody)
        {
            SomeoneDied();
            return;
        }

        var nextTurn = FindNextCardInTurn(cardsOnArena);
        if (nextTurn == null)
        {
            RoundOver();

            return;
        }

        var (attackingCard, position) = nextTurn.Value;

        var oppositePosition = position.Opposite();
        cardsOnArena.TryGetValue(oppositePosition, out Card targetCard);

        attackingCard.AttackAnimationFinished += CardFinishedAttackAnimation;
        previousCard = attackingCard;

        attackingCard.StartAttack(new(targetCard, this));
    }

    private void RoundOver()
    {
        ResetStuff();
        EmitSignal(SignalName.FightRoundEnded);
    }

    private void SomeoneDied()
    {
        var playerDead = hasDied == Who.Player;
        ResetStuff();
        EmitSignal(SignalName.SomebodyDied, playerDead);
    }

    private void ResetStuff()
    {
        fightInProgress = false;
        cardsOnArena = null;
        playerHp = null;
        enemyHp = null;
        hasDied = Who.Nobody;
    }

    public void CardAttacksTarget(Card attacker, Card target, bool IsPlayersCard)
    {
        if (target == null)
        {
            var damage = attacker.Damage;
            var character = IsPlayersCard ? enemyHp : playerHp;
            character.CurrentHp -= damage;

            if (character.IsDead)
            {
                hasDied = IsPlayersCard ? Who.Enemy : Who.Player;
            }
        }
        else
        {
            target.CurrentHp -= attacker.Damage;
            if (target.IsDead)
            {
                target.PlayDieAnimation();
            }
            else
            {
                target.PlayHurtAnimation();
            }
        }
    }

    private (Card, ArenaPosition)? FindNextCardInTurn(Dictionary<ArenaPosition, Card> cardsOnArena)
    {
        while (true)
        {
            if (turnIndex >= fightOrder.Count)
            {
                return null;
            }

            var position = fightOrder[turnIndex];
            if (cardsOnArena.ContainsKey(position))
            {
                return (cardsOnArena[position], position);
            }

            // no-one on this square, check if the next square in turn has a card
            turnIndex++;
        }
    }

    private void UnregisterPreviousCard()
    {
        if (previousCard != null)
        {
            previousCard.AttackAnimationFinished -= CardFinishedAttackAnimation;
            previousCard = null;
        }
    }

    public void CardFinishedAttackAnimation()
    {
        turnIndex++;
        PlayNextTurn();
    }

    private void CleanupDeadCards()
    {
        foreach (var (position, card) in cardsOnArena)
        {
            if (card.IsDead)
            {
                cardsOnArena.Remove(position);
                card.QueueFree();
            }
        }
    }
}
