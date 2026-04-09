using System;
using Sandbox;



public class Perishable : BaseItemBehavior
{
	[BehaviorState] public double DecayTime { get; set; } = 60.0f;

	public override bool CanActivate( GameObject user )
	{
		return true;
	}

	public override void OnActive( GameObject user )
	{
		
	}
}

