using Godot;

[GlobalClass]
public partial class CardStats : Resource {
	private int _MaxHp = 10;
	[Export]
	public int MaxHp {
		get => _MaxHp;
		set {
			_MaxHp = value;
			CurrentHp = value;
		}
	}

	private int _CurrentHp = 10;
	public int CurrentHp {
		get => _CurrentHp;
		set {
			if (value > MaxHp) {
				value = MaxHp;
			}
			if (value < 0) {
				value = 0;
			}
			_CurrentHp = value;
		}
	}
	private int _Damage = 1;
	[Export]
	public int Damage {
		get => _Damage;
		set {
			_Damage = value;
		}
	}

	public bool IsDead {
		get => CurrentHp == 0;
	}

	[Export]
	public MonsterVariant MonsterVariant {
		get; set;
	}

	public CardStats() { }

	public CardStats(int hp, int damage) {
		Damage = damage;
		MaxHp = hp;
		CurrentHp = hp;
	}

	public CardStats(int hp, int damage, MonsterVariant monsterVariant) {
		Damage = damage;
		MaxHp = hp;
		CurrentHp = hp;
		MonsterVariant = monsterVariant;
	}

	public CardStats Clone() {
		return new CardStats(MaxHp, Damage, MonsterVariant);
	}
}
