HEADER
{
	Description = "Template Shader for S&box";
}

FEATURES
{
    #include "common/features.hlsl"
}

COMMON
{
	#include "common/shared.hlsl"
}

MODES
{
	Default();
}

CS
{
	RWTexture2D<float4> g_tOutput<Attribute("OutputTexture");>;

	[numthreads(8,8,1)]
	void MainCs(uint uGroupIndex: SV_GroupIndex, uint3 vThreadId : SV_DispatchThreadID)
	{
		g_tOutput[vThreadId.xy] = float4(1, 0, 1, 1);
	}
}