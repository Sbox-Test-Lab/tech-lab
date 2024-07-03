using TestLab;

namespace ItemBuilder;

public class PlayerInteraction : InputActionFunction
{
	[Property] public float InteractRange { get; set; } = 1024.0f;
	[Property] public float InteractRadius { get; set; } = 4.0f;

	protected override void OnInputActionActive( string action, InputState state )
	{
		base.OnInputActionActive( action, state );

		TryInteract();
	}

	public void TryInteract()
	{
		var ray = Scene.Camera.ScreenPixelToRay( Screen.Size / 2 );

		var traceResult = Game.ActiveScene.Trace.Ray( ray, InteractRange )
			.WithoutTags( "player", "trigger" )
			.Run();
		
		var interactable = traceResult.GameObject?.Components.Get<Interactable>();
		interactable?.Interact( Player.Local.GameObject );
	}

}
