using Godot;
using System;

public partial class esjonnejaujookulmaa : AnimatedSprite2D
{
	[Export]
	public AnimatedSprite2D jonne;

	[Export]
	public AnimatedSprite2D leikkuri;

	private Vector2 Velocity = Vector2.Left;

    public override void _Ready() {
        jonne.Play("default");
		leikkuri.Play("default");

		if (new RandomNumberGenerator().Randf() < 0.0001f) {
			Visible = true;
		}
    }

	public override void _Process(double delta)
	{
		var x = Position.X;
		x += (float)delta * Velocity.X;
		Velocity *= 1.01f;

		if (x < -200.0f) {
			x += 400.0f;
		}

		Position = new(x, Position.Y);
	}
}
