using Editor;
using Sandbox;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Linq;

namespace ItemBuilder;

public class ItemAbilitiesWidget : Widget
{	
	public Dictionary<TypeDescription, JsonObject> Components { get; set; } = new Dictionary<TypeDescription, JsonObject>();

	public ItemAbilitiesWidget( Widget parent ) : base( parent, true )
	{
		Layout = Layout.Column();

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
					if ( property.GetValue<bool>() == true && !Components.ContainsKey( typeDescription ) )
					{
						var json = component.Serialize().AsObject();
						json.Remove( "__guid" );
	
						Components.Add( typeDescription, json );
					}
					else if ( Components.ContainsKey( typeDescription ) )
					{
						Components.Remove( typeDescription );

						return;
					}
				}
			};

			var componentSheet = new ControlSheet { Margin = new Sandbox.UI.Margin( 0, 0, 0, 0 ) };

			componentSheet.AddGroup( typeDescription.Title, properties );

			Layout.Add( componentSheet );
		}

		var componentsSheet = new ControlSheet();

		componentsSheet.AddObject( Components.GetSerialized() );

		Layout.Add( componentsSheet );
		
		var generateButton = new Button.Primary( "Generate" );

		generateButton.Clicked = GenerateItem;

		generateButton.MaximumWidth = 96f;
		generateButton.Layout = Layout.Row();
		generateButton.Layout.Margin = 8f;

		Layout.Add( generateButton );
		
	}


	public void GenerateItem()
	{
		var selected = SceneEditorSession.Active.SerializeSelection();

		var gameObject = SceneEditorSession.Active.Scene.CreateObject();

		foreach ( var componentData in Components )
		{
			var component = gameObject.Components.Create( componentData.Key );
			component.DeserializeImmediately( componentData.Value );

			
		}


		Log.Info( $"{selected} {SceneEditorSession.Active.Selection.Count}" );
	}
	/*	
		var selectedGameObject = GetSelectedComponent<ModelRenderer>().GameObject;

		var gameObject = Scene.CreateObject();

		gameObject.Tags.Add( "interactable" );

		gameObject.Name = ItemData.Name;

		gameObject.Transform.World = selectedGameObject.Transform.World;

		var model = gameObject.Components.GetOrCreate<ModelRenderer>().Model = GetSelectedComponent<ModelRenderer>().Model;

		gameObject.Components.GetOrCreate<Interactable>();

		foreach ( var componentData in Components )
		{
			var component = gameObject.Components.Create( componentData.Key );
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
		worldPanel.Enabled = ItemData.ShowToolTip;
		worldPanel.WorldPosition = gameObject.GetBounds().Center;
		worldPanel.WorldPosition += new Vector3( 0, 0, 8.0f );

		worldPanel.PanelSize = new Vector2( 512f, 128f );

		worldPanel.LookAtCamera = true;

		var itemInfoPanel = uiGameObject.Components.Create<ItemWorldInfo>();

		gameObject.SetParent( selectedGameObject.Parent );

		selectedGameObject.Destroy();

		}
	}
	*/
}


