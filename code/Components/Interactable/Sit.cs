using Sandbox.Internal;

public sealed class Sit : Component
{
	public SkinnedModelRenderer ModelRenderer { get; private set; } = new SkinnedModelRenderer();

	protected override void OnStart()
	{
		base.OnStart();

		ModelRenderer.Model = Model.Load( "models/citizen/citizen.vmdl" );
		ModelRenderer.Set( "sit", true );
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

	}

	protected override void DrawGizmos()
	{
		base.DrawGizmos();
		Model model = Model.Load( "models/citizen/citizen.vmdl" );
		
		//Gizmo.Hitbox.Model( ModelRenderer.Model);
		
		SceneObject sceneObject = Gizmo.Draw.Model( model );
		if ( sceneObject != null )
		{
			sceneObject.Flags.CastShadows = true;
		}
	}
}
