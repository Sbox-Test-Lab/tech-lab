namespace ItemBuilder;

public class Interactable : Component
{
	private Item Item => GameObject.Components.Get<Item>();
	public void Interact(GameObject user)
	{
		IItemEvent.PostToGameObject( GameObject.Root, x => x.OnItemInteraction(Item, user ) );
	}
}
