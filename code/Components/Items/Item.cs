using ItemBuilder.UI;

namespace ItemBuilder;

public class Item : Component
{
	[Property] public string Name { get; set; }
	[Property] public string Description { get; set; }

	[RequireComponent] public Interactable Interaction { get; set; }

	public IEnumerable<BaseItemAbility> Abilities => Components.GetAll<BaseItemAbility>();

	public ItemWorldInfo ItemInfo { get; set; }

	protected override void OnStart()
	{
		Interaction.OnInteraction += OnItemInteraction;

		ItemInfo = GameObject.Components.Get<ItemWorldInfo>( FindMode.InChildren );
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( ItemInfo.IsValid() )
		{
			var position = new Vector3( GameObject.GetBounds().Center.x, GameObject.GetBounds().Center.y, GameObject.GetBounds().Center.z + (GameObject.GetBounds().Extents.z + 8.0f));

			ItemInfo.Transform.Position = position;
		}
	}

	private bool CanActivate( GameObject user )
	{
		//If any item ability cannot be used, don't use any of the item's abilities
		if(Abilities.Any( x => !x.CanActivate( user )))
			return false;
		
		return true;
	}

	public void OnItemInteraction( GameObject user )
	{	
		if(!CanActivate( user)) 
			return;

		foreach(var ability in Abilities )
		{
			ability.OnActive( user );
		}
	}
}
