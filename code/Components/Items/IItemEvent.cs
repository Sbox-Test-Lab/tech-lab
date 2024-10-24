namespace ItemBuilder;

public interface IItemEvent : ISceneEvent<IItemEvent>
{
	void OnItemAdded( Item item );
	void OnItemRemoved( Item item );
	void OnItemInteraction( Item item, GameObject user );

}
