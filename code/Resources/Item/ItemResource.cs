namespace ItemBuilder;

[GameResource( "Item Resource", "item", "Item data" )]
public class ItemResource : GameResource
{
	public string Name { get; set; }
	public string Description { get; set; }
}
