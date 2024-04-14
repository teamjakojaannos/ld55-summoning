using System.Collections.Generic;
using Godot;

public partial class FightClub : Control
{
    [Signal]
    public delegate void FightRoundEndedEventHandler();

    private readonly List<ArenaPosition> fightOrder =
        new()
        {
            ArenaPosition.TopLeft,
            ArenaPosition.TopMid,
            ArenaPosition.TopRight,
            ArenaPosition.BotLeft,
            ArenaPosition.BotMid,
            ArenaPosition.BotRight,
        };
    private int turnIndex = 0;

    private bool fightInProgress = false;
    private Dictionary<ArenaPosition, Card> cardsOnArena;

    private Card previousCard;

    public void StartFight(Dictionary<ArenaPosition, Card> cardsOnArena)
    {
        if (fightInProgress)
        {
            return;
        }

        fightInProgress = true;
        turnIndex = 0;
        this.cardsOnArena = cardsOnArena;

        PlayNextTurn();
    }

    private void PlayNextTurn()
    {
        UnregisterPreviousCard();
        CleanupDeadCards();

        var nextTurn = FindNextCardInTurn(cardsOnArena);
        if (nextTurn == null)
        {
            FightOver();

            return;
        }

        var (attackingCard, position) = nextTurn.Value;

        var oppositePosition = position.Opposite();
        cardsOnArena.TryGetValue(oppositePosition, out Card targetCard);

        attackingCard.AttackAnimationFinished += CardFinishedAttackAnimation;
        previousCard = attackingCard;

        attackingCard.StartAttack(new(targetCard, this));
    }

    private void FightOver()
    {
        fightInProgress = false;
        EmitSignal(SignalName.FightRoundEnded);
    }

    public void CardAttacksTarget(Card attacker, Card target)
    {
        if (target == null)
        {
            GD.Print("Boom!");
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
