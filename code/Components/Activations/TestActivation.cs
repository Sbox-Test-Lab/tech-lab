partial class TestActivation : Component, Component.ITriggerListener, IActivation
{
	[Property] public GameObject Activator { get; set; }
	[Property] public SoundEvent ActivationSound { get; set; }

	void OnActivated( GameObject activator )
	{
		Log.Info( "TestActivation.OnActivated" );
	}

	public void OnTriggerEnter( Collider other )
	{
		Log.Info(other.GameObject.Name + " entered trigger");

		if ( other.GameObject.Root.Tags.Has( "activatable" ) && other.GameObject.Name == Activator.Name)
		{
			Log.Info( $"{other.GameObject.Root} activating {this}" );

			Sound.Play( ActivationSound );
		}
	}

	public void OnTriggerExit( Collider other )
	{
		Log.Info( other.GameObject.Name + " exited trigger" );
	}
}
