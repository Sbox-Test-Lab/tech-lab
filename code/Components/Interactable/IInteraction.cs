public interface IInteractionEvent : ISceneEvent<IInteractionEvent>
{
	void OnInteract( GameObject user );
}
