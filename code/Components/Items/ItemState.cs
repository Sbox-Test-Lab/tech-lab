using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

/// <summary>
/// Lightweight snapshot of an item's mutable state. Stores the resource path
/// plus only the properties marked with <see cref="BehaviorStateAttribute"/>
/// on each behavior — no full GameObject serialization.
/// </summary>
public sealed class ItemState
{
	/// <summary>Resource path used to look up the <see cref="ItemResource"/> and its prefab.</summary>
	public string ResourcePath { get; set; } = string.Empty;

	/// <summary>All behavior type names present on the item at capture time.</summary>
	public List<string> Behaviors { get; set; } = new();

	/// <summary>BehaviorTypeName → { PropertyName → JSON value }.</summary>
	public Dictionary<string, Dictionary<string, string>> Deltas { get; set; } = new();

	/// <summary>
	/// Capture the current [BehaviorState] property values from a live item.
	/// </summary>
	public static ItemState Capture( Item item )
	{
		var state = new ItemState
		{
			ResourcePath = item.Resource?.ResourcePath ?? string.Empty
		};

		foreach ( var behavior in item.Abilities )
		{
			var typeDesc = TypeLibrary.GetType( behavior.GetType() );
			if ( typeDesc is null ) continue;

			state.Behaviors.Add( typeDesc.Name );

			var dict = new Dictionary<string, string>();
			foreach ( var prop in typeDesc.Properties )
			{
				if ( !prop.HasAttribute<BehaviorStateAttribute>() ) continue;

				try
				{
					var val = prop.GetValue( behavior );
					dict[prop.Name] = JsonSerializer.Serialize( val );
				}
				catch { /* skip unsupported types */ }
			}

			if ( dict.Count > 0 )
				state.Deltas[typeDesc.Name] = dict;
		}

		return state;
	}

	/// <summary>
	/// Apply captured deltas back onto a freshly-instantiated prefab.
	/// </summary>
	public void Restore( GameObject go )
	{
		var behaviors = go.Components.GetAll<BaseItemBehavior>( FindMode.InSelf );

		foreach ( var (typeName, props) in Deltas )
		{
			var behavior = behaviors.FirstOrDefault( b => b.GetType().Name == typeName );
			if ( behavior is null ) continue;

			var typeDesc = TypeLibrary.GetType( behavior.GetType() );
			if ( typeDesc is null ) continue;

			foreach ( var (propName, json) in props )
			{
				var prop = typeDesc.GetProperty( propName );
				if ( prop is null ) continue;

				try
				{
					var value = JsonSerializer.Deserialize( json, prop.PropertyType );
					prop.SetValue( behavior, value );
				}
				catch { /* skip incompatible values */ }
			}
		}
	}

	/// <summary>
	/// Serialize the entire state to a single JSON string (for NetList storage).
	/// </summary>
	public string Serialize() => JsonSerializer.Serialize( this );

	/// <summary>
	/// Deserialize from a JSON string.
	/// </summary>
	public static ItemState Deserialize( string json )
	{
		try { return JsonSerializer.Deserialize<ItemState>( json ); }
		catch { return null; }
	}
}
