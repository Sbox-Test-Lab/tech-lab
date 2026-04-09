public class Interactable : BaseItemBehavior, IInteractionEvent
{
	protected override void OnAwake()
	{
		base.OnStart();

		EnableOnSpawn = true;
	}

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
