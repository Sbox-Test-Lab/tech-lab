using ItemBuilder.UI;
using Microsoft.VisualBasic;

namespace ItemBuilder;

public class Item : Component
{
	[Property] public string Name { get; set; }
	[Property] public string Description { get; set; }

	[RequireComponent] public Interactable Interaction { get; set; }

	public IEnumerable<BaseItemAbility> Abilities => Components.GetAll<BaseItemAbility>();

	protected override void OnStart()
	{
		Interaction.OnInteraction += OnItemInteraction;

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

	public void OnItemInteraction( GameObject user )
	{
		//If any item ability cannot be used, don't use any of the item's abilities
		foreach( var ability in Abilities )
		{
			if ( !ability.CanActivate(user) )
				return;
		}

		foreach(var ability in Abilities )
		{
			ability.OnActive( user );
		}
	}
}
