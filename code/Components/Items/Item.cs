using ItemBuilder.UI;

public class Item : Component, IItemEvent
{
	[Property] public string Name { get; set; }
	[Property] public string Description { get; set; }

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

	void IItemEvent.OnItemAdded()
	{
		GameEventFeed.BroadcastGameFeedEvent( "info", $"Added {Name} to inventory" );
	}
	void IItemEvent.OnItemRemoved()
	{
		GameEventFeed.BroadcastGameFeedEvent( "info", $"Removed {Name} from inventory" );
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

	private Texture GenerateThumbnailTexture()
	{
		var scene = new Scene();
		using ( scene.Push() )
		{
			var go = new GameObject();
			var mr = go.AddComponent<ModelRenderer>();
			mr.Model = Model.Load( "models/error.vmdl" );

			var gameobject = new GameObject();
			var camera = gameobject.AddComponent<CameraComponent>();

			var texture = Texture.Create( 128, 128 ).Finish();

			camera.RenderToTexture( texture );

			return texture;
		}
	}

}
