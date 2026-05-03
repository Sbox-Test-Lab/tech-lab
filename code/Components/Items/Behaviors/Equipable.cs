

public class Equipable : BaseItemBehavior
{
	[Property] public HoldTypeResource HoldConfig { get; set; }

	public override bool CanActivate( GameObject user )
	{
		return true;
	}

	public override void OnActive( GameObject user )
	{
		
	}
}
