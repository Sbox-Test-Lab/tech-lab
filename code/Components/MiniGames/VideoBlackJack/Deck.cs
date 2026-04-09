using System;

public class Deck
{
	private List<Card> cards;
	private Random random;

	public Deck()
	{
		random = new Random();
		ResetDeck();
	}

	public void ResetDeck()
	{
		cards = new List<Card>();
		foreach ( Card.Suit suit in Enum.GetValues<Card.Suit>() )
		{
			foreach ( Card.Rank rank in Enum.GetValues<Card.Rank>() )
			{
				cards.Add( new Card( suit, rank ) );
			}
		}
		Shuffle();
	}

	public void Shuffle()
	{
		for ( int i = cards.Count - 1; i > 0; i-- )
		{
			int j = random.Next( i + 1 );
			(cards[i], cards[j]) = (cards[j], cards[i]);
		}
	}

	public Card DrawCard(bool hidden = false)
	{
		if ( cards.Count == 0 )
			ResetDeck();

		var card = cards[0];

		card.IsHidden = hidden;

		cards.RemoveAt( 0 );
		return card;
	}
}
