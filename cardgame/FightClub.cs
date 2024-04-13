using System.Collections.Generic;
using Godot;

public class FightClub
{
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

        var nextTurn = FindNextCardInTurn(cardsOnArena);
        if (nextTurn == null)
        {
            fightInProgress = false;
            return;
        }

        var (attackingCard, position) = nextTurn.Value;

        var oppositePosition = position.Opposite();
        cardsOnArena.TryGetValue(oppositePosition, out Card targetCard);

        attackingCard.AttackAnimationFinished += CardFinishedAttackAnimation;
        previousCard = attackingCard;

        attackingCard.StartAttack(new(targetCard, this));
    }

    public void CardAttacksTarget(Card attacker, Card target)
    {
        if (target == null)
        {
            GD.Print("Boom!");
        }
        else
        {
            GD.Print("Minor boom!");
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
}
