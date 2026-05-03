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
/// 
/// If the selected object already has an <see cref="Item"/> component with a linked
/// <see cref="ItemResource"/>, the widget switches to edit mode and re-saves in place.
/// </summary>
public class ItemResourceWidget : Widget
{
	// ── Item resource (drives the "Item Info" section) ────────────

	private static readonly string[] _skipProperties = ["PrefabFile"];
	private readonly ItemResource _tempResource = new();
	private readonly SerializedObject _resourceSerialized;

	// ── Behavior selection ────────────────────────────────────────

	private readonly Dictionary<TypeDescription, bool> _behaviorToggles = new();
	private readonly Dictionary<TypeDescription, Checkbox> _behaviorCheckboxes = new();

	// ── UI references ─────────────────────────────────────────────

	private Label _selectionLabel;
	private Label _statusLabel;
	private Button _actionButton;

	// ── Selection from EditorTool ─────────────────────────────────

	private GameObject _selectedObject;

	// ── Edit mode ─────────────────────────────────────────────────

	/// <summary>
	/// Non-null when editing an existing item. Holds the live resource to overwrite.
	/// </summary>
	private ItemResource _existingResource;
	private bool IsEditMode => _existingResource is not null;

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

		// ── Behavior toggles ──────────────────────────────────────

		Layout.Add( new Label( "Behaviors" ) { Color = Theme.Blue } );

		foreach ( var typeDesc in TypeLibrary.GetTypes<BaseItemBehavior>().OrderByDescending( x => x.ClassName ) )
		{
			if ( typeDesc.IsAbstract )
				continue;

			var isDefault = typeDesc.TargetType == typeof( Interactable );
			_behaviorToggles[typeDesc] = isDefault;

			var row = Layout.AddRow();
			var checkbox = row.Add( new Checkbox( typeDesc.Title ) );
			checkbox.Value = isDefault;
			_behaviorCheckboxes[typeDesc] = checkbox;

			var capturedDesc = typeDesc;
			checkbox.Toggled += () => _behaviorToggles[capturedDesc] = checkbox.Value;
		}

		Layout.AddSeparator();

		// ── Status ────────────────────────────────────────────────

		_statusLabel = Layout.Add( new Label( "" ) );

		// ── Action button (text changes based on mode) ────────────

