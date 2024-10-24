using Sandbox.Events;

namespace TestLab;

public record GamemodeInitializedEvent(string Title) : IGameEvent;

public sealed partial class GameMode : Component, Component.INetworkListener
{
	[Property] string Title { get; set; }
	protected override void OnStart()
	{
		Scene.Dispatch(new GamemodeInitializedEvent(Title));

		base.OnStart();
	}
}
