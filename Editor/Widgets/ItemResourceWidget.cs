using Editor;
using ItemBuilder.UI;
using Sandbox;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TestLab;

/// <summary>
/// Unified item-creation widget. The workflow is:
/// 1. Select a model/GameObject in the scene
/// 2. Fill in item info (Name, Description, MaxStackSize)
/// 3. Toggle which behaviors to include
/// 4. Click "Generate" → creates a .prefab (with Rigidbody, Item, chosen behaviors)
///    and an .item resource that references it, all in one step.
/// </summary>
public class ItemResourceWidget : Widget
{
	// ── Item resource (drives the "Item Info" section) ────────────

	private readonly ItemResource _tempResource = new();
	private readonly SerializedObject _resourceSerialized;

	// ── Behavior selection ────────────────────────────────────────

	private readonly Dictionary<TypeDescription, bool> _behaviorToggles = new();
	private readonly Dictionary<TypeDescription, Component> _behaviorComponents = new();
	private readonly Dictionary<TypeDescription, ControlSheet> _behaviorSheets = new();

	// ── UI references ─────────────────────────────────────────────

	private Label _selectionLabel;
	private Label _statusLabel;

	// ── Selection from EditorTool ─────────────────────────────────

	private GameObject _selectedObject;

	public ItemResourceWidget( Widget parent ) : base( parent, true )
	{
		Layout = Layout.Column();
		Layout.Spacing = 4;
		Layout.Margin = 8;

		// ── Selection display ─────────────────────────────────────

		Layout.Add( new Label( "Selection" ) { Color = Theme.Blue } );
		_selectionLabel = Layout.Add( new Label( "Select an object in the scene" ) { Color = Theme.TextLight } );

		Layout.AddSeparator();

		// ── Item info section (driven by ItemResource properties) ────

		Layout.Add( new Label( "Item Info" ) { Color = Theme.Blue } );

		_resourceSerialized = _tempResource.GetSerialized();

		var resourceSheet = new ControlSheet();
		resourceSheet.AddObject( _resourceSerialized, p => p.Name != "PrefabFile" );

		Layout.Add( resourceSheet );

		Layout.AddSeparator();

		// ── Behavior toggles + property sheets ────────────────────

		Layout.Add( new Label( "Behaviors" ) { Color = Theme.Blue } );

		foreach ( var typeDesc in TypeLibrary.GetTypes<BaseItemBehavior>().OrderByDescending( x => x.ClassName ))
		{
			if ( typeDesc.IsAbstract )
				continue;

			var isDefault = typeDesc.TargetType == typeof( Interactable );
			_behaviorToggles[typeDesc] = isDefault;

			// Create a temporary component instance so we can serialize its properties
			var component = typeDesc.Create<Component>();
			component.Enabled = true;
			_behaviorComponents[typeDesc] = component;

			// Checkbox row
			var row = Layout.AddRow();
			var checkbox = row.Add( new Checkbox( typeDesc.Title ) );
			checkbox.Value = isDefault;

			// Build the property sheet for this behavior (hidden by default)
			var serialized = component.GetSerialized();
			var properties = serialized
				.Where( p => p.Name != "GenerateComponentEditor"
						  && ( p.HasAttribute<BehaviorStateAttribute>()
							|| p.HasAttribute<ItemAbilityPropertyAttribute>() ) )
				.ToArray();

			ControlSheet sheet = null;

			if ( properties.Length > 0 )
			{
				sheet = new ControlSheet();
				sheet.AddGroup( typeDesc.Title, properties );
				sheet.Enabled = isDefault;

				Layout.Add( sheet );
				_behaviorSheets[typeDesc] = sheet;
			}

			// Capture for closure
			var capturedDesc = typeDesc;
			var capturedSheet = sheet;

			checkbox.Toggled += () =>
			{
				_behaviorToggles[capturedDesc] = checkbox.Value;

				if ( capturedSheet is not null )
					capturedSheet.Enabled = checkbox.Value;
			};
		}

		Layout.AddSeparator();

		// ── Status ────────────────────────────────────────────────

		_statusLabel = Layout.Add( new Label( "" ) );

		// ── Generate button ───────────────────────────────────────

		var generateButton = Layout.Add( new Button.Primary( "Generate Item" ) );
		generateButton.Clicked = GenerateItem;
	}

	/// <summary>
	/// Called by <see cref="ItemBuilder"/> when the scene selection changes.
	/// </summary>
	public void SetSelectedObject( GameObject go )
	{
		_selectedObject = go;

		if ( go is not null )
		{
			_selectionLabel.Text = $"✓ {go.Name}";
			_selectionLabel.Color = Theme.Green;

			// Auto-fill name from the object name if empty
			if ( string.IsNullOrWhiteSpace( _tempResource.Name ) )
			{
				_tempResource.Name = go.Name;
			}
		}
		else
		{
			_selectionLabel.Text = "Select an object in the scene";
			_selectionLabel.Color = Theme.TextLight;
		}
	}

