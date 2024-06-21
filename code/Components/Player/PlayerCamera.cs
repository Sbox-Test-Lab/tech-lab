namespace ItemBuilder;

public partial class PlayerCamera : Component
{
	[RequireComponent] public CameraComponent Camera { get; set; }

	private PlayerController PlayerController { get; set; }

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if(PlayerController is null)
		{
			PlayerController = Player.Local.Controller;

			return;
		}
		
		var cameraPosition = PlayerController.Eye.Transform.Position;
		var cameraRotation = PlayerController.EyeAngles.ToRotation();
		
		var cameraOffset = cameraRotation * PlayerController.CameraOffset;

		var cameraForward = cameraRotation.Forward;

		var cameraTrace = Scene.Trace.Ray( cameraPosition + cameraOffset, (cameraPosition + cameraOffset) - (cameraForward * PlayerController.CameraDistance) )
			.WithoutTags( "player", "trigger" )
			.Run();

		cameraPosition = cameraTrace.Hit ? cameraTrace.HitPosition + cameraTrace.Normal : cameraTrace.EndPosition;

		Camera.Transform.Position = cameraPosition;
		Camera.Transform.Rotation = cameraRotation;
	}
}
