using Godot;

public class MonsterDrops {
	public CardStats commonDrop;
	public CardStats rareDrop;
	public CardStats legendaryDrop;

	public const int legendaryDropChance = 25;
	public const int rareDropChance = 50;

	private RandomNumberGenerator rng = new();

	public MonsterDrops(CardStats commonDrop, CardStats rareDrop, CardStats legendaryDrop) {
		this.commonDrop = commonDrop;
		this.rareDrop = rareDrop;
		this.legendaryDrop = legendaryDrop;
	}

	public CardStats GenerateDrop() {
		var r = rng.RandiRange(0, 100);
		if (r < legendaryDropChance) {
			return legendaryDrop;
		}

		if (r < (legendaryDropChance + rareDropChance)) {
			return rareDrop;
		}

		return commonDrop;
	}
}
