﻿[Title( "Player Clothing" )]
public sealed class PlayerClothing : Component, Component.INetworkSpawn
{
	[Property]
	public SkinnedModelRenderer BodyRenderer { get; set; }

	public void OnNetworkSpawn( Connection owner )
	{
		var clothing = new ClothingContainer();
		clothing.Deserialize( owner.GetUserData( "avatar" ) );
		clothing.Apply( BodyRenderer );
	}
}
