using System;
using System.Text.Json;
using System.Text.Json.Nodes;

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
		if ( Items.Count < 1)
			return false;

		SpawnItem( index, position );

		Items.RemoveAt( index );

		return true;
	}

	public virtual bool CanUpdateItem()
	{
		return Items.Count <= MaxItems && Items.Count >= 0;
	}

	private void SpawnItem(int index, Vector3 position)
	{
		var jsonObject = JsonSerializer.Deserialize<JsonObject>( Items[index] );

		var gameObject = new GameObject();
		gameObject.Deserialize( jsonObject );

		gameObject.Components.GetOrCreate<Rigidbody>().Gravity = true;

		foreach(var component in gameObject.Components.GetAll<BaseItemAbility>(FindMode.InSelf))
		{
			component.Enabled = component.EnableOnSpawn;
		}

		gameObject.Transform.Position = position;

		gameObject.NetworkSpawn( Connection.Host );
	}

	[Broadcast] 
	public void DestroyItem(Guid guid)
	{
		Game.ActiveScene.Directory.FindByGuid( guid )?.Destroy();
	}
}
