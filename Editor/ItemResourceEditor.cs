using Editor;
using Sandbox;
using System.Linq;

namespace ItemBuilder.Editor;

/// <summary>
/// Asset Browser editor for .item files. Shows the resource properties
/// for editing, plus a read-only preview of all behaviors on the linked prefab.
/// </summary>
public sealed class ItemResourceEditor : BaseResourceEditor<ItemResource>
{
	private SerializedObject ResourceObject { get; set; }
	private Layout _behaviorsLayout;
	private ItemResource _resource;

	public ItemResourceEditor()
	{
		WindowTitle = "Item Resource Editor";
		Layout = Layout.Column();
	}

	protected override void Initialize( Asset asset, ItemResource resource )
	{
		Layout.Clear( true );
		_resource = resource;

		ResourceObject = resource.GetSerialized();

		var sheet = new ControlSheet();
		sheet.AddObject( ResourceObject );

		Layout.Add( sheet );

		ResourceObject.OnPropertyChanged += OnResourcePropertyChanged;
	}

	private void OnResourcePropertyChanged( SerializedProperty property )
	{
		NoteChanged( property );
	}
}
