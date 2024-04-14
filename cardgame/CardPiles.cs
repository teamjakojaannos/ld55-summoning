using Godot;

public partial class CardPiles : Control
{
    private Label labelDrawPile;
    private Label labelDiscardPile;

    public override void _Ready()
    {
        labelDrawPile = GetNode<Label>("LabelDrawPile");
        labelDiscardPile = GetNode<Label>("LabelDiscardPile");

        UpdateLabels();
    }

    public void UpdateLabels()
    {
        labelDrawPile.Text = $"Draw pile: 0";
        labelDiscardPile.Text = $"Discard pile: 0";
    }
}
