using Editor;
using Sandbox;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

using ItemBuilder.UI;

namespace ItemBuilder;

public class ItemData
{
	public string Name { get; set; }
	public string Description { get; set; }
}

[EditorTool]
[Title( "Item Builder" )]
[Icon("engineering")]
//[Shortcut("editortool.itembuilder", "i")]
[Alias( "item" )]
[Group( "0" )]
public class ItemBuilder : EditorTool
{
	public ItemData ItemData { get; set; } = new ItemData();

	public Dictionary<TypeDescription, JsonObject> Components { get; set; } = new Dictionary<TypeDescription, JsonObject>();
	public override void OnEnabled()
	{
		
		var window = new WidgetWindow( SceneOverlay );

		window.WindowTitle = "Item Builder";
		window.Layout = Layout.Column();
		
		window.MinimumSize = new Vector2( 128, 240 );
		window.MinimumHeight = 128.0f;
		window.MaximumWidth = 480.0f;

		var controlSheet = new ControlSheet();

		controlSheet.AddObject( ItemData.GetSerialized() );
		
		window.Layout.Add( controlSheet );
		
	
		foreach ( var typeDescription in TypeLibrary.GetTypes<BaseItemAbility>() )
		{
			if ( typeDescription.IsAbstract )
				continue;

			var component = typeDescription.Create<Component>();
			component.Enabled = true;

			var serialized = component.GetSerialized();
			var properties = serialized.Where( x => x.HasAttribute<ItemAbilityPropertyAttribute>() ).ToArray();
			
			serialized.OnPropertyChanged += property =>
			{
				if ( property.Name == "GenerateComponentEditor" )
				{
					if ( property.GetValue<bool>() == true && !Components.ContainsKey(typeDescription) )
					{
						var json = component.Serialize().AsObject();
						json.Remove( "__guid" );
						Log.Info( $"{json}" );
						Components.Add( typeDescription, json );
					}
					else if ( Components.ContainsKey( typeDescription ) )
					{	
						Components.Remove(typeDescription);

						return;
					}

					Log.Info( $"{Components.Count}" );
				}
			};

			var componentSheet = new ControlSheet { Margin = new Sandbox.UI.Margin( 0, 0, 0, 0 ) };

			componentSheet.AddGroup( typeDescription.Title, properties );

			window.Layout.Add( componentSheet );
		}

		var componentsSheet = new ControlSheet();

		componentsSheet.AddObject( Components.GetSerialized() );
		
		window.Layout.Add( componentsSheet );

		var generateButton = new Button.Primary( "Generate" );
	
		generateButton.Clicked = GenerateItem;
		
		generateButton.MaximumWidth = 96f;
		generateButton.Layout = Layout.Row();
		generateButton.Layout.Margin = 8f;

		window.Layout.Add( generateButton );
		
		AddOverlay( window, TextFlag.Top, 16 );
		
	}

	public override void OnUpdate()
	{
		base.OnUpdate();

		var traceResult = Scene.Trace.Ray( Gizmo.CurrentRay, 4096 )
			.UseRenderMeshes( true )
			.UsePhysicsWorld( false )
			.Run();

		if ( traceResult.Hit )
		{
			using ( Gizmo.Scope( "cursor" ) )
			{
				//Gizmo.Transform = new Transform( traceResult.HitPosition.SnapToGrid(4.0f), Rotation.LookAt( traceResult.Normal ) );
				//Gizmo.Draw.SolidSphere(Vector3.Zero, 8.0f);
			}
		}
	}

	public void GenerateItem()
	{
		var selectedGameObject = GetSelectedComponent<ModelRenderer>().GameObject;

		var gameObject = Scene.CreateObject();

		gameObject.Tags.Add( "interactable" );

		gameObject.Name = ItemData.Name;

		gameObject.Transform.World = selectedGameObject.Transform.World;

		var model = gameObject.Components.GetOrCreate<ModelRenderer>().Model = GetSelectedComponent<ModelRenderer>().Model;

		gameObject.Components.GetOrCreate<Interactable>();

		foreach(var componentData in Components)
		{
			var component = gameObject.Components.Create(componentData.Key );
			component.DeserializeImmediately( componentData.Value );
		}

		
		var item = gameObject.Components.GetOrCreate<Item>();
		item.Name = ItemData.Name;
		item.Description = ItemData.Description;

		var collider = gameObject.Components.GetOrCreate<ModelCollider>();
		collider.Model = model;

		gameObject.Components.Create<Rigidbody>();

		var uiGameObject = Scene.CreateObject();
		uiGameObject.Name = "UI";
		uiGameObject.SetParent( gameObject );

		var worldPanel = uiGameObject.Components.Create<WorldPanel>();

		worldPanel.Transform.Position = gameObject.GetBounds().Center;
		worldPanel.Transform.Position += new Vector3( 0, 0, 8.0f );

		worldPanel.PanelSize = new Vector2( 512f, 128f );

		worldPanel.LookAtCamera = true;

		var itemInfoPanel = uiGameObject.Components.Create<ItemWorldInfo>();

		gameObject.SetParent( selectedGameObject.Parent );

		selectedGameObject.Destroy();

	}
}
