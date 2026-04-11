using System;

public abstract class BaseItemBehavior : Component
{
	[ItemAbilityProperty] public bool GenerateComponentEditor { get; set; } = false;
	/// <summary>
	/// Whether this behavior should be re-enabled when the item is restored
	/// from inventory. Defaults to <c>true</c>; set to <c>false</c> for
	/// one-time behaviors like <see cref="Marketable"/>.
	/// </summary>
	[Property] public bool EnableOnRestore { get; set; } = true;
	[RequireComponent] public Item Item { get; set; }

	public abstract bool CanActivate( GameObject user );
	public abstract void OnActive( GameObject user );

	/// <summary>
	/// Called by s&box CodeGenerator when a property with [BehaviorState]
	/// is assigned. The wrapper provides a WrappedPropertySet{T} which
	/// lets us call the original setter and then mark the owning
	/// GameObject as dirty for later capture.
	/// </summary>
	internal void OnBehaviorStateSet<T>( WrappedPropertySet<T> p )
	{
		// Execute the original setter
		p.Setter( p.Value );

		// Mark the owning GameObject as dirty so the capture pass knows it changed.
		//ItemStateTracker.MarkDirty( this.GameObject );
	}
}

[AttributeUsage( AttributeTargets.Property )]
public sealed class ItemAbilityPropertyAttribute : Attribute
{

}

/// <summary>
/// Use on properties you want code-gen to wrap. The s&box code generator will
/// replace the setter so `OnBehaviorStateSet` on the instance runs whenever the
/// property is assigned.
/// </summary>
[AttributeUsage( AttributeTargets.Property )]
[CodeGenerator( CodeGeneratorFlags.WrapPropertySet | CodeGeneratorFlags.Instance, "OnBehaviorStateSet" )]
public sealed class BehaviorStateAttribute : Attribute { }
