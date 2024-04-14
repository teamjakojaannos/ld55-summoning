using Godot;

public partial class ArenaPositionLabel : TextureRect {
	[Export]
	public ArenaPosition PositionOnArena;

    [Export]
	[ExportCategory("Prewire")]
    public Cardgame Cardgame;

	[Export]
	public Color HighlightedColor;

	[Export]
	public Color DimmedColor;

    public override void _Ready() {
		Modulate = DimmedColor;

        Cardgame.PlayerSelectedCardInHand += () => {
			var positionTaken = Cardgame.IsPositionOnTableTaken(PositionOnArena);
            var useHighlight = !positionTaken;
            var color = useHighlight ? HighlightedColor : DimmedColor;

            Modulate = color;
        };

		Cardgame.PlayerDeselectedCardInHand += () => {
			Modulate = DimmedColor;
		};
    }
}
