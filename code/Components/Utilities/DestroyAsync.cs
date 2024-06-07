namespace ItemBuilder;

public class DestroyAsync : Component
{
	public async void Remove()
	{
		await GameTask.Delay( 1 );

		GameObject.Destroy();
	}
}
