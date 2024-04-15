using System.Collections.Generic;

public class CardDecks {
	public static List<CardStats> PlayerDeck() {
		var cards = new List<CardStats>();
		for (int i = 0; i < 8; i++) {
			cards.Add(new(i + 1, i + 1));
		}

		return cards;
	}

	public static List<CardStats> EnemyDeck() {
		var list = new List<CardStats> {
			new(12, 7),
			new(34, 8),
			new(56, 9),
			new(78, 1),
			new(99, 2),
			new(87, 3),
			new(65, 2),
			new(43, 1)
		};

		return list;
	}
}
