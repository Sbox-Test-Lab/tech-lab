

public interface IItemEvent : ISceneEvent<IItemEvent>
{
	void OnItemAdded();
	void OnItemRemoved();
	void OnItemInteraction(GameObject user );
}
