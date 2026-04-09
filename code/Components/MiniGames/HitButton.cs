public class HitButton :  Component, IInteractionEvent
{
	[Property] public VideoBlackJack MiniGame;

	public void OnInteract( GameObject user )
	{

		MiniGame.Hit();
	}

	
}
