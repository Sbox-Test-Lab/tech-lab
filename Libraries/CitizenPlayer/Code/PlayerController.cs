using Sandbox;
using Sandbox.Citizen;

public class PlayerController : Component
{
	[RequireComponent] public CharacterController CharacterController { get; set; }
	[RequireComponent] public CitizenAnimationHelper AnimationHelper { get; set; }

	[Property] public GameObject Eye { get; set; }
	[Property] public GameObject Body { get; set; }

	[Property] public float WishSpeed { get; set; } = 128.0f;
	[Property] public float MaxWishSpeed { get; set; } = 320.0f;
	[Property] public float GroundControl { get; set; } = 4.0f;
	[Property] public float AirControl { get; set; } = 0.1f;
	[Property] public float MaxForce { get; set; } = 48.0f;

	[Property] public float RotationSpeed { get; set; } = 8.0f;

	[Property] public float CameraDistance { get; set; } = 256.0f;
	[Property] public float MinCameraDistance { get; set; } = 32.0f;
	[Property] public float MaxCameraDistance { get; set; } = 256.0f;
	[Property] public float CameraZoomSpeed { get; set; } = 32.0f;

	[Property] public Vector3 CameraOffset { get; set; } = new Vector3(0, -16, 0);

	[Sync] public Angles EyeAngles { get; set; }
	[Sync] public bool IsRunning { get; set; }

	public Vector3 WishVelocity { get; private set; }
	public Vector3 ProxyWishVelocity { get; set; }

	public Vector3 AimPosition => Eye.Transform.Position + (EyeAngles.ToRotation() * CameraOffset);

	protected override void OnUpdate()
	{
		base.OnUpdate();

		UpdateCamera();
		UpdateEyeAngles();

		RotateCharacter();
		AnimateCharacter();
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( IsProxy )
			return;

		BuildWishVelocity();
		MoveCharacter();
	}

	private void UpdateCamera()
	{
		if ( IsProxy )
			return;

		CameraDistance = MathX.Clamp(MathX.LerpTo(CameraDistance, CameraDistance + Input.MouseWheel.y * CameraZoomSpeed, Time.Delta * CameraZoomSpeed), MinCameraDistance, MaxCameraDistance);
	}

	private void UpdateEyeAngles()
	{
		if ( IsProxy )
			return;

		var eyeAngles = EyeAngles;
		
		eyeAngles.pitch += Input.MouseDelta.y * 0.1f;
		eyeAngles.yaw -= Input.MouseDelta.x * 0.1f;
		eyeAngles.roll = 0.0f;

		eyeAngles.pitch = eyeAngles.pitch.Clamp( -89.0f, 89.0f );

		EyeAngles = eyeAngles.ToRotation();
	}

	private void RotateCharacter()
	{
		var targetAngle = new Angles(0, EyeAngles.yaw, 0).ToRotation();

		float rotateDifference = Body.Transform.Rotation.Distance(targetAngle);

		if( rotateDifference > 30f || CharacterController.Velocity.Length > 10.0f)
		{
			Body.Transform.Rotation = Rotation.Lerp( Body.Transform.Rotation, targetAngle, Time.Delta * RotationSpeed );
		}
	}

	private void AnimateCharacter()
	{
		var targetAngle = new Angles( 0, EyeAngles.yaw, 0 ).ToRotation();

		float rotateDifference = Body.Transform.Rotation.Distance( targetAngle );

		AnimationHelper.WithVelocity( CharacterController.Velocity );
		AnimationHelper.WithWishVelocity(WishVelocity);
		AnimationHelper.AimAngle = EyeAngles;
		AnimationHelper.IsGrounded = CharacterController.IsOnGround;
		AnimationHelper.MoveRotationSpeed = rotateDifference;
		AnimationHelper.MoveStyle = IsRunning ? CitizenAnimationHelper.MoveStyles.Run : CitizenAnimationHelper.MoveStyles.Walk;
	}

	private void BuildWishVelocity()
	{
		var rotation = EyeAngles.ToRotation();

		WishVelocity = rotation * Input.AnalogMove;
		WishVelocity = WishVelocity.WithZ( 0 );

		if ( !WishVelocity.IsNearZeroLength )
			WishVelocity = WishVelocity.Normal;

		if ( !IsProxy )
			IsRunning = Input.Down( "Run" );

		WishVelocity *= IsRunning ? MaxWishSpeed : WishSpeed;
	}

	private void MoveCharacter()
	{
		// Check for character jump
		if(CharacterController.IsOnGround && Input.Down("Jump"))
		{
			float groundFactor = 1.0f;
			float multiplier = 268.3281572999747f * 1.2f;

			CharacterController.Punch( Vector3.Up * multiplier * groundFactor );

			BroadcastJumpAnimation();
		}

		// Handle character on ground movement
		var gravity = Scene.PhysicsWorld.Gravity;
		var halfGravity = gravity * Time.Delta * 0.5f;

		if(CharacterController.IsOnGround)
		{
			// Apply acceleration
			CharacterController.Velocity = CharacterController.Velocity.WithZ( 0 );
			CharacterController.Accelerate( WishVelocity );
			CharacterController.ApplyFriction( GroundControl );
		}
		else
		{
			// Calculate character air control before movement
			CharacterController.Velocity += halfGravity;
			CharacterController.Accelerate(WishVelocity.ClampLength(MaxForce));
			CharacterController.ApplyFriction( AirControl );
		}

		// Move the character
		CharacterController.Move();

		if(!CharacterController.IsOnGround)
		{
			CharacterController.Velocity += halfGravity;
		}
		else
		{
			CharacterController.Velocity = CharacterController.Velocity.WithZ( 0 );
		}
	}

	[Broadcast]
	private void BroadcastJumpAnimation()
	{
		AnimationHelper.TriggerJump();
	}
}
