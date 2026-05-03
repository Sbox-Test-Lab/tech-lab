using Editor;
using Sandbox;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EditorApp = Editor.Application;

namespace TestLab;

/// <summary>
/// Interactive editor for creating/editing <see cref="HoldTypeResource"/> assets.
/// Spawns a citizen preview with the selected item, provides real-time sliders
/// for grip offset, rotation, hold type, and per-finger curl.
/// </summary>
public class HoldTypeEditorWidget : Widget
{
	// ── Resource being edited ─────────────────────────────────────
	private HoldTypeResource _holdType = new();
	private SerializedObject _holdTypeSerialized;

	// ── Item to preview ───────────────────────────────────────────
	private ItemResource _selectedItem;

	// ── Preview scene ─────────────────────────────────────────────
	private Scene _previewScene;
	private GameObject _citizenGo;
	private SkinnedModelRenderer _citizenRenderer;
	private CameraComponent _camera;
	private GameObject _itemPreviewGo;

	// ── UI references ─────────────────────────────────────────────
	private Label _statusLabel;
	private Label _itemLabel;
	private ControlSheet _holdTypeSheet;
	private SceneRenderingWidget _renderWidget;

	// ── Orbit camera state ────────────────────────────────────────
	private float _orbitYaw = -40f;
	private float _orbitPitch = 15f;
	private float _zoomScale = 1f;
	private float _pendingScroll;
	private Vector2 _lastCursor;
	private bool _isOrbitDragging;
	private bool _isZoomDragging;

	// ── Existing resource for edit mode ───────────────────────────
	private HoldTypeResource _existingResource;
	private bool IsEditMode => _existingResource is not null;

	public HoldTypeEditorWidget( Widget parent ) : base( parent, true )
	{
		Layout = Layout.Column();
		Layout.Spacing = 4;
		Layout.Margin = 8;

		// ── Top bar: item selection ────────────────────────────────
		_itemLabel = Layout.Add( new Label( "No item selected" ) { Color = Theme.TextLight } );

		var itemRow = Layout.AddRow();
		var selectItemButton = itemRow.Add( new Button( "Select Item..." ) );
		selectItemButton.Clicked = OnSelectItem;

		var loadHoldButton = itemRow.Add( new Button( "Load Existing..." ) );
		loadHoldButton.Clicked = OnLoadExisting;

		Layout.AddSeparator();

		// ── Main split: preview left, properties right ─────────────
		var splitRow = Layout.AddRow();
		splitRow.Spacing = 4;

		// Left: 3D preview + zoom controls
		SetupPreviewScene();

		var previewColumn = new Widget( this );
		previewColumn.Layout = Layout.Column();
		previewColumn.Layout.Spacing = 2;

		_renderWidget = new SceneRenderingWidget( previewColumn );
		_renderWidget.Scene = _previewScene;
		_renderWidget.Camera = _camera;
		_renderWidget.MinimumWidth = 300;
		_renderWidget.MinimumHeight = 400;
		_renderWidget.SetSizeMode( SizeMode.CanGrow, SizeMode.CanGrow );
		_renderWidget.OnPreFrame += OnPreFrame;
		previewColumn.Layout.Add( _renderWidget, 1 );

		// Zoom controls beneath preview
		var zoomRow = previewColumn.Layout.AddRow();
		zoomRow.Spacing = 2;
		zoomRow.Add( new Label( "Zoom:" ) { Color = Theme.TextLight } );
		var zoomOutBtn = zoomRow.Add( new Button( "−" ) );
		zoomOutBtn.FixedWidth = 28;
		zoomOutBtn.Clicked = () => _zoomScale = MathX.Clamp( _zoomScale * 1.2f, 0.1f, 5f );
		var zoomInBtn = zoomRow.Add( new Button( "+" ) );
		zoomInBtn.FixedWidth = 28;
		zoomInBtn.Clicked = () => _zoomScale = MathX.Clamp( _zoomScale * 0.8f, 0.1f, 5f );
		var zoomResetBtn = zoomRow.Add( new Button( "Reset" ) );
		zoomResetBtn.Clicked = () => _zoomScale = 1f;

		splitRow.Add( previewColumn, 1 );

		// Right: scrollable properties panel
		var rightPanel = new Widget( this );
		rightPanel.Layout = Layout.Column();
		rightPanel.Layout.Spacing = 4;
		rightPanel.MinimumWidth = 480;

		var propsScroll = new ScrollArea( rightPanel );

		var propsCanvas = new Widget();
		propsCanvas.Layout = Layout.Column();
		propsCanvas.Layout.Spacing = 4;
		propsCanvas.Layout.Margin = 4;

		propsCanvas.Layout.Add( new Label( "Hold Configuration" ) { Color = Theme.Blue } );

		_holdTypeSerialized = _holdType.GetSerialized();
		_holdTypeSheet = new ControlSheet();
		_holdTypeSheet.AddObject( _holdTypeSerialized );
		propsCanvas.Layout.Add( _holdTypeSheet );

		_holdTypeSerialized.OnPropertyChanged += OnHoldPropertyChanged;

		propsScroll.Canvas = propsCanvas;
		rightPanel.Layout.Add( propsScroll, 1 );

		rightPanel.Layout.AddSeparator();

		_statusLabel = rightPanel.Layout.Add( new Label( "" ) );
		var saveButton = rightPanel.Layout.Add( new Button.Primary( "Save Hold Type" ) );
		saveButton.Clicked = OnSave;

		splitRow.Add( rightPanel, 0 );
	}

