using ItemBuilder.UI;

namespace ItemBuilder;

public class PlayerMenu : InputActionFunction
{
	protected override void OnInputActionActive( string action, InputState state )
	{
		base.OnInputActionActive( action, state );

		OpenInventory();
	}

	public void OpenInventory()
	{
		Inventory.Show();
	}

}
