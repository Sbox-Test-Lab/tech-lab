using Editor;
using Sandbox;
using System.Collections.Generic;

using System.Linq;


namespace ItemBuilder.Editor;

public sealed class ItemResourceEditor : BaseResourceEditor<ItemResource>
{
	private SerializedObject ResourceObject { get; set; }
	private GameObject PrefabObject { get; set; } = new GameObject();
	private List<TypeDescription> Components { get; set; } = new List<TypeDescription>();
	public ItemResourceEditor()
	{
		WindowTitle = "Item Resource Editor";

		Layout = Layout.Column();
	}

	protected override void Initialize( Asset asset, ItemResource resource )
	{
		Layout.Clear( true );

		ResourceObject = resource.GetSerialized();

		var sheet = new ControlSheet();
		sheet.AddObject( ResourceObject );

		Layout.Add( sheet );

		ResourceObject.OnPropertyChanged += NoteChanged;

		foreach ( var typeDescription in TypeLibrary.GetTypes<BaseItemAbility>() )
		{
			if ( typeDescription.IsAbstract )
				continue;

			var component = typeDescription.Create<Component>();

			var serialized = component.GetSerialized();
			var properties = serialized.Where( x => x.HasAttribute<ItemAbilityPropertyAttribute>() ).ToArray();

			serialized.OnPropertyChanged += property =>
			{
				if ( property.Name == "GenerateComponent" )
				{
					if ( property.GetValue<bool>() == true && !Components.Contains(typeDescription) )
					{
						Components.Add( typeDescription );
					} 
					else if(Components.Contains(typeDescription))
					{
						Components.Remove(typeDescription );

						return;
					}
				}

				NoteChanged( property );
			};


			var componentSheet = new ControlSheet { Margin = new Sandbox.UI.Margin( 0, 0, 0, 0 ) };

			componentSheet.AddGroup( typeDescription.Title, properties );

			Layout.Add( componentSheet );
		}

		var saveButton = new Button.Primary( "Save" );
		saveButton.ToolTip = "Save as a prefab";
		saveButton.Clicked += SavePrefab;

		Layout.Add( saveButton );
	}

	private void SavePrefab()
	{
		foreach(var component in Components)
		{
			PrefabObject.Components.Create(component);
		}

		var path = EditorUtility.SaveFileDialog( "Save Item Prefab as...", "prefab", $"{Project.Current.GetAssetsPath()}/untitled.prefab" );

		var asset = AssetSystem.CreateResource( "prefab", path );

		if(!asset.TryLoadResource<PrefabFile>( out var prefab ))
		{
			Log.Error( "Failed to load prefab" );
			return;
		}

		prefab = EditorUtility.Prefabs.CreateAsset( PrefabObject );
		
		asset.SaveToDisk( prefab );
	}
	
}
