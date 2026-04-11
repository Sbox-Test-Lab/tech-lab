using System;

public static class ItemFactory
{
	/// <summary>
	/// Create a live item from an <see cref="ItemResource"/>.
	/// The prefab already contains <see cref="Item"/> and all behavior components.
	/// </summary>
	public static GameObject CreateFromResource( ItemResource resource, Vector3 position = default )
	{
		if ( resource is null )
			throw new ArgumentNullException( nameof( resource ) );

		var go = SceneUtility.GetPrefabScene( resource.PrefabFile ).Clone( position );

		var item = go.Components.GetOrCreate<Item>();
		item.Resource = resource;
		item.Name = resource.Name;
		item.Description = resource.Description;

		return go;
	}

	/// <summary>
	/// Recreate an item from an <see cref="ItemState"/> snapshot.
	/// Instantiates the prefab, then applies the captured [BehaviorState] deltas.
	/// </summary>
	public static GameObject CreateFromState( ItemState state, Vector3 position = default )
	{
		if ( state is null )
			throw new ArgumentNullException( nameof( state ) );

		var resource = ResourceLibrary.Get<ItemResource>( state.ResourcePath );
		if ( resource is null )
			throw new InvalidOperationException( $"ItemResource not found: {state.ResourcePath}" );

		var go = CreateFromResource( resource, position );

		state.Restore( go );

		go.Components.GetOrCreate<Rigidbody>().Gravity = true;

		foreach ( var behavior in go.Components.GetAll<BaseItemBehavior>( FindMode.InSelf ) )
		{
			behavior.Enabled = behavior.EnableOnRestore;
		}

		return go;
	}
}
