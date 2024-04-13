using Godot;
using System;

public partial class WizardCellarIntro : Node2D {
    [Export]
    public AnimationPlayer Animation;

	[Export]
	public Player Player;

    private string[] ritualSteps = new string[] { "intro_idle", "intro_setup_ritual" };
    private int ritualStep = 0;

    public override void _Ready() {
        Animation.Play("intro_idle");
		Player.IsInCinematic = true;
		Player.Visible = false;
    }

	public override void _Process(double delta) {
		if (Input.IsActionJustPressed("move_up")) {
			ritualStep = (ritualStep + 1) % ritualSteps.Length;
			Animation.Play(ritualSteps[ritualStep]);
		}
	}
}
