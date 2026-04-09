public class BetButton :  Component, IInteractionEvent
{
	[Property] public VideoBlackJack MiniGame;
	

	public void OnInteract( GameObject user )
	{
		
		MiniGame.PlaceBet( 10 );
	}

	
}
