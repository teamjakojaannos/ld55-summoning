using System;
using Godot;

public partial class CardInHand : VBoxContainer
{
    private Label label;

    public override void _Ready()
    {
        label = GetNode<Label>("Label");
    }

    public void SetText(string text)
    {
        label.Text = text;
    }
}
