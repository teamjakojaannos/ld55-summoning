using Godot;

public partial class HpBar : Control
{
    private Label label;

    private int _CurrentHp = 100;
    public int CurrentHp
    {
        get => _CurrentHp;
        set
        {
            if (value < 0)
            {
                value = 0;
            }

            if (value > MaxHp)
            {
                value = MaxHp;
            }

            _CurrentHp = value;

            UpdateLabel();
        }
    }

    private int _MaxHp = 100;
    public int MaxHp
    {
        get => _MaxHp;
        set
        {
            _MaxHp = value;
            if (CurrentHp > MaxHp)
            {
                CurrentHp = MaxHp;
            }
            UpdateLabel();
        }
    }

    public override void _Ready()
    {
        label = GetNode<Label>("Label");
        UpdateLabel();
    }

    public void UpdateLabel()
    {
        label.Text = $"HP {CurrentHp}";
    }
}
