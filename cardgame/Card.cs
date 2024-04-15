using Godot;

public partial class Card : Control {
	[Signal]
	public delegate void AttackAnimationFinishedEventHandler();

	private CardStats stats;

	public int MaxHp {
		get => stats.MaxHp;
		set {
			stats.MaxHp = value;
			UpdateLabels();
		}
	}

	public int CurrentHp {
		get => stats?.CurrentHp ?? 1;
		set {
			stats.CurrentHp = value;
			UpdateLabels();
		}
	}
	public int Damage {
		get => stats?.Damage ?? 1;
		set {
			stats.Damage = value;
			UpdateLabels();
		}
	}

	public bool IsDead {
		get => stats.IsDead;
	}
	private Label nameLabel;
	private Label hpLabel;
	private Label dmgLabel;

	private TextureRect image;
	private TextureRect elementSprite;
	private TextureRect creatureSprite;

	private bool highlighted = false;

	private AnimationPlayer animation;

	private string attackAnimation = "attack_animation";
	private string attackAnimationUpward = "attack_animation_upward";

	private AttackInfo attackInfo;

	public bool IsPlayersCard = false;

	private Vector2 targetPosition;
	private Vector2 targetScale = Vector2.One;

	private float posLerpFactor = 10.0f;

	[Export]
	[ExportCategory("Highlight")]
	public float HighlightZoomFactor = 2.0f;

	[Export]
	public float HighlightZoomInSpeed = 20.0f;

	[Export]
	public float RemoveHighlightZoomOutSpeed = 50.0f;

	public override void _Ready() {
		hpLabel = GetNode<Label>("Sprite/HpLabel");
		dmgLabel = GetNode<Label>("Sprite/DamageLabel");
		image = GetNode<TextureRect>("Sprite");
		animation = GetNode<AnimationPlayer>("AnimationPlayer");
		nameLabel = GetNode<Label>("Sprite/ÖllinNimi");
		elementSprite = GetNode<TextureRect>("Sprite/Elememtpi");
		creatureSprite = GetNode<TextureRect>("Sprite/Ölli");

		animation.AnimationFinished += (name) => {
			if (name == attackAnimation || name == attackAnimationUpward) {
				EmitSignal(SignalName.AttackAnimationFinished);
			}
		};

		MouseEntered += () => {
			Highlight();
		};

		MouseExited += () => {
			RemoveHighlight();
		};

		targetPosition = Vector2.Zero;

		UpdateLabels();
	}

	public override void _Process(double delta) {
		Position = Position.Lerp(targetPosition, (float)delta * posLerpFactor);
		Scale = Scale.Lerp(targetScale, (float)delta * HighlightZoomInSpeed);
	}

	public void UpdateLabels() {
		if (hpLabel == null || dmgLabel == null) {
			return;
		}

		hpLabel.Text = $"H {CurrentHp}";
		dmgLabel.Text = $"D {Damage}";
	}

	public void SetHighlighted(bool highlighted) {
		var noChange = this.highlighted == highlighted;
		if (noChange) {
			return;
		}

		this.highlighted = highlighted;
		if (highlighted) {
			Highlight();
		} else {
			RemoveHighlight();
		}
	}

	public void Highlight() {
		targetScale = HighlightZoomFactor * Vector2.One;
		ZIndex = 1;
	}

	public void RemoveHighlight() {
		targetScale = Vector2.One;
		ZIndex = 0;
	}

	public void StartAttack(AttackInfo info) {
		attackInfo = info;
		var animationName = IsPlayersCard ? attackAnimationUpward : attackAnimation;
		animation.Play(animationName);
	}

	public void CardAttacks() {
		if (attackInfo == null) {
			return;
		}

		attackInfo.fightManager.CardAttacksTarget(this, attackInfo.target, IsPlayersCard);
		attackInfo = null;
	}

	public void PlayHurtAnimation() {
		animation.Play("take_damage");
	}

	public void PlayDieAnimation() {
		animation.Play("die");
	}

	public void MoveInstantlyTo(Vector2 position) {
		GlobalPosition = position;
		targetPosition = position;
		posLerpFactor = 99999f;
	}

	public void StartMovingTo(Vector2 target, float moveSpeed) {
		targetPosition = target - GlobalPosition;
		posLerpFactor = moveSpeed;
	}

	public void StopMovement() {
		posLerpFactor = 0.0f;
		Position = Vector2.Zero;
	}

	internal void MoveToNewParent(Vector2 oldPosition, float cardMoveSpeed) {
		GlobalPosition = oldPosition;
		targetPosition = Vector2.Zero;
		posLerpFactor = cardMoveSpeed;
	}

	public void SetInfoVisible(bool visible) {
		nameLabel.Visible = visible;
		hpLabel.Visible = visible;
		dmgLabel.Visible = visible;
		elementSprite.Visible = visible;
		creatureSprite.Visible = visible;
	}

	public void SetStats(CardStats stats) {
		this.stats = stats;
		UpdateLabels();
	}
}
