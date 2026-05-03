using Editor;
using Sandbox;

namespace TestLab;

[EditorTool( "HoldTypeEditor" )]
[Title( "Hold Type Editor" )]
[Icon( "front_hand" )]
[Alias( "holdtype" )]
[Group( "0" )]
public class HoldTypeEditor : EditorTool
{
	private HoldTypeEditorWidget _widget;

	public override void OnEnabled()
	{
		var window = new WidgetWindow( SceneOverlay );
		window.WindowTitle = "Hold Type Editor";
		window.Layout = Layout.Column();
		window.MinimumSize = new Vector2( 800, 600 );
		window.MaximumSize = new Vector2( 800, 2000 );

		var scroll = new ScrollArea( window );
		scroll.Canvas = new Widget();
		scroll.Canvas.Layout = Layout.Column();

		_widget = new HoldTypeEditorWidget( scroll.Canvas );
		scroll.Canvas.Layout.Add( _widget );
		scroll.Canvas.Layout.AddStretchCell();

		window.Layout.Add( scroll );
		AddOverlay( window );
	}
}
