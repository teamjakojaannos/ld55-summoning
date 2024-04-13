using System;

public enum ArenaPosition
{
    TopLeft,
    TopMid,
    TopRight,
    BotLeft,
    BotMid,
    BotRight,
}

public static class ArenaPositionExtensions
{
    public static ArenaPosition Opposite(this ArenaPosition position) =>
        position switch
        {
            ArenaPosition.TopLeft => ArenaPosition.BotLeft,
            ArenaPosition.TopMid => ArenaPosition.BotMid,
            ArenaPosition.TopRight => ArenaPosition.BotRight,
            ArenaPosition.BotLeft => ArenaPosition.TopLeft,
            ArenaPosition.BotMid => ArenaPosition.TopMid,
            ArenaPosition.BotRight => ArenaPosition.TopRight,
            _
                => throw new NotSupportedException(
                    "The above cases are exhaustive. There be dragons if this is ever reached."
                ),
        };
}
