using Foliage;

namespace Editor.FoliagePainter;


/// <summary>
/// Brushes you can use
/// </summary>
public class FoliageList
{
	public FoliageResource Selected { get; set; }
	public List<FoliageResource> AllFoliage = new();

	public FoliageList()
	{
		LoadAll();
	}

	public void LoadAll()
	{
		AllFoliage = FoliageResource.All.ToList();
		

		Selected = AllFoliage.FirstOrDefault();
	}
}

public class FoliageType
{
	public string Name { get; private set; }
	public Texture Texture { get; private set; }
	public Pixmap Pixmap { get; private set; }

	public void Set( string name )
	{
		Texture = Texture.Load( FileSystem.Mounted, $"materials/tools/terrain/brushes/{name}.png" );
	}

	internal static FoliageType LoadFromFile( string filename )
	{
		var foliageType = new FoliageType();
		foliageType.Name = System.IO.Path.GetFileNameWithoutExtension( filename );
		foliageType.Texture = Texture.Load( FileSystem.Content, filename );
		foliageType.Pixmap = Pixmap.FromFile( FileSystem.Content.GetFullPath( filename ) );
		return foliageType;
	}
}
