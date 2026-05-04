using System.Collections.Generic;

/// <summary>
/// Lightweight item definition asset. Behaviors live on the prefab;
/// mutable fields are marked with <see cref="BehaviorStateAttribute"/>.
/// </summary>
[AssetType( Name = "Item Resource", Extension = "item", Category = "Item data" )]
public partial class ItemResource : GameResource
{
	[Property, Group( "Info" )]
	public string Name { get; set; }

	[Property, Group( "Info" ), TextArea]
	public string Description { get; set; }

	/// <summary>
	/// Optional icon for inventory / HUD display without instantiating the prefab.
	/// </summary>
	[Property, Group( "Info" )]
	public Texture Icon { get; set; }

	/// <summary>
	/// The prefab that houses the <see cref="Item"/> component and all behavior components.
	/// </summary>
	[Property, Group( "Prefab" )]
	public PrefabFile PrefabFile { get; set; }

	/// <summary>
	/// How this item is held — grip, rotation offsets, and finger curl.
	/// Generated automatically by the Item Builder tool.
	/// </summary>
	[Property, Group( "Prefab" )]
	public HoldTypeResource HoldType { get; set; }

	/// <summary>
	/// Maximum items per inventory slot. 1 = non-stackable.
	/// </summary>
	[Property, Group( "Inventory" )]
	public int MaxStackSize { get; set; } = 1;

	// ── Runtime registry ──────────────────────────────────────────

	/// <summary>
	/// Returns the icon texture to use as a thumbnail in inventory UI.
	/// </summary>
	public Texture GetThumbnail() => Icon;

	public static IReadOnlyDictionary<string, ItemResource> All => _all;
	private static readonly Dictionary<string, ItemResource> _all = new();

	protected override void PostReload()
	{
		base.PostReload();
		if ( !string.IsNullOrWhiteSpace( ResourcePath ) )
			_all[ResourcePath] = this;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ( !string.IsNullOrWhiteSpace( ResourcePath ) )
			_all.Remove( ResourcePath );
	}
}
