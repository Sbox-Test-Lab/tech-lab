using System;

namespace ItemBuilder;

public abstract class BaseItemAbility : Component
{
	[ItemAbilityProperty] public bool GenerateComponentEditor { get; set; } = false;
	[Property] public bool EnableOnSpawn { get; set; } = false;
	[RequireComponent] public Item Item { get; set; }

	public abstract bool CanActivate( GameObject user );
	public abstract void OnActive( GameObject user );
}

[AttributeUsage( AttributeTargets.Property )]
public sealed class ItemAbilityPropertyAttribute : Attribute
{

}
