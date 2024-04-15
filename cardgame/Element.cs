using System.Collections.Generic;

public enum Element {
	Air,
	Fire,
	Water,
	Earth,
}

public static class ElementExtensions {

	private static readonly Dictionary<(Element, Element), float> multipliers = new(){
		{(Element.Water, Element.Fire), 2.0f}
	};

	public static float DamageMultiplier(this Element attacker, Element defender) {
		var key = (attacker, defender);

		if (multipliers.ContainsKey(key)) {
			return multipliers[key];
		}

		return 1.0f;
	}
}
