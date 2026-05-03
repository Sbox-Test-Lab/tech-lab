using ItemBuilder.UI;

public class ViewInventoryInput : InputActionFunction
{
protected override void OnInputActionActive( string action, InputState state )
{
var player = Player.LocalPlayer();
if ( player is null ) return;

var panel = player.Components.Get<InventoryPanel>( FindMode.EverythingInSelf );
if ( panel is null )
{
    player.Components.Create<InventoryPanel>( true );
}
}
}
