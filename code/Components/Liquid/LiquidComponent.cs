using System;
using Sandbox;

public sealed class LiquidComponent : Component, Component.ExecuteInEditor
{
	[Property, Range(0, 1)] public float FillAmount { get; set; } = 0.5f;
	[Property, Range( 0, 0.5f )] public float FoamThickness { get; set; } = 0.05f;

	[Property] public Color FillColorFoam { get; set; } = new Color( 0, 0.6f, 0.7f );
	[Property] public Color FillColorUpper { get; set; } = new Color( 0.0f, 0.5f, 0.5f );
	[Property] public Color FillColorLower { get; set; } = new Color( 0.0f, 0.0f, 1.0f);

	[Property, Range(0, 8), Step(0.1f)] public float RimLightStrengthPower { get; set; } = 2;

	[Property, Range(0, 0.1f)] public float MaxWobble { get; set; } = 0.004f;

	[Property, Range( 0, 64 ), Step(0.25f)] public float WobbleFrequency { get; set; } = 4f;
	[Property, Range( 0, 8 ), Step(0.1f)] public float WobbleAmplitude { get; set; } = 0.05f;

	[Property, Group( "Refraction" ), Range( 0, 1 )] public float Opacity { get; set; } = 0.4f;
	[Property, Group( "Refraction" ), Range( 1.0f, 1.1f )] public float RefractionStrength { get; set; } = 1.01f;
	[Property, Group( "Refraction" ), Range( 0, 1 )] public float BlurAmount { get; set; } = 0f;

	float BobTime { get; set; } = 0.75f;

	float WobbleAmountAddX;
	float WobbleAmountAddY;

	Vector3 LastPosition;
	Vector3 LastRotation;

	protected override void OnPreRender()
	{
		base.OnPreRender();

		var renderer = GameObject.Components.Get<ModelRenderer>();
		if ( renderer?.SceneObject is not SceneObject model )
			return;

		BobTime += Time.Delta;

		WobbleAmountAddX = MathX.Lerp( WobbleAmountAddX, 0, Time.Delta * 1 );
		WobbleAmountAddY = MathX.Lerp( WobbleAmountAddY, 0, Time.Delta * 1 );

		var pulse = 2f * (float) Math.PI * 1f;

		var wobbleAmountX = WobbleAmountAddX * (float) Math.Sin( pulse * BobTime );
		var wobbleAmountY = WobbleAmountAddY * (float) Math.Sin( pulse * BobTime );

		model.Batchable = false;

		// Z-prepass writes front-face depth so the forward pass can resolve
		// front vs back face ordering via the depth buffer
		model.Flags.WantsPrePass = true;
		model.Flags.IsTranslucent = true;
		model.Flags.WantsFrameBufferCopy = true;

		// Auto-calculate fill height and center from model bounds
		if ( renderer.Model != null )
		{
			var bounds = renderer.Model.Bounds;
			model.Attributes.Set( "FillCenter", bounds.Center );
			model.Attributes.Set( "FillHeight", bounds.Size.z * 0.5f * WorldScale.z );
		}

		model.Attributes.Set( "Direction", WorldRotation.Up );

		model.Attributes.Set( "FillAmount", FillAmount );

		model.Attributes.Set( "FoamThickness", FoamThickness );
		model.Attributes.Set( "FillColorFoam", FillColorFoam );
		model.Attributes.Set( "FillColorUpper", FillColorUpper );
		model.Attributes.Set( "FillColorLower", FillColorLower );

		model.Attributes.Set( "RimLightStrengthPower", RimLightStrengthPower );

		model.Attributes.Set( "WobbleX", wobbleAmountX );
		model.Attributes.Set( "WobbleY", wobbleAmountY );

		model.Attributes.Set( "FillWobbleFrequency", WobbleFrequency );
		model.Attributes.Set( "FillWobbleAmplitude", WobbleAmplitude );

		// Refraction
		model.Attributes.Set( "RefractionStrength", RefractionStrength );
		model.Attributes.Set( "Opacity", Opacity );
		model.Attributes.Set( "BlurAmount", BlurAmount );

		var velocity = (LastPosition - WorldPosition) / Time.Delta;
		var angularVelocity = WorldRotation.Angles().AsVector3() - LastRotation;

		WobbleAmountAddX += MathX.Clamp( (velocity.y + (angularVelocity.x * 1)) * MaxWobble, -MaxWobble, MaxWobble );
		WobbleAmountAddY += MathX.Clamp( (velocity.x + (angularVelocity.y * 1)) * MaxWobble, -MaxWobble, MaxWobble );

		LastPosition = WorldPosition;
		LastRotation = WorldRotation.Angles().AsVector3();
	}
		
}




