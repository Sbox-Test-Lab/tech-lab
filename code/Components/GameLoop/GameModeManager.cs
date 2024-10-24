using Sandbox.Events;
using TestLab;

public partial class GameModeManager : Component, IGameEventHandler<GamemodeInitializedEvent>
{
	void IGameEventHandler<GamemodeInitializedEvent>.OnGameEvent( GamemodeInitializedEvent eventArgs )
	{
		Log.Info( $"Gamemode initialized: {eventArgs.Title}" );
	}
}