	/// <summary>
	/// Poll system cursor to drive orbit and zoom.
	/// Left-drag = orbit. Right-drag up/down = zoom in/out.
	/// SceneRenderingWidget swallows all mouse events so we poll directly.
	/// </summary>
	private void PollOrbit()
	{
		if ( !_renderWidget.IsValid() ) return;

		var cursor = EditorApp.CursorPosition;
		var screenRect = _renderWidget.ScreenRect;
		var inside = screenRect.IsInside( cursor );
		var leftDown = EditorApp.MouseButtons.HasFlag( MouseButtons.Left );
		var middleDown = EditorApp.MouseButtons.HasFlag( MouseButtons.Middle );

		// Left-drag = orbit
		if ( inside && leftDown )
		{
			if ( _isOrbitDragging )
			{
				var delta = _lastCursor - cursor;
				_orbitYaw += delta.x * 0.4f;
				_orbitPitch = MathX.Clamp( _orbitPitch + delta.y * 0.4f, -80f, 80f );
			}
			_isOrbitDragging = true;
		}
		else
		{
			_isOrbitDragging = false;
		}

		// Middle-drag up/down = zoom
		if ( inside && middleDown )
		{
			if ( _isZoomDragging )
			{
				var dy = _lastCursor.y - cursor.y;
				_zoomScale = MathX.Clamp( _zoomScale * (1f - dy * 0.005f), 0.1f, 5f );
			}
			_isZoomDragging = true;
		}
		else
		{
			_isZoomDragging = false;
		}

		_lastCursor = cursor;

		// Wheel fallback (fires only when cursor is NOT directly over SceneRenderingWidget)
		if ( _pendingScroll != 0f )
		{
			_zoomScale = MathX.Clamp( _zoomScale * (1f - _pendingScroll * 0.1f), 0.1f, 5f );
			_pendingScroll = 0f;
		}
	}

	protected override void OnWheel( WheelEvent e )
	{
		if ( _renderWidget.IsValid() && _renderWidget.ScreenRect.IsInside( EditorApp.CursorPosition ) )
			_pendingScroll += e.Delta;
		base.OnWheel( e );
	}

