namespace ItemBuilder;

public class Carriable : BaseItemAbility
{
	protected override void OnAwake()
	{
		base.OnStart();

		EnableOnSpawn = true;
	}

	public override bool CanActivate( GameObject user )
	{
		return true;
	}

	public override void OnActive(GameObject user)
	{
		var inventory = user.Components.Get<PlayerInventory>();
		inventory?.AddItem( Item );
	}
}
