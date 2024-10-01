using System;

namespace ItemBuilder;

public class Wave : Component
{
	[Property] public float Amplitude { get; set; } = 0.1f;
	protected override void OnUpdate()
	{
		base.OnUpdate();

		GameObject.Transform.Position = new Vector3(GameObject.Transform.Position.x, GameObject.Transform.Position.y, GameObject.Transform.Position.z + MathF.Sin( Time.Now ) * Amplitude);
	}
}
