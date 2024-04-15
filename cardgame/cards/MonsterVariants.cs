public enum MonsterVariant {
	Air1,
	Air2,
	Earth1,
	Earth2,
	Fire1,
	Fire2,
	Water1,
	Water2,
}

public static class MonsterVariantExtensions {
	public static Element GetElement(this MonsterVariant monster) => monster switch {
		MonsterVariant.Air1 => Element.Air,
		MonsterVariant.Air2 => Element.Air,
		MonsterVariant.Earth1 => Element.Earth,
		MonsterVariant.Earth2 => Element.Earth,
		MonsterVariant.Fire1 => Element.Fire,
		MonsterVariant.Fire2 => Element.Fire,
		MonsterVariant.Water1 => Element.Water,
		MonsterVariant.Water2 => Element.Water,
		_ => Element.Air,
	};
}
