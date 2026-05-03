using Sandbox.Citizen;

/// <summary>
/// Defines how an item is held — hand animation, grip offset, and per-finger curl.
/// Create .holdtype assets in the editor or via the Hold Type Editor tool.
/// </summary>
[AssetType( Name = "Hold Type", Extension = "holdtype", Category = "Item data" )]
public class HoldTypeResource : GameResource
{
	/// <summary>
	/// The item this hold type was authored for. Used to restore the preview when loading.
	/// </summary>
	[Property, Group( "Animation" )]
	public ItemResource PreviewItem { get; set; }

	[Property, Group( "Animation" )]
	public CitizenAnimationHelper.HoldTypes HoldType { get; set; } = CitizenAnimationHelper.HoldTypes.None;

	/// <summary>
	/// Position offset from the hold_R bone origin.
	/// </summary>
	[Property, Group( "Grip" )]
	public Vector3 PositionOffset { get; set; } = Vector3.Zero;

	/// <summary>
	/// Rotation offset from the hold_R bone.
	/// </summary>
	[Property, Group( "Grip" )]
	public Angles RotationOffset { get; set; } = Angles.Zero;

	/// <summary>
	/// Per-axis scale applied to the item when held.
	/// </summary>
	[Property, Group( "Grip" )]
	public Vector3 Scale { get; set; } = Vector3.One;

	// ── Right Arm ─────────────────────────────────────────────────

	[Property, Group( "Right Arm" ), Title( "Upper Arm" )]
	public Angles RightUpperArm { get; set; } = Angles.Zero;

	[Property, Group( "Right Arm" ), Title( "Forearm" )]
	public Angles RightForearm { get; set; } = Angles.Zero;

	[Property, Group( "Right Arm" ), Title( "Wrist" )]
	public Angles RightWrist { get; set; } = Angles.Zero;

	// ── Left Arm ──────────────────────────────────────────────────

	[Property, Group( "Left Arm" ), Title( "Upper Arm" )]
	public Angles LeftUpperArm { get; set; } = Angles.Zero;

	[Property, Group( "Left Arm" ), Title( "Forearm" )]
	public Angles LeftForearm { get; set; } = Angles.Zero;

	[Property, Group( "Left Arm" ), Title( "Wrist" )]
	public Angles LeftWrist { get; set; } = Angles.Zero;

	// ── Right Hand Fingers ────────────────────────────────────────

	[Property, Group( "Right Hand" ), Range( 0f, 1f ), Title( "Thumb" )]
	public float RightThumb { get; set; } = 0.0f;

	[Property, Group( "Right Hand" ), Range( 0f, 1f ), Title( "Index" )]
	public float RightIndex { get; set; } = 0.0f;

	[Property, Group( "Right Hand" ), Range( 0f, 1f ), Title( "Middle" )]
	public float RightMiddle { get; set; } = 0.0f;

	[Property, Group( "Right Hand" ), Range( 0f, 1f ), Title( "Ring" )]
	public float RightRing { get; set; } = 0.0f;

	[Property, Group( "Right Hand" ), Range( 0f, 1f ), Title( "Pinky" )]
	public float RightPinky { get; set; } = 0.0f;

	// ── Left Hand Fingers ─────────────────────────────────────────

	[Property, Group( "Left Hand" ), Range( 0f, 1f ), Title( "Thumb" )]
	public float LeftThumb { get; set; } = 0.0f;

	[Property, Group( "Left Hand" ), Range( 0f, 1f ), Title( "Index" )]
	public float LeftIndex { get; set; } = 0.0f;

	[Property, Group( "Left Hand" ), Range( 0f, 1f ), Title( "Middle" )]
	public float LeftMiddle { get; set; } = 0.0f;

	[Property, Group( "Left Hand" ), Range( 0f, 1f ), Title( "Ring" )]
	public float LeftRing { get; set; } = 0.0f;

	[Property, Group( "Left Hand" ), Range( 0f, 1f ), Title( "Pinky" )]
	public float LeftPinky { get; set; } = 0.0f;
}
