using System.Collections.Generic;
using System.Collections.Immutable;
using Godot;

public class RandomAI : IEnemyAI
{
    private readonly RandomNumberGenerator rng = new();

    public Dictionary<ArenaPosition, Card> GetCardsPlacement(
        ImmutableDictionary<ArenaPosition, Card> cardsOnArena,
        List<Card> myHand,
        List<ArenaPosition> mySquares
    )
    {
        var result = new Dictionary<ArenaPosition, Card>();

        var freeSquares = new List<ArenaPosition>();
        foreach (var square in mySquares)
        {
            if (!cardsOnArena.ContainsKey(square))
            {
                freeSquares.Add(square);
            }
        }

        foreach (var square in freeSquares)
        {
            if (myHand.Count == 0)
            {
                break;
            }

            var index = rng.RandiRange(0, myHand.Count - 1);
            var card = myHand[index];
            result[square] = card;
            myHand.RemoveAt(index);
        }

        return result;
    }
}
