class ComputeTest : Component
{

	[Property] public SpotLight SpotLight { get; set; }
	
	public ComputeShader Shader { get; set; } = new ComputeShader( "shaders/compute/compute-test.shader" );
	public Texture ComputeTexture { get; set; } = Texture.Create( 512, 512 )
													.WithUAVBinding()
													.WithFormat( ImageFormat.RGBA16161616F )
													.Finish();



	protected override void OnStart()
	{
		base.OnStart();

		Shader.Attributes.Set("OutputTexture", ComputeTexture);
		Shader.Dispatch(ComputeTexture.Width, ComputeTexture.Height);
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		SpotLight.Cookie = ComputeTexture;
	}
}
