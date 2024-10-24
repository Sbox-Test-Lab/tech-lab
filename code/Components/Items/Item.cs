using ItemBuilder.UI;

namespace ItemBuilder;

public class Item : Component, IItemEvent
{
	[Property] public string Name { get; set; }
	[Property] public string Description { get; set; }

	[RequireComponent] public Interactable Interaction { get; set; }

	public IEnumerable<BaseItemAbility> Abilities => Components.GetAll<BaseItemAbility>();

	public ItemWorldInfo ItemInfo { get; set; }

	protected override void OnStart()
	{
		//Interaction.OnInteraction += OnItemInteraction;

		ItemInfo = GameObject.Components.Get<ItemWorldInfo>( FindMode.InChildren );
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( ItemInfo.IsValid() )
		{
			var position = new Vector3( GameObject.GetBounds().Center.x, GameObject.GetBounds().Center.y, GameObject.GetBounds().Center.z + (GameObject.GetBounds().Extents.z + 8.0f) );

			ItemInfo.WorldPosition = position;
		}
	}

	private bool CanActivate( GameObject user )
	{
		//If any item ability cannot be used, don't use any of the item's abilities
		if ( Abilities.Any( x => !x.CanActivate( user ) ) )
			return false;

		return true;
	}

	void IItemEvent.OnItemAdded( Item item )
	{
		if(item != this)
			return;
		if(IsProxy)
			return;

		Log.Info( $"Added {item.Name} to inventory" );
	}

	void IItemEvent.OnItemRemoved( Item item )
	{
		if ( item != this )
			return;

		Log.Info($"Removed {item.Name} from inventory");
	}

	void IItemEvent.OnItemInteraction( Item item, GameObject user )
	{
		if ( item != this )
			return;

		if ( !CanActivate( user ) )
			return;

		foreach ( var ability in Abilities )
		{
			ability.OnActive( user );
		}
	}
}
