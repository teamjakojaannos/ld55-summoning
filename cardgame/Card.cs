using Godot;

public partial class Card : Control
{
    [Signal]
    public delegate void AttackAnimationFinishedEventHandler();

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

    public bool IsDead
    {
        get => CurrentHp == 0;
    }
    private Label hpLabel;
    private Label dmgLabel;
    private Label numberLabel;

    private TextureRect image;

    private bool highlighted = false;

    private AnimationPlayer animation;

    private string attackAnimation = "attack_animation";
    private string attackAnimationUpward = "attack_animation_upward";

    private AttackInfo attackInfo;

    public bool IsPlayersCard = false;

    private Vector2 oldPosition;
    private Vector2 targetPosition;

    private float movementTime = 1.0f;
    private float moveProgress = 0.0f;

    public override void _Ready()
    {
        hpLabel = GetNode<Label>("Sprite/HpLabel");
        dmgLabel = GetNode<Label>("Sprite/DamageLabel");
        numberLabel = GetNode<Label>("NumberKey");
        numberLabel.Visible = false;
        image = GetNode<TextureRect>("Sprite");
        animation = GetNode<AnimationPlayer>("AnimationPlayer");

        animation.AnimationFinished += (name) =>
        {
            if (name == attackAnimation || name == attackAnimationUpward)
            {
                EmitSignal(SignalName.AttackAnimationFinished);
            }
        };

        oldPosition = Position;
        targetPosition = Position;

        UpdateLabels();
    }

    public override void _Process(double delta)
    {
        moveProgress += (float)delta;
        var t = Mathf.Clamp(moveProgress / movementTime, 0.0f, 1.0f);

        Position = oldPosition.Lerp(targetPosition, t);
    }

    public void UpdateLabels()
    {
        if (hpLabel == null || dmgLabel == null || numberLabel == null)
        {
            return;
        }

        hpLabel.Text = $"H {CurrentHp}";
        dmgLabel.Text = $"D {Damage}";
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

    public void StartAttack(AttackInfo info)
    {
        attackInfo = info;
        var animationName = IsPlayersCard ? attackAnimationUpward : attackAnimation;
        animation.Play(animationName);
    }

    public void CardAttacks()
    {
        if (attackInfo == null)
        {
            return;
        }

        attackInfo.fightManager.CardAttacksTarget(this, attackInfo.target, IsPlayersCard);
        attackInfo = null;
    }

    public void PlayHurtAnimation()
    {
        animation.Play("take_damage");
    }

    public void PlayDieAnimation()
    {
        animation.Play("die");
    }

    public void MoveInstantlyTo(Vector2 position)
    {
        Position = position;
        targetPosition = Position;
        oldPosition = Position;
        moveProgress = 1.0f;
    }

    public void StartMovingTo(Vector2 target, float movementTime)
    {
        targetPosition = target;
        oldPosition = Position;
        this.movementTime = movementTime;
        moveProgress = 0.0f;
    }

    public void StopMovement()
    {
        moveProgress = 1.0f;
        Position = targetPosition;
    }
}
