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
		MonsterVariant.Earth1 => Element.Air,
		MonsterVariant.Earth2 => Element.Air,
		MonsterVariant.Fire1 => Element.Air,
		MonsterVariant.Fire2 => Element.Air,
		MonsterVariant.Water1 => Element.Air,
		MonsterVariant.Water2 => Element.Air,
		_ => Element.Air,
	};
}