	/// <summary>
	/// Set up the preview scene with a citizen model, camera, and lighting.
	/// </summary>
	private void SetupPreviewScene()
	{
		_previewScene = Scene.CreateEditorScene();

		using ( _previewScene.Push() )
		{
			// Camera
			var camGo = new GameObject( true, "camera" );
			_camera = camGo.AddComponent<CameraComponent>();
			_camera.BackgroundColor = Theme.ControlBackground;
			_camera.FieldOfView = 30f;
			_camera.ZNear = 0.1f;
			_camera.ZFar = 15000f;

			// Lighting
			var sunGo = new GameObject( true, "sun" );
			var sun = sunGo.AddComponent<DirectionalLight>();
			sun.Shadows = true;
			sun.WorldRotation = new Angles( 50, 45, 0 );
			sun.LightColor = Color.White * 0.6f;

			var ambientGo = new GameObject( true, "ambient" );
			var ambient = ambientGo.AddComponent<AmbientLight>();
			ambient.Color = Color.White * 0.1f;

			var envGo = new GameObject( true, "envmap" );
			var env = envGo.AddComponent<EnvmapProbe>();
			env.Mode = EnvmapProbe.EnvmapProbeMode.CustomTexture;
			env.Texture = Texture.Load( "textures/cubemaps/default2.vtex" );
			env.Bounds = BBox.FromPositionAndSize( Vector3.Zero, 100000 );

			// Citizen model
			_citizenGo = new GameObject( true, "citizen" );
			_citizenRenderer = _citizenGo.AddComponent<SkinnedModelRenderer>();
			_citizenRenderer.Model = Model.Load( "models/citizen/citizen.vmdl" );
			_citizenRenderer.CreateBoneObjects = true;

			// Tick once so the model initializes its skeleton
			_previewScene.EditorTick( 0f, 0.1f );

			// Frame camera on citizen
			UpdateCamera();
		}
	}

	/// <summary>
	/// Called every frame before the preview renders — updates the citizen pose.
	/// </summary>
	private void OnPreFrame()
	{
		if ( _previewScene is null || !_citizenGo.IsValid() ) return;

		PollOrbit();

		using ( _previewScene.Push() )
		{
			// Keep the citizen grounded so the animgraph doesn't collapse the skeleton
			_citizenRenderer.Set( "b_grounded", true );
			_citizenRenderer.Set( "move_groundspeed", 0f );

			UpdatePose();

			// Tick the scene so the animgraph evaluates with our parameters
			_previewScene.EditorTick( RealTime.Now, RealTime.Delta );

			// Cache finger bones once after the first tick (GetBoneObject modifies internal collection)
			if ( _fingerBones is null )
				_fingerBones = CacheBonesFromRenderer( _citizenRenderer );

			// Apply bone overrides AFTER the tick so the animgraph doesn't overwrite them
			ApplyFingerPose( _fingerBones, _holdType );

			UpdateCamera();
		}
	}

	/// <summary>
	/// Frame the camera on the citizen model, looking at the upper body.
	/// </summary>
	private void UpdateCamera()
	{
		if ( !_camera.IsValid() || !_citizenRenderer.IsValid() ) return;

		var bounds = _citizenRenderer.Bounds;
		var center = bounds.Center + Vector3.Up * 10f;
		var distance = MathX.SphereCameraDistance( bounds.Size.Length * 0.5f, _camera.FieldOfView );

		var dir = Rotation.From( _orbitPitch, _orbitYaw, 0 ).Forward;

		_camera.WorldPosition = center - dir * distance * 0.8f * _zoomScale;
		_camera.WorldRotation = Rotation.LookAt( dir );
	}

	/// <summary>
	/// Called when any hold type property changes — updates the preview in real-time.
	/// </summary>
	private void OnHoldPropertyChanged( SerializedProperty prop )
	{
		// Preview updates on next OnPreFrame
	}

