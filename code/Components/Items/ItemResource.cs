[GameResource("Item Definition", "item", "Describes an item")]
public class ItemResource : GameResource
{
	public string Name { get; set; }
	public string Description { get; set; }
	List<Component> Behaviors { get; set; }
}
