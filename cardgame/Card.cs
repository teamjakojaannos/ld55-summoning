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

    private TextureRect image;

    private bool highlighted = false;

    public override void _Ready()
    {
        hpLabel = GetNode<Label>("Card/HpLabel");
        dmgLabel = GetNode<Label>("Card/DamageLabel");
        numberLabel = GetNode<Label>("NumberKey");
        numberLabel.Visible = false;
        image = GetNode<TextureRect>("Card");

        UpdateLabels();
    }

    public void UpdateLabels()
    {
        if (hpLabel == null || dmgLabel == null || numberLabel == null)
        {
            return;
        }

        hpLabel.Text = $"HP: {CurrentHp} / {MaxHp}";
        dmgLabel.Text = $"DMG: {Damage}";
    }

    public void SetHighlighted(bool highlighted)
    {
        var noChange = this.highlighted == highlighted;
        if (noChange)
        {
            return;
        }

        var highlightOffset = new Vector2(0, -10.0f);

        this.highlighted = highlighted;
        if (highlighted)
        {
            // add highlight
            image.Position += highlightOffset;
        }
        else
        {
            // remove highlight
            image.Position -= highlightOffset;
        }
    }

    public void SetNumberLabelColor(Color color)
    {
        numberLabel.LabelSettings.FontColor = color;
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
