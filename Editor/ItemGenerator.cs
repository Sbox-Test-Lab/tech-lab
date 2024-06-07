using Editor;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

public class ItemBuilder : EditorTool<ModelRenderer>
{
	public StringProperty ItemName {  get; set; }
	public StringProperty ItemType { get; set; }



	public override void OnEnabled()
	{
		ItemName = new StringProperty( null );
		ItemType = new StringProperty( null );

		var window = new WidgetWindow( SceneOverlay );

		window.WindowTitle = "Item Builder";

		window.Layout = Layout.Column();
		window.Layout.Margin = 4;
		window.MinimumSize = new Vector2( 192, 480 );

		ItemName.PlaceholderText = "Item Name";
		ItemType.PlaceholderText = "Item Type";

		window.Layout.Add( ItemName );
		window.Layout.Add( ItemType );
		
		AddOverlay( window, TextFlag.Top, 16 );
	}

	public override void OnSelectionChanged()
	{
		var target = GetSelectedComponent<ModelRenderer>();
	}

	public IEnumerable<Type> GetDerivedClasses(Type type)
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(x => x.IsSubclassOf(type));
    }
}