	/// <summary>
	/// Opens a resource picker for an ItemResource.
	/// </summary>
	private void OnSelectItem()
	{
		var picker = AssetPicker.Create( this, AssetType.FromExtension( "item" ) );
		picker.OnAssetPicked = ( assets ) =>
		{
			var asset = assets.FirstOrDefault();
			if ( asset is null ) return;

			var resource = ResourceLibrary.Get<ItemResource>( asset.Path );
			if ( resource is null ) return;

			_selectedItem = resource;
			_itemLabel.Text = $"✓ {resource.Name}";
			_itemLabel.Color = Theme.Green;

			// Reset to a blank hold type so previous values don't bleed into a new item
			_existingResource = null;
			_holdType = new HoldTypeResource();
			_holdTypeSerialized = _holdType.GetSerialized();
			_holdTypeSheet.Clear( true );
			_holdTypeSheet.AddObject( _holdTypeSerialized );
			_holdTypeSerialized.OnPropertyChanged += OnHoldPropertyChanged;

			using ( _previewScene.Push() )
			{
				RebuildItemPreview();
			}
		};
		picker.Show();
	}

	/// <summary>
	/// Load an existing .holdtype resource to edit.
	/// </summary>
	private void OnLoadExisting()
	{
		var picker = AssetPicker.Create( this, AssetType.FromExtension( "holdtype" ) );
		picker.OnAssetPicked = ( assets ) =>
		{
			var asset = assets.FirstOrDefault();
			if ( asset is null ) return;

			var resource = ResourceLibrary.Get<HoldTypeResource>( asset.Path );
			if ( resource is null ) return;

			_existingResource = resource;
			_holdType = resource;

			// Rebuild the ControlSheet bound to the loaded resource
			_holdTypeSerialized = _holdType.GetSerialized();
			_holdTypeSheet.Clear( true );
			_holdTypeSheet.AddObject( _holdTypeSerialized );
			_holdTypeSerialized.OnPropertyChanged += OnHoldPropertyChanged;

			// Restore item preview if the resource remembers which item it was made for
			if ( resource.PreviewItem is not null )
			{
				_selectedItem = resource.PreviewItem;
				_itemLabel.Text = $"✓ {resource.PreviewItem.Name}";
				_itemLabel.Color = Theme.Green;

				using ( _previewScene.Push() )
				{
					RebuildItemPreview();
				}
			}

			SetStatus( $"Loaded: {resource.ResourceName}", Theme.Yellow );
		};
		picker.Show();
	}

	/// <summary>
	/// Rebuild the item preview model. Called once when the item selection changes,
	/// NOT during rendering to avoid collection-modified exceptions.
	/// </summary>
	private void RebuildItemPreview()
	{
		if ( _itemPreviewGo.IsValid() )
		{
			_itemPreviewGo.Destroy();
			_itemPreviewGo = null;
		}

		if ( _selectedItem?.PrefabFile is null || !_citizenRenderer.IsValid() ) return;

		try
		{
			var prefabScene = SceneUtility.GetPrefabScene( _selectedItem.PrefabFile );
			if ( prefabScene is null ) return;

			_itemPreviewGo = prefabScene.Clone( Vector3.Zero );
		}
		catch ( System.Exception e )
		{
			Log.Warning( $"[HoldTypeEditor] Failed to load prefab: {e.Message}" );
			return;
		}

		if ( !_itemPreviewGo.IsValid() ) return;

		// Disable physics so the item just sits in the hand
		foreach ( var rb in _itemPreviewGo.Components.GetAll<Rigidbody>( FindMode.EverythingInDescendants ) )
			rb.Enabled = false;
		foreach ( var col in _itemPreviewGo.Components.GetAll<Collider>( FindMode.EverythingInDescendants ) )
			col.Enabled = false;

		var hand = _citizenRenderer.GetBoneObject( "hold_R" );
		if ( !hand.IsValid() ) return;
		_itemPreviewGo.SetParent( hand );
		_itemPreviewGo.LocalPosition = Vector3.Zero;
		_itemPreviewGo.LocalRotation = Rotation.Identity;
	}

