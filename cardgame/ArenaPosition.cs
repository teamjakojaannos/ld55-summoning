using System;

public enum ArenaPosition {
    OpponentLeft,
    OpponentMid,
    OpponentRight,
    PlayerLeft,
    PlayerMid,
    PlayerRight,
}

public static class ArenaPositionExtensions {
    public static ArenaPosition Opposite(this ArenaPosition position) =>
        position switch {
            ArenaPosition.OpponentLeft => ArenaPosition.PlayerLeft,
            ArenaPosition.OpponentMid => ArenaPosition.PlayerMid,
            ArenaPosition.OpponentRight => ArenaPosition.PlayerRight,
            ArenaPosition.PlayerLeft => ArenaPosition.OpponentLeft,
            ArenaPosition.PlayerMid => ArenaPosition.OpponentMid,
            ArenaPosition.PlayerRight => ArenaPosition.OpponentRight,
            _
                => throw new NotSupportedException(
                    "The above cases are exhaustive. There be dragons if this is ever reached."
                ),
        };
}
