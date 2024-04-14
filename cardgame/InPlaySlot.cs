using Godot;
using System;

public partial class InPlaySlot : TextureRect {
	public Card Card;

	public bool IsTaken {
		get => Card != null;
	}

	public bool IsFree {
		get => !IsTaken;
	}
}
