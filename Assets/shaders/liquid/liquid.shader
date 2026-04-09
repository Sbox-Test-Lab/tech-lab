//=========================================================================================================================

HEADER
{
    Description = "Simple Liquid Shader for S&Box";
}

//=========================================================================================================================

FEATURES
{
    #include "common/features.hlsl"
}

//=========================================================================================================================

MODES
{
	Forward();                                                 // Translucent forward rendering
	Depth( S_MODE_DEPTH );                                     // Depth pass for translucent shadows
	ToolsShadingComplexity("tools_shading_complexity.shader"); // Shows how expensive drawing is in debug view
}

//=========================================================================================================================

COMMON
{
	#define BLEND_MODE_ALREADY_SET 1
	#include "common/shared.hlsl"

	float g_flWobbleX <Attribute("WobbleX");>;
	float g_flWobbleY <Attribute("WobbleY");>;
}

//=========================================================================================================================

struct VertexInput
{
    #include "common/vertexinput.hlsl"
};

//=========================================================================================================================

struct PixelInput
{
    #include "common/pixelinput.hlsl"

	float3 vFillPosition : POSITION < Semantic( PosXyz ); >;
};

//=========================================================================================================================

VS
{
    #include "common/vertex.hlsl"

	// Liquid Fill Level Attributes
	float3 g_vFillCenter < Attribute("FillCenter"); Default3(0,0,0); >;
	float g_flFillHeight < Attribute("FillHeight"); Default(1.0); >;

	float3 RotateAroundX(float3 position, float degrees) 
	{
		degrees = radians(degrees);

		float3x3 mat = 
		{
			1, 0, 0,
			0, cos(degrees), -sin(degrees),
			0, sin(degrees), cos(degrees)	
		};

		return mul(mat, position);
	}

	float3 RotateAroundY(float3 position, float degrees)
	{
		degrees = radians(degrees);

		float3x3 mat = 
		{
			cos(degrees), 0, sin(degrees),
			0, 1, 0,
			-sin(degrees), 0, cos(degrees)	
		};

		return mul(mat, position);
	}
	
	PixelInput MainVs( VertexInput v )
	{
		PixelInput i = ProcessVertex( v );

		float3x4 matObjectToWorld = GetTransformMatrix( v.nInstanceTransformID );
		float3 vWorldDir = mul( matObjectToWorld, float4( v.vPositionOs.xyz - g_vFillCenter, 0.0 ) );
		float3 vPositionWs = vWorldDir / max( g_flFillHeight, 0.001 );
		
		float3 worldPosX = RotateAroundX(vPositionWs, 90) * g_flWobbleX;
		float3 worldPosY = RotateAroundY(vPositionWs, 90) * g_flWobbleY;

		float3 vPositionWsOffset = worldPosX + worldPosY;

		i.vFillPosition = vPositionWs + vPositionWsOffset;

		return FinalizeVertex( i );
	}
}

//=========================================================================================================================

