using Editor;
using ItemBuilder.UI;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ItemBuilder;

public class ItemInfo
{
	[Property] public string Name { get; set; }
	[Property, TextArea] public string Description { get; set; }
}

public class ItemBuilder : EditorTool<ModelRenderer>
{
	public ItemInfo ItemInfo { get; set; } = new();
	public Dictionary<Type, BoolProperty> ItemAbilities { get; set; } = new Dictionary<Type, BoolProperty>();

	public override void OnEnabled()
	{
		var window = new WidgetWindow( SceneOverlay );

		window.WindowTitle = "Item Builder";
		window.Layout = Layout.Column();
		
		window.MinimumSize = new Vector2( 128, 240 );
		window.MinimumHeight = 128.0f;
		window.MaximumWidth = 480.0f;

		var controlSheet = new ControlSheet();

		controlSheet.AddRow( EditorUtility.GetSerializedObject( ItemInfo ).GetProperty(nameof(ItemInfo.Name)) );
		controlSheet.AddRow( EditorUtility.GetSerializedObject( ItemInfo ).GetProperty(nameof(ItemInfo.Description)));

		controlSheet.Margin = new Sandbox.UI.Margin( 8 );

		controlSheet.SetMinimumColumnWidth( 0, 4 );
		
		window.Layout.Add(controlSheet);

		window.Layout.AddSeparator();

		var componentSheet = new ControlSheet();

		foreach(var item in GetDerivedClasses(typeof(BaseItemAbility)))
		{
			var boolProperty = new BoolProperty( null );
			
			boolProperty.Text = item.Name;
			boolProperty.Layout = Layout.Row();
			
			componentSheet.Add( boolProperty );

			ItemAbilities.Add(item, boolProperty);
		}

		window.Layout.Add( componentSheet );

		var generateButton = new Button.Primary( "Generate" );
	
		generateButton.Clicked = GenerateItem;
		generateButton.MaximumWidth = 96f;
		generateButton.Layout = Layout.Row();
		
		window.Layout.Add( generateButton );
		
		AddOverlay( window, TextFlag.Top, 16 );
	}

	public override void OnUpdate()
	{
		base.OnUpdate();

		var model = GetSelectedComponent<ModelRenderer>();

		if(model.IsValid())
		{
			var position = model.Bounds.Center.WithZ( model.Bounds.Maxs.z + 4) ;

			Gizmo.Draw.SolidSphere( position, 0.25f);
		}
	}

	public void GenerateItem()
	{
		var selectedGameObject = GetSelectedComponent<ModelRenderer>().GameObject;

		var gameObject = Scene.CreateObject();

		gameObject.Tags.Add( "interactable" );
	
		gameObject.Name = ItemInfo.Name;

		gameObject.Transform.World = selectedGameObject.Transform.World;

		var model = gameObject.Components.GetOrCreate<ModelRenderer>().Model = GetSelectedComponent<ModelRenderer>().Model;

		gameObject.Components.GetOrCreate<Interactable>();
		
		foreach(var itemType in ItemAbilities)
		{
			if ( itemType.Value.Value is false )
				continue;

			var typeDescription = TypeLibrary.GetType( itemType.Key );

			gameObject.Components.Create( typeDescription );
		}

		var item = gameObject.Components.GetOrCreate<Item>();
		item.Name = ItemInfo.Name;
		item.Description = ItemInfo.Description;

		var collider = gameObject.Components.GetOrCreate<ModelCollider>();
		collider.Model = model;

		gameObject.Components.Create<Rigidbody>();

		var uiGameObject = Scene.CreateObject();
		uiGameObject.Name = "UI";
		uiGameObject.SetParent( gameObject );

		var worldPanel = uiGameObject.Components.Create<WorldPanel>();

		worldPanel.Transform.Position = gameObject.GetBounds().Center;
		worldPanel.Transform.Position += new Vector3(0, 0, 8.0f);

		worldPanel.PanelSize = new Vector2( 512f, 128f );
		
		worldPanel.LookAtCamera = true;

		var itemInfoPanel = uiGameObject.Components.Create<ItemWorldInfo>();
		itemInfoPanel.ItemName = ItemInfo.Name;
		
		gameObject.SetParent( selectedGameObject.Parent );

		selectedGameObject.Destroy();
	}

	public IEnumerable<Type> GetDerivedClasses(Type type)
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(x => x.IsSubclassOf(type));
    }
}
