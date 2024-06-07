using Sandbox.Citizen;

namespace ItemBuilder;

public class PlayerController : Component
{
	[RequireComponent] public CharacterController CharacterController { get; set; }
	[RequireComponent] public CitizenAnimationHelper AnimationHelper { get; set; }

}