PS
{
	StaticCombo( S_MODE_DEPTH, 0..1, Sys( ALL ) );

	// Z-prepass technique: the depth pass writes front-face-only depth.
	// In the forward pass, back faces automatically fail the depth test
	// wherever a front face exists — but survive where none does (the cap
	// visible when looking down into the glass).
	#if S_MODE_DEPTH
		RenderState( CullMode, BACK );
	#else
		RenderState( CullMode, NONE );
		RenderState( DepthWriteEnable, false );
	#endif

	#define DEPTH_STATE_ALREADY_SET 1
	#define BLEND_MODE_ALREADY_SET 1
	#define S_TRANSLUCENT 1

	#include "common/pixel.hlsl"
	#include "common/classes/Depth.hlsl"

	// Frame buffer copy for refraction (same approach as glass.shader)
	BoolAttribute( bWantsFBCopyTexture, true );
	Texture2D g_tFrameBufferCopyTexture < Attribute("FrameBufferCopyTexture"); SrgbRead( false ); >;

	// Liquid Fill Level Attributes
	float g_flFillAmount < Attribute("FillAmount"); UiType( Slider); Range(0, 1.0); Default(0.5f);>;
	float g_flFoamThickness < Attribute("FoamThickness"); Range(0, 1.0); Default(0.05f);>;
	float3 g_vDirection <Attribute("Direction"); Default3(0,0,-1);>;

	// Liquid Animation Attributes
	float g_flFillWobbleFrequency <Attribute("FillWobbleFrequency"); UiType( Slider); Range(0, 64.0); Default(8); >;
	float g_flFillWobbleAmplitude <Attribute("FillWobbleAmplitude"); UiType( Slider); Range(0, 1.0); Default(0.1); >;

	// Liquid Color Attributes
	float3 g_vFoamColor <Attribute("FillColorFoam"); UiType(Color); Default3(0, 0.6, 0.7);>;
	float3 g_vFillColorUpper < Attribute("FillColorUpper"); UiType(Color); Default3(0, 0.5, 0.5);>;
	float3 g_vFillColorLower < Attribute("FillColorLower"); UiType(Color); Default3(0, 0, 1);>;

	// Rim Lighting Attributes
	float g_flRimStrengthPower <Attribute("RimLightStrengthPower"); UiType(Slider); Default(2);>;

	// Refraction Attributes
	float g_flRefractionStrength < Attribute("RefractionStrength"); Default(1.01); Range(1.0, 1.1); UiType(Slider); >;
	float g_flOpacity < Attribute("Opacity"); Default(0.4); Range(0.0, 1.0); UiType(Slider); >;
	float g_flBlurAmount < Attribute("BlurAmount"); Default(0.0); Range(0.0, 1.0); UiType(Slider); >;

	float4 MainPs(PixelInput i, bool isFrontFace : SV_IsFrontFace) : SV_Target0
	{
		// Fill calculation
		float wobbleIntensity = abs(g_flWobbleX) + abs(g_flWobbleY);
		float wobble = sin((i.vFillPosition.x * g_flFillWobbleFrequency) + (i.vFillPosition.y * g_flFillWobbleFrequency) + (g_flTime)) * (g_flFillWobbleAmplitude * wobbleIntensity);

		float fillPosition = i.vFillPosition.z + wobble;
		float fillEdge = (2 * g_flFillAmount) - 1.0f;
		float fillAmount = step(fillPosition, fillEdge);

		clip(fillAmount > 0 ? 1 : -1);

		// Back faces: only keep a thin band at the fill edge (the cap).
		// Everything below is clipped so back faces can never fight front faces.
		if (!isFrontFace)
		{
			float distFromEdge = fillEdge - fillPosition;
			clip(distFromEdge < g_flFoamThickness ? 1 : -1);
		}

		// Depth pass
		#if S_MODE_DEPTH
		{
			return 1;
		}
		#endif

		// Flip normal for back faces so the cap surface lights correctly
		float3 vNormal = normalize(i.vNormalWs);
		if (!isFrontFace)
			vNormal = -vNormal;

		// Liquid tint color (gradient from upper to lower)
		float3 fillColor = lerp(g_vFillColorUpper, g_vFillColorLower, fillPosition);

		// View direction and fresnel
		float3 vViewRayWs = normalize(i.vPositionWithOffsetWs.xyz);
		float flNDotV = saturate(dot(-vNormal, vViewRayWs));
		float fresnel = pow(1.0 - flNDotV, g_flRimStrengthPower);

		// Refraction via frame buffer copy
		float flDepthPs = 1.0f - Depth::GetNormalized(i.vPositionSs.xy);
		float3 vRefractionWs = RecoverWorldPosFromProjectedDepthAndRay(flDepthPs, vViewRayWs) - g_vCameraPositionWs;
		float flDistanceVs = distance(i.vPositionWithOffsetWs.xyz, vRefractionWs);

		float3 vRefractRayWs = refract(vViewRayWs, vNormal, 1.0 / g_flRefractionStrength);
		float3 vRefractWorldPosWs = i.vPositionWithOffsetWs.xyz + vRefractRayWs * flDistanceVs;

		float4 vPositionPs = Position4WsToPs(float4(vRefractWorldPosWs, 0));
		float2 vScreenUv = vPositionPs.xy / vPositionPs.w;
		vScreenUv = vScreenUv * 0.5 + 0.5;
		vScreenUv.y = 1.0 - vScreenUv.y;

		float flBlur = g_flBlurAmount * (1.0 - (1.0 / max(flDistanceVs, 0.001)));
		flBlur /= max(flNDotV, 0.01);
		const int nNumMips = 7;

		float2 vUV = vScreenUv * g_vFrameBufferCopyInvSizeAndUvScale.zw;
		float3 vRefractedColor = g_tFrameBufferCopyTexture.SampleLevel(g_sTrilinearMirror, vUV, sqrt(flBlur) * nNumMips).xyz;

		if (isFrontFace)
		{
			// Front face: refracted scene blended with liquid tint
			float3 bodyColor = lerp(vRefractedColor, fillColor, g_flOpacity) + fresnel;

			// Foam band at the fill edge
			float foamLine = step(fillPosition, fillEdge) - step(fillPosition, fillEdge - g_flFoamThickness);
			bodyColor = lerp(bodyColor, g_vFoamColor, foamLine);

			return float4(bodyColor, 1.0);
		}
		else
		{
			// Back face cap: solid foam surface (only renders near fill edge due to clip above)
			return float4(g_vFoamColor, 1.0);
		}
	}
}