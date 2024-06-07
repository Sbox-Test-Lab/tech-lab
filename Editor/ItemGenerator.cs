using Editor;
using Sandbox;

public class ItemBuilder : EditorTool<ModelRenderer>
{
	public StringProperty ItemName {  get; set; }
	public StringProperty ItemType { get; set; }
}
