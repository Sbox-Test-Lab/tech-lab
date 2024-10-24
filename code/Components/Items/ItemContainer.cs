using System;
using System.Text.Json;
using System.Text.Json.Nodes;

using ItemBuilder.UI;

namespace ItemBuilder;

public class ItemContainer : Component
{
	[Property] public int MaxItems { get; set; } = 32;
	[Sync] public NetList<string> Items { get; set; } = new NetList<string>();

	public bool AddItem(Item item)
	{
		if ( Items.Count > MaxItems )
			return false;

		var json = item.GameObject.Serialize().ToString();

		Items.Add(json);

		DestroyItem( item.GameObject.Id );

		IItemEvent.PostToGameObject(item.GameObject.Root, x => x.OnItemAdded( item ) );

		GameEventFeed.BroadcastGameFeedEvent( "info", $"Added {item.Name} from inventory: {Items.Count}" );

		return true;
	}

	public bool RemoveItem(int index) 
	{
		if(Items.Count < 1)
			return false;

		Items.RemoveAt( index );

		GameEventFeed.BroadcastGameFeedEvent( "info", $"Removed {GetItemName( index )} from inventory: {Items.Count}" );

		return true;
	}

	public bool RemoveItem(int index, Vector3 position)
	{
		if ( Items.Count < 1)
			return false;

		SpawnItem( index, position );

		var json = JsonSerializer.Deserialize<JsonObject>( Items[index] );

		Log.Info( $"{json}" );

		//IItemEvent.PostToGameObject( GetItemGameObject(index).Root, x => x.OnItemAdded( item ) );

		Items.RemoveAt( index );



		return true;
	}

	public virtual bool CanUpdateItem()
	{
		return Items.Count <= MaxItems && Items.Count >= 0;
	}

	private void SpawnItem(int index, Vector3 position)
	{
		var gameObject = GetItemGameObject( index );

		gameObject.Components.GetOrCreate<Rigidbody>().Gravity = true;

		foreach(var component in gameObject.Components.GetAll<BaseItemAbility>(FindMode.InSelf))
		{
			component.Enabled = component.EnableOnSpawn;
		}

		gameObject.WorldPosition = position;

		gameObject.NetworkSpawn( Connection.Host );
	}

	public string GetItemName(int index)
	{
		return JsonSerializer.Deserialize<JsonObject>( Items[index] )["Name"].ToString();
	}

	public GameObject GetItemGameObject(int index)
	{
		var json = JsonSerializer.Deserialize<JsonObject>( Items[index] );

		var gameObject = new GameObject();
		gameObject.Deserialize(json);

		return gameObject;
	}

	[Broadcast] 
	public void DestroyItem(Guid guid)
	{
		Game.ActiveScene.Directory.FindByGuid( guid )?.Destroy();
	}
}
