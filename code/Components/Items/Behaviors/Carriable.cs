public class Carriable : BaseItemBehavior
{
	public override bool CanActivate( GameObject user )
	{
		return true;
	}

	public override void OnActive(GameObject user)
	{
		var inventory = user.Components.Get<PlayerInventory>();

		if(!inventory.IsValid())
		{
			Log.Info( "Could not find PlayerInventory component!" );
			return;
		}

		inventory.AddItem( Item );
	}
}
