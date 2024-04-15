using Godot;

[GlobalClass]
public partial class CardStats : Resource {
	private int _MaxHp = 10;
	[Export]
	public int MaxHp {
		get => _MaxHp;
		set {
			_MaxHp = value;
		}
	}

	private int _CurrentHp = 10;
	[Export]
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

	public CardStats() { }

	public CardStats(int hp, int damage) {
		Damage = damage;
		MaxHp = hp;
		CurrentHp = hp;
	}
}
