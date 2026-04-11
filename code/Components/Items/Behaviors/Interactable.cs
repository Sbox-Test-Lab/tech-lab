public class Interactable : BaseItemBehavior, IInteractionEvent
{
	public void OnInteract(GameObject user)
	{
		IItemEvent.PostToGameObject( GameObject, x => x.OnItemInteraction(user) );
	}

	public override bool CanActivate( GameObject user )
	{
		return true;
	}

	public override void OnActive( GameObject user )
	{

	}
}