	/// <summary>
	/// Update pose and offsets every frame. Does NOT create or destroy GameObjects.
	/// </summary>
	private void UpdatePose()
	{
		if ( !_citizenGo.IsValid() || !_citizenRenderer.IsValid() ) return;

		// ── Pose the citizen with hold type ───────────────────────
		_citizenRenderer.Set( "holdtype", (int)_holdType.HoldType );

		// ── Update item offsets ────────────────────────────────────
		if ( _itemPreviewGo.IsValid() )
		{
			_itemPreviewGo.LocalPosition = _holdType.PositionOffset;
			_itemPreviewGo.LocalRotation = Rotation.From( _holdType.RotationOffset );
			_itemPreviewGo.LocalScale = _holdType.Scale;
		}
	}

	// ── Cached finger bones (populated once after first tick) ─────
	private const float MaxCurlAngle = 90f;
	private record struct BoneEntry( GameObject Go, Rotation RestPose );
	private Dictionary<string, BoneEntry> _fingerBones;

	/// <summary>
	/// Cache all finger and arm bone GameObjects once so we don't call GetBoneObject every frame.
	/// </summary>
	private static Dictionary<string, BoneEntry> CacheBonesFromRenderer( SkinnedModelRenderer renderer )
	{
		var dict = new Dictionary<string, BoneEntry>();
		if ( !renderer.IsValid() ) return dict;

		// Arm bones — try common citizen naming variants, store under the first that resolves
		foreach ( var side in new[] { "R", "L" } )
		{
			AddFirst( dict, renderer, $"upperarm_{side}",
				$"upperarm_{side}", $"arm_upper_{side}", $"upper_arm_{side}" );
			AddFirst( dict, renderer, $"lowerarm_{side}",
				$"lowerarm_{side}", $"arm_lower_{side}", $"forearm_{side}" );
			AddFirst( dict, renderer, $"hand_{side}",
				$"hand_{side}" );
		}

		// Finger bones
		foreach ( var side in new[] { "R", "L" } )
		{
			for ( int j = 0; j < 3; j++ )
				Add( dict, renderer, $"finger_thumb_{j}_{side}" );

			foreach ( var finger in new[] { "index", "middle", "ring" } )
			{
				Add( dict, renderer, $"finger_{finger}_meta_{side}" );
				for ( int j = 0; j < 3; j++ )
					Add( dict, renderer, $"finger_{finger}_{j}_{side}" );
			}
		}

		var armKeys = string.Join( ", ", dict.Keys.Where( k => !k.StartsWith( "finger" ) ) );
		if ( armKeys.Length == 0 )
			Log.Warning( "[HoldTypeEditor] No arm bones resolved — check citizen bone names." );
		return dict;

		static void Add( Dictionary<string, BoneEntry> d, SkinnedModelRenderer r, string name )
		{
			var go = r.GetBoneObject( name );
			if ( go.IsValid() ) d[name] = new BoneEntry( go, go.LocalRotation );
		}

		// Try each candidate name in order; store under key (first candidate) when one resolves.
		static void AddFirst( Dictionary<string, BoneEntry> d, SkinnedModelRenderer r,
			string key, params string[] candidates )
		{
			foreach ( var name in candidates )
			{
				var go = r.GetBoneObject( name );
				if ( go.IsValid() )
				{
					d[key] = new BoneEntry( go, go.LocalRotation );
					return;
				}
			}
		}
	}

	private static void ApplyFingerPose( Dictionary<string, BoneEntry> bones, HoldTypeResource hold )
	{
		if ( bones is null || hold is null ) return;

		// Arm bones
		OverrideBone( bones, "upperarm_R", hold.RightUpperArm );
		OverrideBone( bones, "lowerarm_R", hold.RightForearm );
		OverrideBone( bones, "hand_R",     hold.RightWrist );
		OverrideBone( bones, "upperarm_L", hold.LeftUpperArm );
		OverrideBone( bones, "lowerarm_L", hold.LeftForearm );
		OverrideBone( bones, "hand_L",     hold.LeftWrist );

		// Right hand fingers
		CurlThumb( bones, "R", hold.RightThumb );
		CurlFinger( bones, "index", "R", hold.RightIndex );
		CurlFinger( bones, "middle", "R", hold.RightMiddle );
		CurlFinger( bones, "ring", "R", hold.RightRing );

		// Left hand fingers
		CurlThumb( bones, "L", hold.LeftThumb );
		CurlFinger( bones, "index", "L", hold.LeftIndex );
		CurlFinger( bones, "middle", "L", hold.LeftMiddle );
		CurlFinger( bones, "ring", "L", hold.LeftRing );
	}

