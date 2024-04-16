using Godot;
using System;

public partial class ASD : TextureRect
{
	[Export]
	public Color HoverColor;

	[Export]
	public Color DefaultColor;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		MouseEntered += () => {
			Modulate = HoverColor;
		};

		MouseExited += () => {
			Modulate = DefaultColor;
		};
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
