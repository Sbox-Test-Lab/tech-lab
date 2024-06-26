using ItemBuilder.UI;

namespace ItemBuilder;

public class Item : Component
{
	[Property] public string Name { get; set; }
	[Property] public string Description { get; set; }

	protected override void OnStart()
	{
		// TODO: Figure out a better way to update the ui when item name is generated

		var info = GameObject.Components.Get<ItemWorldInfo>( FindMode.EverythingInChildren );

		if ( info.IsValid() )
		{
			info.ItemName = GameObject.Name;
		}
	}
}
