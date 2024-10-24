using System.Text.Json.Nodes;
using System.Text.Json;

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

	protected override void OnUpdate()
	{
		base.OnUpdate();


		// Handle Scroll Wheel Input
		var wheel = Input.MouseWheel;

		if ( Input.Pressed( "NextSlot" ) ) wheel.y = -1;
		if ( Input.Pressed( "PrevSlot" ) ) wheel.y = 1;

		if ( wheel.y == 0f ) return;

		// Get the Next Avaliable Equipment Item

		// Assign Item to Current Slot

		// Switch to Equipment Item
		
	}
}
