using ItemBuilder.UI;

public class Item : Component, IItemEvent
{
	[Property] public string Name { get; set; }
	[Property] public string Description { get; set; }

	/// <summary>
	/// The <see cref="ItemResource"/> this item was created from.
	/// Set by <see cref="ItemFactory"/> on instantiation.
	/// </summary>
	[Property] public ItemResource Resource { get; set; }

	public Texture Thumbnail { get; set; }
	public IEnumerable<BaseItemBehavior> Abilities => Components.GetAll<BaseItemBehavior>();

	public ItemWorldInfo ItemInfo { get; set; }

	protected override void OnStart()
	{
		ItemInfo = GameObject.Components.Get<ItemWorldInfo>( FindMode.InChildren );
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( ItemInfo.IsValid() )
		{
			var bounds = GameObject.GetBounds();
			var position = new Vector3( bounds.Center.x, bounds.Center.y, bounds.Center.z + (bounds.Extents.z + 8.0f) );

			ItemInfo.WorldPosition = position;
		}
	}

	private bool CanActivate( GameObject user )
	{
		//If any item ability cannot be used, don't use any of the item's abilities
		if ( Abilities.Any( x => !x.CanActivate( user ) ) )
		{
			Log.Info( "Cannot activate item because at least one ability cannot be activated." );
			
			return false;
		}

		return true;
	}

	void IItemEvent.OnItemAdded()
	{
		GameEventFeed.ShowGameFeedEvent( "info", $"Added {Name} to inventory" );
	}
	void IItemEvent.OnItemRemoved()
	{
		GameEventFeed.ShowGameFeedEvent( "info", $"Removed {Name} from inventory" );
	}

	void IItemEvent.OnItemInteraction(GameObject user)
	{
		if ( !CanActivate( user ) )
			return;

		foreach ( var ability in Abilities )
		{
			ability.OnActive( user );
		}
	}
}
