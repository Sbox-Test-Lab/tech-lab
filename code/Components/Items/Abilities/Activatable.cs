namespace ItemBuilder;

public class Activatable : BaseItemAbility, Component.ITriggerListener
{
	public override bool CanActivate( GameObject user )
	{
		return true;
	}

	public override void OnActive( GameObject user )
	{
		
	}

	void OnTriggerEnter( Collider other )
	{
		Log.Info( "Item Entered Trigger" );
	}

	void OnTriggerExit( Collider other )
	{
		Log.Info( "Item Exited Trigger" );
	}
}

