public class Interactable : BaseItemAbility, IInteractionEvent
{
	public void OnInteract(GameObject user)
	{
		IItemEvent.PostToGameObject( GameObject.Root, x => x.OnItemInteraction(Item, user ) );
	}

	public override bool CanActivate( GameObject user )
	{
		return true;
	}

	public override void OnActive( GameObject user )
	{

	}
}
