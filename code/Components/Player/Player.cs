

[Title( "Player" )]
public partial class Player : Component, PlayerController.IEvents
{
	[RequireComponent] public PlayerController Controller { get; set; }

	[Property] public GameObject Body { get; set; }

	public static Player LocalPlayer()
	{
		return Game.ActiveScene.GetAllComponents<Player>().Where(x => !x.IsProxy ).FirstOrDefault();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		
		OnControl();
	}

	private void OnControl()
	{
		
	}
}
