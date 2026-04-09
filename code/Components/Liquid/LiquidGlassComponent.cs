using System;
using Sandbox;

public sealed class LiquidGlassComponent : Component, Component.ExecuteInEditor
{
	[Property, Range( 0, 1 )] public float FillAmount { get; set; } = 0.5f;
	[Property, Range( 0, 1 )] public float ContainerBottom { get; set; } = 0f;
	[Property, Range( 0, 0.5f )] public float FoamThickness { get; set; } = 0.05f;

	[Property, Group( "Liquid" )] public Color FillColorFoam { get; set; } = new Color( 1.0f, 0.95f, 0.8f );
	[Property, Group( "Liquid" )] public Color FillColorUpper { get; set; } = new Color( 0.9f, 0.55f, 0.05f );
	[Property, Group( "Liquid" )] public Color FillColorLower { get; set; } = new Color( 0.5f, 0.2f, 0.02f );
	[Property, Group( "Liquid" ), Range( 0, 1 )] public float LiquidOpacity { get; set; } = 0.7f;

	[Property, Group( "Glass" )] public Color GlassTint { get; set; } = Color.White;
	[Property, Group( "Glass" ), Range( 0, 1 )] public float GlassOpacity { get; set; } = 0.1f;
	[Property, Group( "Glass" ), Range( 0, 1 )] public float GlassRoughness { get; set; } = 0.05f;

	[Property, Group( "Label" )] public Texture OpacityMask { get; set; }
	[Property, Group( "Label" )] public Texture LabelColor { get; set; }

	[Property, Range( 0, 8 ), Step( 0.1f )] public float RimLightStrengthPower { get; set; } = 2;

	[Property, Range( 0, 0.1f )] public float MaxWobble { get; set; } = 0.004f;

	[Property, Range( 0, 64 ), Step( 0.25f )] public float WobbleFrequency { get; set; } = 4f;
	[Property, Range( 0, 8 ), Step( 0.1f )] public float WobbleAmplitude { get; set; } = 0.05f;

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

		var pulse = 2f * (float)Math.PI * 1f;

		var wobbleAmountX = WobbleAmountAddX * (float)Math.Sin( pulse * BobTime );
		var wobbleAmountY = WobbleAmountAddY * (float)Math.Sin( pulse * BobTime );

		model.Batchable = false;

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

		// Fill
		model.Attributes.Set( "FillAmount", FillAmount );
		model.Attributes.Set( "ContainerBottom", ContainerBottom );
		model.Attributes.Set( "FoamThickness", FoamThickness );

		// Liquid colors
		model.Attributes.Set( "FillColorFoam", FillColorFoam );
		model.Attributes.Set( "FillColorUpper", FillColorUpper );
		model.Attributes.Set( "FillColorLower", FillColorLower );
		model.Attributes.Set( "LiquidOpacity", LiquidOpacity );

		// Glass
		model.Attributes.Set( "GlassTint", GlassTint );
		model.Attributes.Set( "GlassOpacity", GlassOpacity );
		model.Attributes.Set( "GlassRoughness", GlassRoughness );
		model.Attributes.Set( "FillDirection", WorldRotation.Up );

		// Label / Opacity Mask
		if ( OpacityMask != null )
		{
			model.Attributes.Set( "OpacityMask", OpacityMask );
			model.Attributes.Set( "HasOpacityMask", 1.0f );

			if ( LabelColor != null )
				model.Attributes.Set( "LabelColor", LabelColor );
		}
		else
		{
			model.Attributes.Set( "HasOpacityMask", 0.0f );
		}

		// Shared
		model.Attributes.Set( "RimLightStrengthPower", RimLightStrengthPower );
		model.Attributes.Set( "RefractionStrength", RefractionStrength );
		model.Attributes.Set( "BlurAmount", BlurAmount );

		// Wobble
		model.Attributes.Set( "WobbleX", wobbleAmountX );
		model.Attributes.Set( "WobbleY", wobbleAmountY );
		model.Attributes.Set( "FillWobbleFrequency", WobbleFrequency );
		model.Attributes.Set( "FillWobbleAmplitude", WobbleAmplitude );

		// Wobble physics
		var velocity = (LastPosition - WorldPosition) / Time.Delta;
		var angularVelocity = WorldRotation.Angles().AsVector3() - LastRotation;

		WobbleAmountAddX += MathX.Clamp( (velocity.y + (angularVelocity.x * 1)) * MaxWobble, -MaxWobble, MaxWobble );
		WobbleAmountAddY += MathX.Clamp( (velocity.x + (angularVelocity.y * 1)) * MaxWobble, -MaxWobble, MaxWobble );

		LastPosition = WorldPosition;
		LastRotation = WorldRotation.Angles().AsVector3();
	}
}
