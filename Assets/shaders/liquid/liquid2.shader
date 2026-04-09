//=========================================================================================================================
// Combined Glass + Liquid Shader (Alyx Approach)
// The entire effect lives on the bottle/glass mesh - nothing is inside it.
// Above the fill line the surface looks like glass; below it looks like liquid.
// The liquid cap is a front-face visual effect based on view angle + fill proximity.
//=========================================================================================================================

HEADER
{
	Description = "Combined Glass + Liquid Shader for S&Box";
}

//=========================================================================================================================

FEATURES
{
	#include "common/features.hlsl"
}

//=========================================================================================================================

MODES
{
	Forward();
	Depth( S_MODE_DEPTH );
	ToolsShadingComplexity("tools_shading_complexity.shader");
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

	// Front faces only - the cap is a front-face visual effect.
	RenderState( CullMode, BACK );

	#if !S_MODE_DEPTH
		RenderState( DepthWriteEnable, false );
	#endif

	#define DEPTH_STATE_ALREADY_SET 1
	#define BLEND_MODE_ALREADY_SET 1
	#define S_TRANSLUCENT 1

	#include "common/pixel.hlsl"
	#include "common/classes/Depth.hlsl"

	// Frame buffer copy for refraction
	BoolAttribute( bWantsFBCopyTexture, true );
	Texture2D g_tFrameBufferCopyTexture < Attribute("FrameBufferCopyTexture"); SrgbRead( false ); >;
	// -- Fill --
	float g_flFillAmount < Attribute("FillAmount"); UiType( Slider); Range(0, 1.0); Default(0.5f); >;
	float g_flContainerBottom < Attribute("ContainerBottom"); UiType( Slider); Range(0, 1.0); Default(0.0f); >;
	float g_flFoamThickness < Attribute("FoamThickness"); Range(0, 1.0); Default(0.05f); >;
	float3 g_vFillDirection < Attribute("FillDirection"); Default3(0, 0, 1); >;

	// -- Liquid Animation --
	float g_flFillWobbleFrequency < Attribute("FillWobbleFrequency"); UiType( Slider); Range(0, 64.0); Default(8); >;
	float g_flFillWobbleAmplitude < Attribute("FillWobbleAmplitude"); UiType( Slider); Range(0, 1.0); Default(0.1); >;

	// -- Liquid Colors --
	float3 g_vFoamColor < Attribute("FillColorFoam"); UiType(Color); Default3(1, 0.95, 0.8); >;
	float3 g_vFillColorUpper < Attribute("FillColorUpper"); UiType(Color); Default3(0.9, 0.55, 0.05); >;
	float3 g_vFillColorLower < Attribute("FillColorLower"); UiType(Color); Default3(0.5, 0.2, 0.02); >;
	float g_flLiquidOpacity < Attribute("LiquidOpacity"); Default(0.7); Range(0.0, 1.0); UiType(Slider); >;
	// -- Glass --
	float3 g_vGlassTint < Attribute("GlassTint"); UiType(Color); Default3(1, 1, 1); >;
	float g_flGlassOpacity < Attribute("GlassOpacity"); Default(0.1); Range(0.0, 1.0); UiType(Slider); >;
	float g_flGlassRoughness < Attribute("GlassRoughness"); Default(0.05); Range(0.0, 1.0); UiType(Slider); >;

	// -- Rim Lighting --
	float g_flRimStrengthPower < Attribute("RimLightStrengthPower"); UiType(Slider); Default(2); >;

	// -- Label / Opacity Mask (Alyx-style) --
	// Opacity mask defines opaque regions (labels, stickers) that block the glass/liquid effect.
	// When no mask is set (HasOpacityMask = 0), the shader behaves as pure glass+liquid.
	Texture2D g_tLabelColor < Attribute("LabelColor"); SrgbRead( true ); >;
	Texture2D g_tOpacityMask < Attribute("OpacityMask"); SrgbRead( false ); >;
	float g_flHasOpacityMask < Attribute("HasOpacityMask"); Default(0); >;

	// -- Refraction (shared) --
	float g_flRefractionStrength < Attribute("RefractionStrength"); Default(1.01); Range(1.0, 1.1); UiType(Slider); >;
	float g_flBlurAmount < Attribute("BlurAmount"); Default(0.0); Range(0.0, 1.0); UiType(Slider); >;
	// Schlick fresnel approximation (F0 = 0.04 for glass IOR ~1.5)
	float SchlickFresnel(float flNDotV)
	{
		return 0.04 + 0.96 * pow(1.0 - flNDotV, 5.0);
	}

	// Sample the refracted scene through the frame buffer copy.
	float3 SampleRefraction(PixelInput i, float3 vNormal, float3 vViewRayWs, float flNDotV, float flRoughness)
	{
		float flDepthPs = 1.0f - Depth::GetNormalized(i.vPositionSs.xy);
		float3 vRefractionWs = RecoverWorldPosFromProjectedDepthAndRay(flDepthPs, vViewRayWs) - g_vCameraPositionWs;
		float flDistanceVs = distance(i.vPositionWithOffsetWs.xyz, vRefractionWs);

		float3 vRefractRayWs = refract(vViewRayWs, vNormal, 1.0 / g_flRefractionStrength);
		float3 vRefractWorldPosWs = i.vPositionWithOffsetWs.xyz + vRefractRayWs * flDistanceVs;

		float4 vPositionPs = Position4WsToPs(float4(vRefractWorldPosWs, 0));
		float2 vScreenUv = vPositionPs.xy / vPositionPs.w;
		vScreenUv = vScreenUv * 0.5 + 0.5;
		vScreenUv.y = 1.0 - vScreenUv.y;

		float flBlur = g_flBlurAmount * flRoughness * (1.0 - (1.0 / max(flDistanceVs, 0.001)));
		flBlur /= max(flNDotV, 0.01);
		const int nNumMips = 7;

		float2 vUV = vScreenUv * g_vFrameBufferCopyInvSizeAndUvScale.zw;
		return g_tFrameBufferCopyTexture.SampleLevel(g_sTrilinearMirror, vUV, sqrt(flBlur) * nNumMips).xyz;
	}
	float4 MainPs(PixelInput i) : SV_Target0
	{
		// -- Fill calculation --
		float wobbleIntensity = abs(g_flWobbleX) + abs(g_flWobbleY);
		float wobble = sin((i.vFillPosition.x * g_flFillWobbleFrequency) + (i.vFillPosition.y * g_flFillWobbleFrequency) + (g_flTime)) * (g_flFillWobbleAmplitude * wobbleIntensity);

		float fillPosition = i.vFillPosition.z + wobble;
		float clipZ = (2 * g_flContainerBottom) - 1.0;
		float fillEdge = lerp(clipZ, 1.0, g_flFillAmount);
		bool isLiquid = fillPosition <= fillEdge && fillPosition >= clipZ;

		float3 vNormal = normalize(i.vNormalWs);
		float3 vViewRayWs = normalize(i.vPositionWithOffsetWs.xyz);
		float flNDotV = saturate(dot(-vNormal, vViewRayWs));

		// -- Depth pass --
		#if S_MODE_DEPTH
		{
			return 1;
		}
		#endif

		// Physically-based fresnel
		float fresnel = SchlickFresnel(flNDotV);

		// Fill direction in world space (up axis of the glass)
		float3 vFillDirWs = normalize(g_vFillDirection);
		float3 surfaceColor;
		if (isLiquid)
		{
			// -- LIQUID REGION --
			// Render as glass surface with liquid color tinted through the refraction.
			// From outside, the surface always looks like glass - you see liquid color "behind" it.
			float3 vRefractedColor = SampleRefraction(i, vNormal, vViewRayWs, flNDotV, 0.05);

			// Surface orientation relative to fill direction
			// Used to gently suppress effects on surfaces directly facing the fill direction (e.g. flat rims)
			// Squared falloff keeps the effect visible on sloped geometry like cone/martini shapes
			float normalDotFill = dot(vNormal, vFillDirWs);
			float surfaceFactor = saturate(1.0 - saturate(normalDotFill) * saturate(normalDotFill));

			// Liquid color gradient (bottom to top of fill, normalized within container)
			float containerHeight = max(fillEdge - clipZ, 0.001);
			float fillT = saturate((fillPosition - clipZ) / containerHeight);
			float3 fillColor = lerp(g_vFillColorLower, g_vFillColorUpper, fillT);

			// How much the viewer is looking down into the container
			float lookDownFactor = saturate(dot(-vViewRayWs, vFillDirWs));

			// View-angle thickness: at grazing angles light passes through more liquid
			float viewThickness = lerp(1.0, 1.5, 1.0 - flNDotV);

			// Depth-based absorption
			// Boost absorption when looking from above so deeper liquid looks opaque
			float depth = saturate((fillEdge - fillPosition) / containerHeight);
			float topDownBoost = lerp(1.0, 1.5, lookDownFactor);
			float flAbsorption = saturate(g_flLiquidOpacity * viewThickness * topDownBoost + depth * 0.5);

			// At grazing angles fresnel reflection dominates so less light enters the liquid
			// This prevents silhouette edges from appearing as solid opaque liquid
			flAbsorption *= (1.0 - fresnel * 0.7);

			// Tint the refracted scene with liquid color
			float3 tintedColor = lerp(vRefractedColor, fillColor, flAbsorption);

			// Apply glass-like fresnel + environment reflection (same as glass region)
			float3 vReflectWs = reflect(vViewRayWs, vNormal);
			float skyBlend = saturate(vReflectWs.z * 0.5 + 0.5);
			float3 envColor = lerp(float3(0.08, 0.08, 0.06), float3(0.35, 0.45, 0.65), skyBlend);
			float3 bodyColor = tintedColor * (1.0 - fresnel) + envColor * fresnel;

			// -- CAP (liquid surface visible near fill edge when looking into the container) --
			// Cap extent grows when looking from above so the liquid surface covers the full opening
			// Use 2x containerHeight so smoothstep never fully reaches 1.0 within the liquid depth
			float capDepthExtent = lerp(g_flFoamThickness * 6.0, containerHeight * 2.0, lookDownFactor * lookDownFactor);
			float edgeProximity = 1.0 - smoothstep(0.0, capDepthExtent, fillEdge - fillPosition);

			float capMask = saturate(edgeProximity * lookDownFactor * 4.0);

			// Cap surface color: blend of upper liquid color and foam
			// Add subtle depth variation so the cap isn't perfectly flat
			float capDepthFade = saturate((fillEdge - fillPosition) / max(containerHeight * 0.5, 0.001));
			float3 capColor = lerp(lerp(g_vFillColorUpper, g_vFoamColor, 0.5), g_vFillColorUpper, capDepthFade * 0.4);
			bodyColor = lerp(bodyColor, capColor, capMask);

			// -- FOAM BAND at fill edge --
			float foamLine = smoothstep(fillEdge - g_flFoamThickness, fillEdge, fillPosition)
							 * smoothstep(fillEdge + g_flFoamThickness * 0.2, fillEdge, fillPosition)
							 * surfaceFactor;
			bodyColor = lerp(bodyColor, g_vFoamColor, foamLine);

			surfaceColor = bodyColor;
		}
		else
		{
			// -- GLASS REGION --
			float3 vRefractedColor = SampleRefraction(i, vNormal, vViewRayWs, flNDotV, g_flGlassRoughness);

			// Glass tint (Beer's law through colored glass)
			float3 glassTinted = vRefractedColor * lerp(float3(1,1,1), g_vGlassTint, g_flGlassOpacity);

			// Environment reflection approximation from reflect direction
			float3 vReflectWs = reflect(vViewRayWs, vNormal);
			float skyBlend = saturate(vReflectWs.z * 0.5 + 0.5);
			float3 envColor = lerp(float3(0.08, 0.08, 0.06), float3(0.35, 0.45, 0.65), skyBlend);

			// Energy-conserving glass: refraction where transparent, reflection at edges
			float3 glassColor = glassTinted * (1.0 - fresnel) + envColor * fresnel;

			surfaceColor = glassColor;
		}

		// -- Opacity mask: Alyx-style opaque label regions --
		// When a mask is provided, opaque areas (labels, stickers) override the transparent surface.
		// Mask value 1 = fully opaque label, 0 = transparent glass/liquid (no change).
		if (g_flHasOpacityMask > 0.5)
		{
			float2 vTexCoords = i.vTextureCoords.xy;
			float flOpacityMask = g_tOpacityMask.Sample(g_sAniso, vTexCoords).r;

			if (flOpacityMask > 0.01)
			{
				float3 labelAlbedo = g_tLabelColor.Sample(g_sAniso, vTexCoords).rgb;

				// Half-lambert diffuse from view direction with ambient base
				float labelNDotL = dot(vNormal, -vViewRayWs) * 0.5 + 0.5;
				float3 labelLit = labelAlbedo * labelNDotL;

				// Rim light for depth
				float rim = pow(1.0 - flNDotV, g_flRimStrengthPower) * 0.15;
				labelLit += rim;

				surfaceColor = lerp(surfaceColor, labelLit, flOpacityMask);
			}
		}

		return float4(surfaceColor, 1.0);
	}
}