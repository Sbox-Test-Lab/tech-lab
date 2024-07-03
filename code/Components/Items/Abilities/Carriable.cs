namespace ItemBuilder;

public class Carriable : BaseItemAbility
{
	protected override void OnAwake()
	{
		base.OnStart();

		EnableOnSpawn = true;
	}

	protected override void OnStart()
	{
		base.OnStart();

		Interaction.OnInteraction += OnItemInteraction;
	}

	public override void OnItemInteraction( GameObject user )
	{
		base.OnItemInteraction( user );

		var inventory = user.Components.Get<PlayerInventory>();
		inventory?.AddItem( Item );
	}
}
