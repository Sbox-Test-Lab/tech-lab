

public sealed class TechLabGameManager : GameObjectSystem<TechLabGameManager>, ISceneStartup
{
	public TechLabGameManager(Scene scene) : base (scene)
	{
		
	}

	void ISceneStartup.OnHostInitialize()
	{
		Log.Info("TechLabGameManager.OnSceneStartup");

		var slo = new SceneLoadOptions();
		slo.SetScene( "scenes/itembuilder.scene" );
		slo.ShowLoadingScreen = true;
	}
}
