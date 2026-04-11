using Editor;
using Sandbox;
using System.Linq;

namespace TestLab;

[EditorTool( "ItemBuilder" )]
[Title( "Item Builder" )]
[Icon( "engineering" )]
[Alias( "item" )]
[Group( "0" )]
public class ItemBuilder : EditorTool
{
	private ItemResourceWidget _widget;

	public override void OnEnabled()
	{
		var window = new WidgetWindow( SceneOverlay );

		window.WindowTitle = "Item Builder";
		window.Layout = Layout.Column();

		window.MinimumSize = new Vector2( 640, 400 );
		window.MaximumSize = new Vector2( 800, 800 );

		var scroll = new ScrollArea( window );
		scroll.Canvas = new Widget();
		scroll.Canvas.Layout = Layout.Column();

		_widget = new ItemResourceWidget( scroll.Canvas );
		scroll.Canvas.Layout.Add( _widget );
		scroll.Canvas.Layout.AddStretchCell();

		window.Layout.Add( scroll );

		AddOverlay( window );
	}

	public override void OnUpdate()
	{
		base.OnUpdate();

		if ( _widget is null ) return;

		// Feed the current scene selection into the widget
		var selection = SceneEditorSession.Active?.Selection;
		var selected = selection?.OfType<GameObject>().FirstOrDefault();

		_widget.SetSelectedObject( selected );
	}
}
