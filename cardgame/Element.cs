using System.Collections.Generic;

public enum Element {
	Air,
	Fire,
	Water,
	Earth,
}

public static class ElementExtensions {

	private static readonly Dictionary<(Element, Element), float> multipliers = new(){
		{(Element.Air, Element.Air), 1.5f },
		{(Element.Air, Element.Earth), 1.0f },
		{(Element.Air, Element.Fire), 1.5f },
		{(Element.Air, Element.Water), 1.5f },

		{(Element.Earth, Element.Air), 1.0f },
		{(Element.Earth, Element.Earth), 0.75f },
		{(Element.Earth, Element.Fire), 0.75f },
		{(Element.Earth, Element.Water), 0.75f },

		{(Element.Fire, Element.Air), 1.5f },
		{(Element.Fire, Element.Earth), 0.75f },
		{(Element.Fire, Element.Fire), 1.0f },
		{(Element.Fire, Element.Water), 0.5f },

		{(Element.Water, Element.Air), 1.5f },
		{(Element.Water, Element.Earth), 0.75f },
		{(Element.Water, Element.Fire), 2.0f },
		{(Element.Water, Element.Water), 1.0f },
	};

	public static float DamageMultiplier(this Element attacker, Element defender) {
		var key = (attacker, defender);

		if (multipliers.ContainsKey(key)) {
			return multipliers[key];
		}

		return 1.0f;
	}
}
