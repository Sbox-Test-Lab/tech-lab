using Editor;
using Sandbox;

public class ItemInfoWidget : Widget
{
	public ItemResource Resource { get; set; } 

	public ItemInfoWidget( Widget parent ) : base( parent, true )
	{
		Layout = Layout.Column();

		Resource = new ItemResource();

		var controlSheet = new ControlSheet();

		controlSheet.AddObject( Resource.GetSerialized() );

		Layout.Add( controlSheet );
	}

}