	private static void OverrideBone( Dictionary<string, BoneEntry> bones, string name, Angles offset )
	{
		if ( offset == Angles.Zero ) return;
		if ( !bones.TryGetValue( name, out var entry ) || !entry.Go.IsValid() ) return;
		entry.Go.Flags |= GameObjectFlags.ProceduralBone;
		entry.Go.LocalRotation = entry.RestPose * Rotation.From( offset );
	}

	private static void CurlBone( Dictionary<string, BoneEntry> bones, string name, Rotation curlRotation )
	{
		if ( !bones.TryGetValue( name, out var entry ) || !entry.Go.IsValid() ) return;
		entry.Go.Flags |= GameObjectFlags.ProceduralBone;
		entry.Go.LocalRotation = entry.RestPose * curlRotation;
	}

	private static void CurlThumb( Dictionary<string, BoneEntry> bones, string side, float curl )
	{
		var rot = Rotation.FromRoll( curl * MaxCurlAngle );
		for ( int j = 0; j < 3; j++ )
			CurlBone( bones, $"finger_thumb_{j}_{side}", rot );
	}

	private static void CurlFinger( Dictionary<string, BoneEntry> bones, string finger, string side, float curl )
	{
		var rot = Rotation.FromPitch( curl * MaxCurlAngle );
		// Only rotate phalange joints (0,1,2) — not meta (metacarpal/palm), which twists the wrist
		for ( int j = 0; j < 3; j++ )
			CurlBone( bones, $"finger_{finger}_{j}_{side}", rot );
	}

	/// <summary>
	/// Save the hold type resource to disk.
	/// </summary>
	private void OnSave()
	{
		// Always stamp PreviewItem so load can restore the model
		_holdType.PreviewItem = _selectedItem;

		if ( IsEditMode )
		{
			var projectRoot = Project.Current.GetRootPath();
			var assetsPath = Path.Combine( projectRoot, "Assets" );
			var fullPath = Path.Combine( assetsPath, _existingResource.ResourcePath );

			var asset = AssetSystem.FindByPath( fullPath );
			asset?.SaveToDisk( _existingResource );

			SetStatus( $"✓ Updated {_existingResource.ResourcePath}", Theme.Green );
		}
		else
		{
			// Create new resource
			var name = _selectedItem?.Name ?? "default";
			var safeName = name.ToLower().Replace( ' ', '_' );

			var projectRoot = Project.Current.GetRootPath();
			var assetsPath = Path.Combine( projectRoot, "Assets" );
			var holdDir = _selectedItem is not null
				? Path.Combine( assetsPath, "items", safeName )
				: Path.Combine( assetsPath, "items", "shared" );
			Directory.CreateDirectory( holdDir );

			var holdPath = Path.Combine( holdDir, $"{safeName}.holdtype" );

			var newAsset = AssetSystem.CreateResource( "holdtype", holdPath );
			newAsset.SaveToDisk( _holdType );

			_existingResource = _holdType;
			SetStatus( $"✓ Created {safeName}.holdtype", Theme.Green );
		}
	}

	private void SetStatus( string text, Color color )
	{
		if ( _statusLabel is null ) return;
		_statusLabel.Text = text;
		_statusLabel.Color = color;
	}

	public override void OnDestroyed()
	{
		base.OnDestroyed();

		if ( _renderWidget.IsValid() )
		{
			_renderWidget.OnPreFrame -= OnPreFrame;
		}

		_previewScene?.Destroy();
	}
}
