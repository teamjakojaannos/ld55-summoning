using Godot;
using System;
using System.Collections.Generic;

public enum WhoIsSpeaking {
	Paju,
	OldWiz,
}

public partial class DialogueBox : HBoxContainer {
	public List<string> Lines = new();

	[Export]
	public Label Text;

	[Signal]
	public delegate void DialogueFinishedEventHandler();

	[Export]
	public TextureRect Paju;

	[Export]
	public TextureRect OldWiz;

	public bool IsInProgress;

    public override void _Ready() {
        Visible = false;
		IsInProgress = false;
		Lines.Clear();
    }

    public override void _Process(double delta) {
		if (Input.IsActionJustPressed("move_up") && IsInProgress) {
			NextLine();
		}
    }

    public void Start(WhoIsSpeaking who, string[] lines) {
		Paju.Visible = who == WhoIsSpeaking.Paju;
		OldWiz.Visible = who == WhoIsSpeaking.OldWiz;

        Lines = new List<string>(lines);
		Visible = true;
		IsInProgress = true;
		NextLine();
    }

	public void NextLine() {
		if (Lines.Count == 0) {
			Visible = false;
			IsInProgress = false;
			EmitSignal("DialogueFinished");
			return;
		}

		Text.Text = Lines[0];
		Lines.RemoveAt(0);
	}
}
