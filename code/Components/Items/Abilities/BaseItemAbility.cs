namespace ItemBuilder;

public abstract class BaseItemAbility : Component
{
	[Property] public bool EnableOnSpawn { get; set; } = false;
	[RequireComponent] public Item Item { get; set; }

	public abstract bool CanActivate( GameObject user );
	public abstract void OnActive( GameObject user );
}
