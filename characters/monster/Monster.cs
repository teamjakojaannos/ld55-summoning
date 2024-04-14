using Godot;

public partial class Monster : CharacterBody2D {
	[Export]
	public Timer MoveTimer;

	[Export]
	public float MoveSpeed = 32.0f;

	private Vector2 target;
	private bool isMoving = false;

	public override void _Ready() {
		var currentTileX = Mathf.FloorToInt(GlobalPosition.X / 32);
		var currentTileY = Mathf.FloorToInt(GlobalPosition.Y / 32);
		GlobalPosition = new(
			currentTileX * 32.0f + 16,
			currentTileY * 32.0f + 16
		);

		target = GlobalPosition;

		MoveTimer.Timeout += () => {
			var currentTileX = Mathf.FloorToInt(GlobalPosition.X / 32);
			var currentTileY = Mathf.FloorToInt(GlobalPosition.Y / 32);

			var rng = new RandomNumberGenerator();
			var direction = new Vector2[] { Vector2.Down, Vector2.Left, Vector2.Right, Vector2.Up }[rng.RandiRange(0, 3)];

			var targetTileX = currentTileX + direction.X * 2;
			var targetTileY = currentTileY + direction.Y * 2;
			target = new(
				targetTileX * 32.0f + 16,
				targetTileY * 32.0f + 16
			);

			isMoving = true;
		};
	}

    public override void _Process(double delta) {
		var distance = target - GlobalPosition;
        var direction = distance.Normalized();

		Velocity = MoveSpeed * direction;
		bool moveFinished = false;
		if (Velocity.Length() * delta > distance.Length()) {
			Velocity = distance;
			moveFinished = true;
		}

		if (isMoving) {
			MoveAndSlide();
		}

		if (moveFinished) {
			var currentTileX = Mathf.FloorToInt(GlobalPosition.X / 32);
			var currentTileY = Mathf.FloorToInt(GlobalPosition.Y / 32);
			GlobalPosition = new(
				currentTileX * 32.0f + 16,
				currentTileY * 32.0f + 16
			);

			target = GlobalPosition;
			isMoving = false;
		}
    }
}
