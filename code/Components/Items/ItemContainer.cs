using System;
using System.Text.Json;
using System.Text.Json.Nodes;

using ItemBuilder.UI;

public class ItemContainer : Component
{
	[Property] public int MaxItems { get; set; } = 32;
	[Sync] public NetList<string> Items { get; set; } = new NetList<string>();

	public bool AddItem(Item item)
	{
		var jsonObj = item.GameObject.Serialize();

		Log.Info( $"{jsonObj}" );

		if ( Items.Count > MaxItems )
			return false;

		var json = item.GameObject.Serialize().ToString();

		Items.Add(json);

		IItemEvent.PostToGameObject( item.GameObject, x => x.OnItemAdded() );

		DestroyItem( item.GameObject.Id );

		return true;
	}

	public bool RemoveItem(int index) 
	{
		if(Items.Count < 1)
			return false;

		Items.RemoveAt( index );

		return true;
	}

	public bool RemoveItem(int index, Vector3 position)
	{
		SpawnItem( index, position );

		RemoveItem( index );

		return true;
	}

	private void SpawnItem(int index, Vector3 position)
	{
		var gameObject = GetItemGameObject( index );

		gameObject.Components.GetOrCreate<Rigidbody>().Gravity = true;

		foreach(var component in gameObject.Components.GetAll<BaseItemBehavior>(FindMode.InSelf))
		{
			component.Enabled = component.EnableOnSpawn;
		}

		gameObject.WorldPosition = position;

		gameObject.NetworkSpawn();

		IItemEvent.PostToGameObject( gameObject, x => x.OnItemRemoved() );
	}

	public bool HasEquipableComponent( int index )
	{
		var json = JsonSerializer.Deserialize<JsonObject>( Items[index] );

		if ( json["Components"] is JsonArray components )
		{
			foreach ( var component in components )
			{
				if ( component is JsonObject componentObj &&
					componentObj["__type"]?.ToString() == "Equipable" )
				{
					return true;
				}
			}
		}

		return false;
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

	[Rpc.Broadcast] 
	public void DestroyItem(Guid guid)
	{
		Game.ActiveScene.Directory.FindByGuid( guid )?.Destroy();
	}
}
