using ItemBuilder.UI;

namespace ItemBuilder;

public class Marketable : BaseItemAbility
{
	[Property, ItemAbilityProperty] public SoundEvent PurchaseSound { get; set; } 
	[Property, ItemAbilityProperty] public int Price { get; set; } = 0;
	protected override void OnAwake()
	{
		base.OnAwake();

		EnableOnSpawn = false;
	}

	public override bool CanActivate(GameObject user)
	{
		var playerMoney = user.Components.Get<PlayerMoney>();

		if ( !playerMoney.IsValid() )
			return false;

		if ( !playerMoney.HasAmount(Price) )
		{
			GameEventFeed.BroadcastGameFeedEvent( "payment", $"Not have enough money to purchase {Item.Name}" );

			return false;
		}

		return true;
	}

	public override void OnActive( GameObject user )
	{
		var playerMoney = user.Components.Get<PlayerMoney>();
		
		playerMoney.Take( Price );

		Sound.Play( PurchaseSound );

		GameEventFeed.BroadcastGameFeedEvent( "payment", $"Purchased {Item.Name}: {playerMoney.CurrentMoney}" );
	}
}
