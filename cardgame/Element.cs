using System.Collections.Generic;

public enum Element {
	Air,
	Fire,
	Water,
	Earth,
}

public static class ElementExtensions {

	public static int TurnOrder(this Element element) => element switch {
		Element.Fire => 4,
		Element.Earth => 3,
		Element.Water => 2,
		Element.Air => 1,
		_ => 0
	};
}
