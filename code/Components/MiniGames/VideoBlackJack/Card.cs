using Sandbox;

public class Card
{
	public enum Suit { Hearts, Diamonds, Clubs, Spades }
	public enum Rank { Ace = 1, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King }

	public Suit CardSuit { get; set; }
	public Rank CardRank { get; set; }

	public bool IsHidden { get; set; } = false;

	public Card( Suit suit, Rank rank )
	{
		CardSuit = suit;
		CardRank = rank;
	}

	public int GetValue( bool aceAsEleven = true )
	{
		if ( CardRank == Rank.Ace )
			return aceAsEleven ? 11 : 1;
		if ( (int)CardRank > 10 )
			return 10;
		return (int)CardRank;
	}

	public string GetDisplayName()
	{
		return $"{GetRankName()}{GetSuitSymbol()}";
	}

	public string GetRankName()
	{
		var rankName = CardRank switch
		{
			Rank.Ace => "A",
			Rank.Jack => "J",
			Rank.Queen => "Q",
			Rank.King => "K",
			_ => ((int)CardRank).ToString()
		};

		return rankName;
	}

	public string GetSuitSymbol()
	{
		var suitSymbol = CardSuit switch
		{
			Suit.Hearts => "♥",
			Suit.Diamonds => "♦",
			Suit.Clubs => "♣",
			Suit.Spades => "♠",
			_ => ""
		};

		return suitSymbol;
	}

}
