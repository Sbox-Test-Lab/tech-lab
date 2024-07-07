namespace ItemBuilder;

public class Marketable : BaseItemAbility
{
	[Property] public int Price { get; set; } = 0;
	protected override void OnAwake()
	{
		base.OnAwake();

		EnableOnSpawn = false;
	}

	public override bool CanActivate(GameObject user)
	{
		var playerMoney = user.Components.Get<PlayerMoney>();

		Log.Info( $"{playerMoney}" );

		if ( !playerMoney.IsValid() )
			return false;

		if ( !playerMoney.Take( Price ) )
		{
			Log.Info( $"{user} cannot purchase this item" );

			return false;
		}

		return true;
	}

	public override void OnActive( GameObject user )
	{
		var playerMoney = user.Components.Get<PlayerMoney>();

		Log.Info( $"{user} purchased {Item.Name} {user} money: {playerMoney.CurrentMoney}" );
	}
}
