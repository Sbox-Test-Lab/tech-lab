public class Hand
{
	public List<Card> Cards { get; private set; }

	public Hand()
	{
		Cards = new List<Card>();
	}

	public void AddCard( Card card )
	{
		Cards.Add( card );
	}

	public int GetValue()
	{
		int value = 0;
		int aces = 0;

		foreach ( var card in Cards )
		{
			if ( card.CardRank == Card.Rank.Ace )
				aces++;
			value += card.GetValue( true );
		}

		// Adjust for aces if value is over 21
		while ( value > 21 && aces > 0 )
		{
			value -= 10;
			aces--;
		}

		return value;
	}

	public bool IsBusted()
	{
		return GetValue() > 21;
	}

	public bool IsBlackjack()
	{
		return Cards.Count == 2 && GetValue() == 21;
	}

	public void Clear()
	{
		Cards.Clear();
	}

	public string GetDisplayString()
	{
		return string.Join( ", ", Cards.Select( c => c.GetDisplayName() ) );
	}
}
