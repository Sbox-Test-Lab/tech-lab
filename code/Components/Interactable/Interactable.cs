namespace ItemBuilder;

public class Interactable : Component
{
	public delegate void InteractionDelegate( GameObject user, GameObject gameObject );
	[Property, Category("Actions")] public InteractionDelegate OnInteraction { get; set; }

	public void Interact(GameObject user)
	{
		OnInteraction?.Invoke( user, GameObject );
	}
}
