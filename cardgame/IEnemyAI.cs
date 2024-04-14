using System.Collections.Generic;
using System.Collections.Immutable;

public interface IEnemyAI
{
    public Dictionary<ArenaPosition, Card> GetCardsPlacement(
        ImmutableDictionary<ArenaPosition, InPlaySlot> cardsOnArena,
        List<Card> myHand,
        List<ArenaPosition> mySquares
    );
}
