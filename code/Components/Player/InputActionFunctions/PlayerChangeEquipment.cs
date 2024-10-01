using TestLab;

namespace ItemBuilder;

public class PlayerChangeEquipment : InputActionFunction
{
	
	protected override void OnInputActionActive( string action, InputState state )
	{
		base.OnInputActionActive( action, state );

		ChangeEquipment();
	}

	public void ChangeEquipment()
	{
		
	}

}
