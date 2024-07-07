namespace ItemBuilder;

public abstract class BaseItemAbility : Component
{
	[Property] public bool EnableOnSpawn { get; set; } = false;
	[RequireComponent] public Item Item { get; set; }

	public virtual bool CanActivate(GameObject user)
	{
		return false;
	}

	public virtual void OnActive(GameObject user)
	{

	}
}
