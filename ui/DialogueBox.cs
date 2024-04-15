using Godot;
using System;
using System.Collections.Generic;

public enum WhoIsSpeaking {
	Paju,
	OldWiz,
	Vellamo,
	Kokko,
	Maahinen,
	Ilmatar,
	Kyltti

}

public partial class DialogueBox : Control {
	public List<string> Lines = new();

	[Export]
	public Label Text;

	[Signal]
	public delegate void DialogueFinishedEventHandler();

	[Export]
	public TextureRect Paju;

	[Export]
	public TextureRect OldWiz;

	[Export]
	public TextureRect Vellamo;

	[Export]
	public TextureRect Kokko;

	[Export]
	public TextureRect Ilmatar; 

	[Export]
	public TextureRect Maahinen; 

	[Export]
	public TextureRect Kyltti; 

	public bool IsInProgress;

    public override void _Ready() {
        Visible = false;
		IsInProgress = false;
		Lines.Clear();
    }

    public override void _Process(double delta) {
		if (Input.IsActionJustPressed("Interact") && IsInProgress) {
			NextLine();
		}
    }

    public void Start(WhoIsSpeaking who, string[] lines) {
		Paju.Visible = who == WhoIsSpeaking.Paju;
		OldWiz.Visible = who == WhoIsSpeaking.OldWiz;
		Vellamo.Visible = who == WhoIsSpeaking.Vellamo;
		Kokko.Visible = who == WhoIsSpeaking.Kokko;
		Ilmatar.Visible = who == WhoIsSpeaking.Ilmatar;
		Maahinen.Visible = who == WhoIsSpeaking.Maahinen;
		Kyltti.Visible = who == WhoIsSpeaking.Kyltti;

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
