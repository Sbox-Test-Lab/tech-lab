namespace ItemBuilder;

public partial class PlayerInventory : Component
{
	[RequireComponent] public Player Player { get; set; }
}
