using ItemBuilder.UI;

public class Marketable : BaseItemBehavior
{
	[Property, ItemAbilityProperty] public SoundEvent PurchaseSound { get; set; } 
	[Property, ItemAbilityProperty, BehaviorState] public int Price { get; set; } = 0;

	public Marketable()
	{
		EnableOnRestore = false;
	}

	public override bool CanActivate(GameObject user)
	{
		var playerMoney = user.Components.Get<PlayerMoney>();

		if ( !playerMoney.IsValid() )
		{
			Log.Warning( "Marketable item does not have a valid PlayerMoney component on the user." );

			return false;
		}

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
