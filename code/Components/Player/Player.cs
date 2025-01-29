namespace TestLab;

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
		Log.Info( "Updating" );
		OnControl();
	}

	private void OnControl()
	{
		if(Input.Pressed("use"))
		{
			Log.Info( "Use pressed" );
		}
	}

	Component PlayerController.IEvents.GetUsableComponent( GameObject go )
	{
        Log.Info( "Pressed" );

        return default;
	}

    void PlayerController.IEvents.StartPressing( Sandbox.Component target )
    {
        Log.Info( "Pressed" );
    }

    void PlayerController.IEvents.StopPressing( Sandbox.Component target )
    {
        
    }


}
