public class DropItemInput : InputActionFunction
{
	[RequireComponent] PlayerInventory Inventory { get; set; }
	protected override void OnInputActionActive( string action, InputState state )
	{
		var ray = Scene.Camera.ScreenPixelToRay( Screen.Size / 2 );

		var traceResult = Game.ActiveScene.Trace.Ray( ray, 320 )
			.WithTag( "interactable" )
			.WithoutTags( "player" )
			.Run();

		Inventory.RemoveItem( Inventory.Items.Count - 1, traceResult.EndPosition);
	}
}
