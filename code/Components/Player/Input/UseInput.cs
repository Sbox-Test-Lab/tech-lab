public class UseInput : InputActionFunction
{
	protected override void OnInputActionActive( string action, InputState state )
	{
		var ray = Scene.Camera.ScreenPixelToRay( Screen.Size / 2 );

		var traceResult = Game.ActiveScene.Trace.Ray( ray, 1024 )
			.WithTag( "interactable" )
			.WithoutTags( "player" )
			.Run();

		if ( !traceResult.Hit )
			return;

		if ( traceResult.GameObject is not GameObject gameObject )
			return;

		IInteractionEvent.PostToGameObject( traceResult.GameObject, x => x.OnInteract( GameObject.Root ) );
	}
}
