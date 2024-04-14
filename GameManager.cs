using Godot;

public partial class GameManager : Node2D
{
	public Player Player;

	[Export]
	public PackedScene PlayerTemplate;

    public override void _EnterTree() {
        var playerNode = GetTree().Root.FindChild("Player", true, false);
		if (playerNode is Player player) {
			Player = player;
		} else {
			Player = PlayerTemplate.Instantiate<Player>();

			var startNode = GetTree().Root.FindChild("PlayerStart", true, false);
			if (startNode is Marker2D startMarker) {
				var newParent = startMarker.GetParent();

				newParent.AddChild(Player);
				Player.GlobalPosition = startMarker.GlobalPosition;
			} else {
				GD.PushError("Could not find PlayerStart");
			}
		}
    }
}
