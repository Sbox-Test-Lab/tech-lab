using System;

namespace Foliage;
[Serializable]
[GameResource( "Foliage/Foliage", "fol", "Foliage Definition", Icon = "grass" )]
public class FoliageResource : GameResource
{
	public static HashSet<FoliageResource> All { get; set; } = new();
	[ResourceType( "vmdl" ),Category("info")]
	public Model Model { get; set; }
	public RangedFloat Scale { get; set; } = new RangedFloat( 1f );
	[Category("placement")]
	public bool AlignToNormal { get; set; } = true;
	[Category("placement"),Description("Maximum normal compared to the Up Vector to paint on")]
	public float MaxNormal { get; set; } = 180;
	[Category("placement")]
	public bool RandomRoll { get; set; } = true;
	[Category("placement")]
	public bool RandomRotation { get; set; } = false;
	[Category("placement")]
	public RangedFloat ZOffset { get; set; } = new RangedFloat( 0f );
	protected override void PostLoad()
	{
		All.Add( this );
	}
}
