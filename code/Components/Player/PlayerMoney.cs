namespace ItemBuilder;

public class PlayerMoney : Component
{
	[Property] public int StartingAmount { get; set; } = 1500;
	[Property] public int MaxAmount { get; set; } = 1000000000;

	[Sync] public int CurrentMoney { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();

		CurrentMoney = StartingAmount;
	}

	public bool Give(int amount)
	{
		if(!CanGive(amount)) 
			return false;

		CurrentMoney += amount;

		return true;
	}

	public bool Take(int amount)
	{
		if(!HasAmount(amount))
			return false;

		CurrentMoney -= amount;

		return true;
	}

	public bool CanGive(int amount)
	{
		var money = CurrentMoney + amount;

		if ( money > MaxAmount )
			return false;

		return true;
	}

	public bool HasAmount(int amount)
	{
		var money = CurrentMoney - amount;

		if ( money < 0 )
			return false;

		return true;
	}
}
