using ItemBuilder.UI;
using Sandbox;

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

	protected override void OnUpdate()
	{
		base.OnUpdate();

		var worldPanel = GameObject.Components.Get<ItemWorldInfo>( FindMode.EverythingInChildren );

		if ( !worldPanel.IsValid() )
			return;

		worldPanel.Transform.Position = GameObject.GetBounds().Center;
		worldPanel.Transform.Position += new Vector3( 0, 0, 8.0f );
	}
}
