namespace ItemBuilder;

public abstract class BaseItemAbility : Component
{
	[Property] public bool EnableOnSpawn { get; set; } = false;
	[RequireComponent] public Interactable Interaction { get; set; }
	[RequireComponent] public Item Item { get; set; }

	public virtual void OnItemInteraction( GameObject user )
	{
	
	}
}
