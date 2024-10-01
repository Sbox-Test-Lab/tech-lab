public class ParticleControllerExample : ParticleController
{
	protected override void OnParticleStep( Particle particle, float delta )
	{
		particle.Velocity += Vector3.Down * 1840 * delta;
	}
}
