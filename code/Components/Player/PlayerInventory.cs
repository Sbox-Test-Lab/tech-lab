using System.Text.Json.Nodes;
using System.Text.Json;
using System;

namespace ItemBuilder;

public partial class PlayerInventory : ItemContainer
{
	public int CurrentItemIndex { get; set; } = 0;
	public void EquipItem()
	{
		var jsonObject = JsonSerializer.Deserialize<JsonObject>( Items.ElementAt( CurrentItemIndex ) );

		var gameObject = new GameObject();
		gameObject.Deserialize( jsonObject );
	}
}
