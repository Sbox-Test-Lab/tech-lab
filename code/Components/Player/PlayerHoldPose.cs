using Sandbox.Citizen;

/// <summary>
/// Applies a <see cref="HoldTypeResource"/> to the player's citizen body each frame —
/// sets the HoldType animgraph parameter and overrides arm / finger bones.
/// Attach to the player Body GameObject; set <see cref="Config"/> to activate.
/// </summary>
public class PlayerHoldPose : Component
{
	public HoldTypeResource Config { get; set; }

	private SkinnedModelRenderer _renderer;
	private CitizenAnimationHelper _anim;

	// Cached bone GameObjects (populated on first apply)
	private GameObject _upperArmR, _forearmR, _wristR;
	private GameObject _upperArmL, _forearmL, _wristL;

	// Rest poses captured from the animgraph before we take over the bones
	private Rotation _restUpperArmR, _restForearmR, _restWristR;
	private Rotation _restUpperArmL, _restForearmL, _restWristL;
	private bool _bonesCached;

	protected override void OnStart()
	{
		CacheComponents();
	}

	private void CacheComponents()
	{
		_renderer ??= Components.Get<SkinnedModelRenderer>();
		_anim     ??= Components.Get<CitizenAnimationHelper>() ??
					  Components.Get<CitizenAnimationHelper>( FindMode.InAncestors );
	}

	protected override void OnUpdate()
	{
		// Retry every frame until both are resolved (handles mid-frame GetOrCreate)
		if ( _renderer is null || _anim is null )
			CacheComponents();

		if ( Config is null )
		{
			ClearPose();
			return;
		}

		ApplyAnimParams();
		EnsureBonesCached();
		ApplyBoneOverrides();
	}

	private void ApplyAnimParams()
	{
		if ( _renderer is null ) return;
		// Set directly on the renderer to avoid PlayerController overwriting CitizenAnimationHelper.HoldType
		_renderer.Set( "holdtype", (int)Config.HoldType );
	}

	private void EnsureBonesCached()
	{
		if ( _bonesCached || _renderer is null ) return;

		_upperArmR = FindBone( "upperarm_R", "arm_upper_R", "upper_arm_R" );
		_forearmR   = FindBone( "lowerarm_R", "arm_lower_R", "forearm_R" );
		_wristR     = FindBone( "hand_R" );
		_upperArmL = FindBone( "upperarm_L", "arm_upper_L", "upper_arm_L" );
		_forearmL   = FindBone( "lowerarm_L", "arm_lower_L", "forearm_L" );
		_wristL     = FindBone( "hand_L" );

		// Capture the animgraph's current local rotation as the rest pose
		// BEFORE we flag any bone as ProceduralBone (which would freeze it).
		_restUpperArmR = BoneRestPose( _upperArmR );
		_restForearmR  = BoneRestPose( _forearmR );
		_restWristR    = BoneRestPose( _wristR );
		_restUpperArmL = BoneRestPose( _upperArmL );
		_restForearmL  = BoneRestPose( _forearmL );
		_restWristL    = BoneRestPose( _wristL );

		Log.Info( $"[PlayerHoldPose] Bones cached — upperArmR={_upperArmR.IsValid()}, forearmR={_forearmR.IsValid()}, wristR={_wristR.IsValid()}" );
		_bonesCached = true;
	}

	private GameObject FindBone( params string[] candidates )
	{
		foreach ( var name in candidates )
		{
			var bone = _renderer.GetBoneObject( name );
			if ( bone.IsValid() ) return bone;
		}
		return null;
	}

	private static Rotation BoneRestPose( GameObject bone )
	{
		if ( bone is null || !bone.IsValid() ) return Rotation.Identity;
		return bone.LocalRotation;
	}

	private void ApplyBoneOverrides()
	{
		OverrideBone( _upperArmR, _restUpperArmR, Config.RightUpperArm );
		OverrideBone( _forearmR,  _restForearmR,  Config.RightForearm );
		OverrideBone( _wristR,    _restWristR,    Config.RightWrist );
		OverrideBone( _upperArmL, _restUpperArmL, Config.LeftUpperArm );
		OverrideBone( _forearmL,  _restForearmL,  Config.LeftForearm );
		OverrideBone( _wristL,    _restWristL,    Config.LeftWrist );
	}

	private static void OverrideBone( GameObject bone, Rotation restPose, Angles offset )
	{
		if ( bone is null || !bone.IsValid() ) return;

		if ( offset == Angles.Zero )
		{
			bone.Flags &= ~GameObjectFlags.ProceduralBone;
			return;
		}

		bone.Flags |= GameObjectFlags.ProceduralBone;
		bone.LocalRotation = restPose * Rotation.From( offset );
	}

	private void ClearPose()
	{
		if ( _renderer is not null )
			_renderer.Set( "holdtype", (int)CitizenAnimationHelper.HoldTypes.None );

		ClearBone( _upperArmR );
		ClearBone( _forearmR );
		ClearBone( _wristR );
		ClearBone( _upperArmL );
		ClearBone( _forearmL );
		ClearBone( _wristL );
	}

	private static void ClearBone( GameObject bone )
	{
		if ( bone is null || !bone.IsValid() ) return;
		bone.Flags &= ~GameObjectFlags.ProceduralBone;
	}

	/// <summary>
	/// Change the active config and invalidate the bone cache so bones are re-fetched
	/// if the body model has changed.
	/// </summary>
	public void SetConfig( HoldTypeResource config )
	{
		if ( Config == config ) return;
		Config = config;
		_bonesCached = false;
	}
}
