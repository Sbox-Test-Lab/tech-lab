namespace ItemBuilder;

public partial class PlayerCamera : Component
{
	[RequireComponent] public CameraComponent Camera { get; set; }

	private PlayerController PlayerController { get; set; }

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if(PlayerController.IsValid())
		{
			PlayerController = Player.Local.Controller;

			return;
		}
	}
}