	/// <summary>
	/// The main generation flow. Creates a prefab and an .item resource in one step.
	/// </summary>
	private void GenerateItem()
	{
		// ── Validate ──────────────────────────────────────────────

		if ( _selectedObject is null || !_selectedObject.IsValid() )
		{
			SetStatus( "Select an object in the scene first.", Theme.Yellow );
			return;
		}

		if ( string.IsNullOrWhiteSpace( _tempResource.Name ) )
		{
			SetStatus( "Name is required.", Theme.Red );
			return;
		}

		var safeName = _tempResource.Name.ToLower().Replace( ' ', '_' );

		// ── Prepare the GameObject for prefab ─────────────────────

		var go = _selectedObject;

		// Tag it as interactable
		go.Tags.Add( "interactable" );
		go.Name = _tempResource.Name;

		// Add core components every item needs
		go.Components.GetOrCreate<Rigidbody>();

		// Add ModelCollider if it has a model
		var modelRenderer = go.Components.Get<ModelRenderer>( FindMode.InSelf );
		if ( modelRenderer is not null && modelRenderer.Model is not null )
		{
			var collider = go.Components.GetOrCreate<ModelCollider>();
			collider.Model = modelRenderer.Model;
		}

		// Add the Item component
		var item = go.Components.GetOrCreate<Item>();
		item.Name = _tempResource.Name;
		item.Description = _tempResource.Description;

		// Add selected behaviors with configured property values
		foreach ( var (typeDesc, enabled) in _behaviorToggles )
		{
			if ( !enabled ) continue;

			// Only add if not already present
			if ( go.Components.Get( typeDesc.TargetType, FindMode.InSelf ) is not null )
				continue;

			var component = go.Components.Create( typeDesc );

			// Copy configured property values from the temp component
			if ( _behaviorComponents.TryGetValue( typeDesc, out var tempComponent ) )
			{
				foreach ( var prop in typeDesc.Properties )
				{
					if ( !prop.HasAttribute<BehaviorStateAttribute>()
					  && !prop.HasAttribute<ItemAbilityPropertyAttribute>() )
						continue;

					prop.SetValue( component, prop.GetValue( tempComponent ) );
				}
			}
		}

		var uiGameObject = go.Scene.CreateObject(true);
		uiGameObject.Name = "UI";
		uiGameObject.SetParent( go );

		var worldPanel = uiGameObject.Components.Create<WorldPanel>();
		worldPanel.Enabled = true;
		worldPanel.WorldPosition = go.GetBounds().Center;
		worldPanel.WorldPosition += new Vector3( 0, 0, 8.0f );

		worldPanel.PanelSize = new Vector2( 512f, 128f );

		worldPanel.LookAtCamera = true;

		var itemInfoPanel = uiGameObject.Components.Create<ItemWorldInfo>();

		//go.SetParent( selectedGameObject.Parent );

		// ── Save as prefab ────────────────────────────────────────

		var projectRoot = Project.Current.GetRootPath();
		var assetsPath = Path.Combine( projectRoot, "Assets" );

		Log.Info( $"[ItemBuilder] RootPath: {projectRoot}" );
		Log.Info( $"[ItemBuilder] AssetsPath: {assetsPath}" );

		var prefabDir = Path.Combine( assetsPath, "prefabs", "items" );
		Directory.CreateDirectory( prefabDir );
		var prefabPath = Path.Combine( prefabDir, $"{safeName}.prefab" );

		Log.Info( $"[ItemBuilder] PrefabPath: {prefabPath}" );

		// Let the engine handle prefab creation — just hand it the prepared GO
		using ( SceneEditorSession.Scope() )
		{
			EditorUtility.Prefabs.ConvertGameObjectToPrefab( go, prefabPath );
		}

		// Load the created prefab asset to verify it exists
		var asset = AssetSystem.FindByPath( prefabPath );

		if ( asset is null )
		{
			SetStatus( "Failed to create prefab.", Theme.Red );
			return;
		}

		// Convert absolute path to resource path (relative to assets folder)
		var resourcePath = Path.GetRelativePath( assetsPath, prefabPath ).Replace( '\\', '/' );

		// Load the actual PrefabFile resource
		var prefab = PrefabFile.Load( resourcePath );

		if ( prefab is null )
		{
			SetStatus( "Failed to load prefab resource.", Theme.Red );
			return;
		}

		Log.Info( $"[ItemBuilder] Prefab created: {prefab.ResourcePath}" );

		// ── Create the ItemResource ───────────────────────────────

		var itemDir = Path.Combine( assetsPath, "resources", "items" );
		Directory.CreateDirectory( itemDir );

		var itemPath = Path.Combine( itemDir, $"{safeName}.item" );

		_tempResource.PrefabFile = prefab;

		var itemAsset = AssetSystem.CreateResource( "item", itemPath );
		itemAsset.SaveToDisk( _tempResource );

		// ── Wire up the back-reference ────────────────────────────

		item.Resource = _tempResource;

		SetStatus( $"✓ Created {safeName}.prefab + {safeName}.item", Theme.Green );

		Log.Info( $"[ItemBuilder] Generated resource at {itemPath}" );
	}

	private void SetStatus( string message, Color color )
	{
		if ( _statusLabel is not null )
		{
			_statusLabel.Text = message;
			_statusLabel.Color = color;
		}
	}
}
