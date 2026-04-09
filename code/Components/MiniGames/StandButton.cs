public class StandButton :  Component, IInteractionEvent
{
	[Property] public VideoBlackJack MiniGame;

	public void OnInteract( GameObject user )
	{
		MiniGame.Stand();
	}

	
}
