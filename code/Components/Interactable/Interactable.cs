namespace ItemBuilder;

public class Interactable : Component, Component.IPressable
{
	private Item Item => GameObject.Components.Get<Item>();
	public void Interact(GameObject user)
	{
		IItemEvent.PostToGameObject( GameObject.Root, x => x.OnItemInteraction(Item, user ) );
	}

	bool IPressable.Press( IPressable.Event e )
	{
		Log.Info( "Pressed" );

		Interact( e.Source.GameObject );

		return false;
	}

	bool IPressable.CanPress( Sandbox.Component.IPressable.Event e ) => true;
}
