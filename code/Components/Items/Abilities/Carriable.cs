namespace ItemBuilder;

public class Carriable : BaseItemAbility
{
	public delegate void PickupDelegate( GameObject user, Item item );
	public static PickupDelegate OnItemPickup { get; set; }

	[RequireComponent] public Interactable Interaction { get; set; }
	[RequireComponent] public Item Item { get; set; }

	protected override void OnStart()
	{
		base.OnStart();

		Interaction.OnInteraction += OnInteraction;
	}

	public void OnInteraction(GameObject user, GameObject gameObject)
	{
		if ( gameObject != GameObject )
			return;

		OnItemPickup?.Invoke( user, Item );
	}
}