		_actionButton = Layout.Add( new Button.Primary( "Generate Item" ) );
		_actionButton.Clicked = OnActionClicked;
	}

	/// <summary>
	/// Called by <see cref="ItemBuilder"/> when the scene selection changes.
	/// Detects whether the selected object is an existing item and switches mode.
	/// </summary>
	public void SetSelectedObject( GameObject go )
	{
		// ── Skip if selection hasn't changed (prevents overwriting user edits) ──
		if ( go == _selectedObject )
			return;

		_existingResource = null;

		ResetFormState();

		if ( go is null )
		{
			_selectionLabel.Text = "Select an object in the scene";
			_selectionLabel.Color = Theme.TextLight;
			_actionButton.Text = "Generate Item";
			_selectedObject = go;
			return;
		}

		// ── Check for existing item ───────────────────────────────

		var existingItem = go.Components.Get<Item>( FindMode.InSelf );
		if ( existingItem is not null && existingItem.Resource is not null )
		{
			_existingResource = existingItem.Resource;

			_selectionLabel.Text = $"✎ Editing: {existingItem.Resource.Name}";
			_selectionLabel.Color = Theme.Yellow;
			_actionButton.Text = "Update Item";

			// Populate the form from the existing resource
			CopyResourceProperties( existingItem.Resource.GetSerialized(), _resourceSerialized );

			// Sync behavior checkboxes with what's on the GameObject
			foreach ( var (typeDesc, checkbox) in _behaviorCheckboxes )
			{
				var hasBehavior = go.Components.Get( typeDesc.TargetType, FindMode.InSelf ) is not null;
				checkbox.Value = hasBehavior;
				_behaviorToggles[typeDesc] = hasBehavior;
			}

			SetStatus( "Editing existing item — changes will overwrite.", Theme.Yellow );
		}
		else
		{
			_selectionLabel.Text = $"✓ {go.Name}";
			_selectionLabel.Color = Theme.Green;
			_actionButton.Text = "Generate Item";

			// Auto-fill name from the object name if empty
			if ( string.IsNullOrWhiteSpace( _tempResource.Name ) )
			{
				if ( _resourceSerialized.TryGetProperty( "Name", out var nameProp ) )
					nameProp.SetValue( go.Name );
			}
		}

		_selectedObject = go;
	}

	/// <summary>
	/// Clear all form fields and behavior toggles back to defaults so stale
	/// values from a previous selection never leak into the next one.
	/// </summary>
	private void ResetFormState()
	{
		CopyResourceProperties( new ItemResource().GetSerialized(), _resourceSerialized );

		foreach ( var (typeDesc, checkbox) in _behaviorCheckboxes )
		{
			var isDefault = typeDesc.TargetType == typeof( Interactable );
			checkbox.Value = isDefault;
			_behaviorToggles[typeDesc] = isDefault;
		}
	}

	/// <summary>
	/// Routes to generate or update based on current mode.
	/// </summary>
	private void OnActionClicked()
	{
		if ( IsEditMode )
			UpdateItem();
		else
			GenerateItem();
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

		// Add selected behaviors
		foreach ( var (typeDesc, enabled) in _behaviorToggles )
		{
			if ( !enabled ) continue;

			if ( go.Components.Get( typeDesc.TargetType, FindMode.InSelf ) is null )
				go.Components.Create( typeDesc );
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

		// ── Save as prefab ────────────────────────────────────────

		var projectRoot = Project.Current.GetRootPath();
		var assetsPath = Path.Combine( projectRoot, "Assets" );

		Log.Info( $"[ItemBuilder] RootPath: {projectRoot}" );
		Log.Info( $"[ItemBuilder] AssetsPath: {assetsPath}" );

		var itemDir = Path.Combine( assetsPath, "items", safeName );
		Directory.CreateDirectory( itemDir );
		var prefabPath = Path.Combine( itemDir, $"{safeName}.prefab" );

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

		var itemPath = Path.Combine( itemDir, $"{safeName}.item" );
		_tempResource.PrefabFile = prefab;
		_tempResource.Icon = GenerateThumbnailTexture( go, assetsPath, safeName );

		var itemAsset = AssetSystem.CreateResource( "item", itemPath );
		itemAsset.SaveToDisk( _tempResource );

		// ── Wire up the back-reference ────────────────────────────

		item.Resource = _tempResource;

		SetStatus( $"✓ Created {safeName}.prefab + {safeName}.item", Theme.Green );

		Log.Info( $"[ItemBuilder] Generated resource at {itemPath}" );
	}

	/// <summary>
	/// Update an existing item's prefab and resource in place.
	/// </summary>
	private void UpdateItem()
	{
		if ( _selectedObject is null || !_selectedObject.IsValid() )
		{
			SetStatus( "Select an object in the scene first.", Theme.Yellow );
			return;
		}

		if ( _existingResource is null )
		{
			SetStatus( "No existing resource to update.", Theme.Red );
			return;
		}

		if ( string.IsNullOrWhiteSpace( _tempResource.Name ) )
		{
			SetStatus( "Name is required.", Theme.Red );
			return;
		}

		var go = _selectedObject;
		var safeName = _tempResource.Name.ToLower().Replace( ' ', '_' );

		// ── Update the Item component ─────────────────────────────

		var item = go.Components.Get<Item>( FindMode.InSelf );
		if ( item is null )
		{
			SetStatus( "Selected object has no Item component.", Theme.Red );
			return;
		}

		item.Name = _tempResource.Name;
		item.Description = _tempResource.Description;
		go.Name = _tempResource.Name;

		// ── Sync behaviors ────────────────────────────────────────

		foreach ( var (typeDesc, enabled) in _behaviorToggles )
		{
			var existing = go.Components.Get( typeDesc.TargetType, FindMode.InSelf );

			if ( enabled )
			{
				if ( existing is null )
					go.Components.Create( typeDesc );
			}
			else if ( existing is not null )
			{
				existing.Destroy();
			}
		}

		// ── Re-save the prefab in place ───────────────────────────

		var projectRoot = Project.Current.GetRootPath();
		var assetsPath = Path.Combine( projectRoot, "Assets" );

		var existingPrefabPath = _existingResource.PrefabFile?.ResourcePath;
		if ( string.IsNullOrWhiteSpace( existingPrefabPath ) )
		{
			SetStatus( "Existing resource has no prefab path.", Theme.Red );
			return;
		}

		var prefabPath = Path.Combine( assetsPath, existingPrefabPath );

		using ( SceneEditorSession.Scope() )
		{
			EditorUtility.Prefabs.ConvertGameObjectToPrefab( go, prefabPath );
		}

		// ── Update the ItemResource and re-save ───────────────────

		CopyResourceProperties( _resourceSerialized, _existingResource.GetSerialized() );
		_existingResource.Icon = GenerateThumbnailTexture( go, assetsPath, safeName );

		var itemPath = Path.Combine( assetsPath, _existingResource.ResourcePath );
		var itemAsset = AssetSystem.FindByPath( itemPath );
		itemAsset?.SaveToDisk( _existingResource );

		item.Resource = _existingResource;

		SetStatus( $"✓ Updated {_existingResource.Name}", Theme.Green );
		Log.Info( $"[ItemBuilder] Updated resource at {_existingResource.ResourcePath}" );
	}

	/// <summary>
	/// Render a thumbnail of the item's model using a temporary editor scene,
	/// save it as a .png alongside the item resource, and return the texture path.
	/// Uses the same pattern as s&box's built-in <see cref="AssetPreview"/> system.
	/// </summary>
	private Texture GenerateThumbnailTexture( GameObject go, string assetsPath, string safeName )
	{
		var modelRenderer = go.Components.Get<ModelRenderer>( FindMode.InSelf );
		if ( modelRenderer?.Model is null )
			return null;

		var model = modelRenderer.Model;

		var scene = Scene.CreateEditorScene();
		using ( scene.Push() )
		{
			// ── Camera ────────────────────────────────────────────
			var camGo = new GameObject( true, "camera" );
			var camera = camGo.AddComponent<CameraComponent>();
			camera.BackgroundColor = Color.Transparent;
			camera.FieldOfView = 30f;
			camera.ZNear = 0.1f;
			camera.ZFar = 15000f;
			camera.WorldRotation = new Angles( 20, 180 + 45, 0 );

			// ── Lighting (matches AssetPreview defaults) ──────────
			var sunGo = new GameObject( true, "sun" );
			var sun = sunGo.AddComponent<DirectionalLight>();
			sun.Shadows = true;
			sun.WorldRotation = new Angles( 50, 45, 0 );
			sun.LightColor = Color.White * 0.6f;

			var ambientGo = new GameObject( true, "ambient" );
			var ambient = ambientGo.AddComponent<AmbientLight>();
			ambient.Color = Color.Cyan * 0.05f;

			var envGo = new GameObject( true, "envmap" );
			var env = envGo.AddComponent<EnvmapProbe>();
			env.Mode = EnvmapProbe.EnvmapProbeMode.CustomTexture;
			env.Texture = Texture.Load( "textures/cubemaps/default2.vtex" );
			env.Bounds = BBox.FromPositionAndSize( Vector3.Zero, 100000 );

			// ── Model ─────────────────────────────────────────────
			var modelGo = new GameObject( true, "model" );
			var renderer = modelGo.AddComponent<ModelRenderer>();
			renderer.Model = model;

			// ── Frame the camera around the model ─────────────────
			var bounds = model.RenderBounds;
			var distance = MathX.SphereCameraDistance( bounds.Size.Length * 0.5f, camera.FieldOfView );
			camera.WorldPosition = bounds.Center + camera.WorldRotation.Forward * -distance;

			// ── Tick the scene so lighting & model are ready ──────
			scene.EditorTick( 0f, 0f );

			// ── Render to bitmap (the proven AssetPreview path) ───
			using var bitmap = new Bitmap( 256, 256 );
			camera.RenderToBitmap( bitmap );

			// ── Save PNG to disk so it becomes a referenceable asset ──
			var thumbDir = Path.Combine( assetsPath, "items", safeName );
			Directory.CreateDirectory( thumbDir );
			var thumbPath = Path.Combine( thumbDir, $"{safeName}_icon.png" );

			var pngBytes = bitmap.ToPng();
			File.WriteAllBytes( thumbPath, pngBytes );

			// ── Load back as a file-backed Texture ────────────────
			var relativePath = Path.GetRelativePath( assetsPath, thumbPath ).Replace( '\\', '/' );
			return Texture.Load( relativePath );
		}
	}

	/// <summary>
	/// Copy all serialized properties between two <see cref="SerializedObject"/> wrappers,
	/// skipping any in <see cref="_skipProperties"/> (e.g. PrefabFile).
	/// Iterates the <paramref name="to"/> side so only actual properties are touched,
	/// and pulls matching values from <paramref name="from"/> via <c>TryGetProperty</c>.
	/// </summary>
	private static void CopyResourceProperties( SerializedObject from, SerializedObject to )
	{
		foreach ( var prop in to )
		{
			if ( !prop.IsProperty )
				continue;

			if ( _skipProperties.Contains( prop.Name ) )
				continue;

			if ( from.TryGetProperty( prop.Name, out var sourceProp ) )
				prop.SetValue( sourceProp.GetValue<object>() );
		}
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
