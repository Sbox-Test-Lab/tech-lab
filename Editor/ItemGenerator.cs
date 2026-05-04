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
	private ItemResourceWidget _itemWidget;
	private HoldTypeEditorWidget _holdTypeWidget;

	public override void OnEnabled()
	{
		var window = new WidgetWindow( SceneOverlay );

		window.WindowTitle = "Item Builder";
		window.Layout = Layout.Column();

		window.MinimumSize = new Vector2( 900, 600 );
		window.MaximumSize = new Vector2( 1200, 2000 );

		var scroll = new ScrollArea( window );
		scroll.Canvas = new Widget();
		scroll.Canvas.Layout = Layout.Column();
		scroll.Canvas.Layout.Spacing = 0;

		_itemWidget = new ItemResourceWidget( scroll.Canvas );
		scroll.Canvas.Layout.Add( _itemWidget );

		var divider = scroll.Canvas.Layout.Add( new Label( "Hold Type" ) );
		divider.Color = Theme.Blue;
		divider.SetStyles( "font-size: 13px; padding: 8px 4px 4px 4px;" );
		scroll.Canvas.Layout.AddSeparator();

		_holdTypeWidget = new HoldTypeEditorWidget( scroll.Canvas );
		_itemWidget.OnItemReady = resource => _holdTypeWidget?.SetItem( resource );
		_itemWidget.OnItemGenerated = ( resource, itemDir ) => _holdTypeWidget?.SaveHoldType( itemDir, resource );
		_itemWidget.OnModelSelected = ( model, name ) => _holdTypeWidget?.SetPreviewModel( model, name );
		scroll.Canvas.Layout.Add( _holdTypeWidget );

		// -- Bottom: status + generate button ----------------------
		scroll.Canvas.Layout.AddSeparator();
		var statusLabel = scroll.Canvas.Layout.Add( new Label( "" ) );
		var generateButton = scroll.Canvas.Layout.Add( new Button.Primary( "Generate Item" ) );
		_itemWidget.SetFooterWidgets( statusLabel, generateButton );

		scroll.Canvas.Layout.AddStretchCell();

		window.Layout.Add( scroll );

		AddOverlay( window );
	}

	public override void OnUpdate()
	{
		base.OnUpdate();

		if ( _itemWidget is null ) return;

		var selection = SceneEditorSession.Active?.Selection;
		var selected = selection?.OfType<GameObject>().FirstOrDefault();

		_itemWidget.SetSelectedObject( selected );
	}
}
