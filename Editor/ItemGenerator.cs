using Editor;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ItemBuilder;

public class ItemBuilder : EditorTool<ModelRenderer>
{
	public Dictionary<Type, BoolProperty> ItemTypes { get; set; } = new Dictionary<Type, BoolProperty>();

	public StringProperty ItemName { get; set; }
	public StringProperty ItemDescription { get; set; }
	public Button GenerateButton { get; set; }

	public override void OnEnabled()
	{
		ItemName = new StringProperty( null );
		ItemDescription = new StringProperty( null );

		GenerateButton = new Button();
		GenerateButton.Text = "Generate";
		GenerateButton.Clicked = GenerateItem;

		var window = new WidgetWindow( SceneOverlay );

		window.WindowTitle = "Item Builder";

		window.Layout = Layout.Column();
		window.Layout.Margin = 4;
		window.MinimumSize = new Vector2( 192, 480 );

		ItemName.PlaceholderText = "Item Name";
		ItemDescription.PlaceholderText = "Item Type";

		window.Layout.Add( ItemName );
		window.Layout.Add( ItemDescription );

		
		foreach(var item in GetDerivedClasses(typeof(BaseItemType)))
		{
			var boolProperty = new BoolProperty( null );
			boolProperty.Text = item.Name;
			window.Layout.Add( boolProperty );

			ItemTypes.Add(item, boolProperty);
		}
		

		window.Layout.Add( GenerateButton );
		
	
		AddOverlay( window, TextFlag.Top, 16 );
	}

	public void GenerateItem()
	{
		var gameObject = Scene.CreateObject();

		gameObject.Name = ItemName.Text;

		gameObject.Transform.World = GetSelectedComponent<ModelRenderer>().GameObject.Transform.World;

		var model = gameObject.Components.GetOrCreate<ModelRenderer>().Model = GetSelectedComponent<ModelRenderer>().Model;

		gameObject.Components.GetOrCreate<Interactable>();
		
		foreach(var itemType in ItemTypes)
		{
			if ( itemType.Value.Value is false )
				continue;

			var typeDescription = TypeLibrary.GetType( itemType.Key );

			gameObject.Components.Create( typeDescription );
		}

		var item = gameObject.Components.GetOrCreate<Item>();
		item.Name = ItemName.Text;
		item.Description = ItemDescription.Text;

		var collider = gameObject.Components.GetOrCreate<ModelCollider>();
		collider.Model = model;

		gameObject.Components.Create<Rigidbody>();
	}

	public IEnumerable<Type> GetDerivedClasses(Type type)
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(x => x.IsSubclassOf(type));
    }
}
