public class DealButton :  Component, IInteractionEvent
{
	[Property] public VideoBlackJack MiniGame;

	public void OnInteract( GameObject user )
	{
		MiniGame.DealCards();
	}

	
}
