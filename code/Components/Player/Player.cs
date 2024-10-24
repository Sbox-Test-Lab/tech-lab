namespace TestLab;

[Title( "Citizen Player" )]
public partial class Player : Component
{
	[RequireComponent] public PhysicalPlayerController Controller { get; set; }

	private static Player _Local = null;

	public static Player Local
	{
		get
		{
			if ( !_Local.IsValid() )
			{
				_Local = Game.ActiveScene.GetAllComponents<Player>().FirstOrDefault( x => x.Network.IsOwner );
			}

			return _Local;
		}
	}
}
