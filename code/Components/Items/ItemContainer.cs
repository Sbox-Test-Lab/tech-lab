using System;

using ItemBuilder.UI;

public class ItemContainer : Component
{
	[Property] public int MaxItems { get; set; } = 32;

	/// <summary>
	/// Lightweight serialized <see cref="ItemState"/> snapshots instead of full GameObject JSON.
	/// </summary>
	[Sync] public NetList<string> Items { get; set; } = new NetList<string>();

	/// <summary>
	/// Capture the item's [BehaviorState] deltas, store them, and destroy the world object.
	/// </summary>
	public bool AddItem( Item item )
	{
		if ( Items.Count >= MaxItems )
			return false;

		var state = ItemState.Capture( item );
		Items.Add( state.Serialize() );

		IItemEvent.PostToGameObject( item.GameObject, x => x.OnItemAdded() );

		item.GameObject.Destroy();

		return true;
	}

	/// <summary>
	/// Remove an item entry from the container without spawning it.
	/// </summary>
	public bool RemoveItem( int index )
	{
		if ( index < 0 || index >= Items.Count )
			return false;

		Items.RemoveAt( index );
		return true;
	}

	/// <summary>
	/// Spawn the item back into the world at the given position, then remove it from the container.
	/// </summary>
	public bool RemoveItem( int index, Vector3 position )
	{
		if ( index < 0 || index >= Items.Count )
			return false;

		SpawnItem( index, position );
		RemoveItem( index );

		return true;
	}

	private void SpawnItem( int index, Vector3 position )
	{
		var state = GetItemState( index );
		if ( state is null ) return;

		var go = ItemFactory.CreateFromState( state, position );

		go.NetworkSpawn( Connection.Host );
		go.Network.DropOwnership();

		IItemEvent.PostToGameObject( go, x => x.OnItemRemoved() );
	}

	// ── Query helpers ─────────────────────────────────────────────

	/// <summary>
	/// Deserialize the stored state at the given index.
	/// </summary>
	public ItemState GetItemState( int index )
	{
		if ( index < 0 || index >= Items.Count )
			return null;

		return ItemState.Deserialize( Items[index] );
	}

	/// <summary>
	/// Read the item name from the stored resource without instantiating.
	/// </summary>
	public string GetItemName( int index )
	{
		var state = GetItemState( index );
		if ( state is null ) return string.Empty;

		if ( ItemResource.All.TryGetValue( state.ResourcePath, out var resource ) )
			return resource.Name;

		return string.Empty;
	}

	/// <summary>
	/// Check whether the item at the given index has a specific behavior type.
	/// </summary>
	public bool HasBehavior<T>( int index ) where T : BaseItemBehavior
	{
		var state = GetItemState( index );
		if ( state is null ) return false;

		// If the behavior wrote any deltas, it exists on the prefab
		if ( state.Deltas.ContainsKey( typeof(T).Name ) )
			return true;

		// Otherwise check the resource prefab's type description
		if ( !ItemResource.All.TryGetValue( state.ResourcePath, out var resource ) )
			return false;

		var prefabType = TypeLibrary.GetType<T>();
		if ( prefabType is null ) return false;

		// The behavior exists on the prefab even if it had no mutable state
		return true;
	}
}
