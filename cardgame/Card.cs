using Godot;

public partial class Card : VBoxContainer
{
    private int _MaxHp = 1;

    public int MaxHp
    {
        get => _MaxHp;
        set
        {
            _MaxHp = value;
            UpdateLabels();
        }
    }

    private int _CurrentHp = 1;
    public int CurrentHp
    {
        get => _CurrentHp;
        set
        {
            if (value > MaxHp)
            {
                value = MaxHp;
            }
            if (value < 0)
            {
                value = 0;
            }
            _CurrentHp = value;
            UpdateLabels();
        }
    }
    private int _Damage = 1;
    public int Damage
    {
        get => _Damage;
        set
        {
            _Damage = value;
            UpdateLabels();
        }
    }

    private Label hpLabel;
    private Label dmgLabel;
    private Label numberLabel;

    public override void _Ready()
    {
        hpLabel = GetNode<Label>("Card/HpLabel");
        dmgLabel = GetNode<Label>("Card/DamageLabel");
        numberLabel = GetNode<Label>("NumberKey");
        numberLabel.Visible = false;

        UpdateLabels();
    }

    public void UpdateLabels()
    {
        if (hpLabel == null || dmgLabel == null || numberLabel == null)
        {
            return;
        }

        hpLabel.Text = $"HP: {CurrentHp} / {MaxHp}";
        dmgLabel.Text = $"Damage: {Damage}";
    }

    public void SetNumberLabelText(string text)
    {
        numberLabel.Text = text;
    }

    public void SetNumberLabelVisible(bool visible)
    {
        numberLabel.Visible = visible;
    }
}
