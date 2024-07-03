namespace ItemBuilder;

public class PlayerItemDrop : InputActionFunction
{
	protected override void OnInputActionActive( string action, InputState state )
	{
		base.OnInputActionActive( action, state );

		DropItem();
	}

	public void DropItem()
	{
		var inventory = TestLab.Player.Local.Components.Get<PlayerInventory>();

		var ray = Scene.Camera.ScreenPixelToRay( Screen.Size / 2 );

		var traceResult = Scene.Trace.Ray( ray, 512f )
			.IgnoreGameObject( GameObject )
			.Run();

		inventory?.RemoveItem(inventory.Items.Count-1, traceResult.EndPosition);
	}


}

