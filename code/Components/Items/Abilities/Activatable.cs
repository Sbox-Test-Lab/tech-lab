namespace ItemBuilder;

public class Activatable : BaseItemAbility
{
	protected override void OnEnabled()
	{
		base.OnEnabled();

		GameObject.Root.Tags.Set( "activatable", true );
	}
	public override bool CanActivate( GameObject user )
	{
		return true;
	}

	public override void OnActive( GameObject user )
	{

	}

	


}

