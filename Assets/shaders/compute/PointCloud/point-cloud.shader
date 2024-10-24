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
	struct PointData 
	{
		float4 Position;
	};

	struct MeshData 
	{
		float3 Position;
	};

	struct MeshDataUV 
	{
		float2 UV;
	};

	// Settings
	float g_fMeshTransition;


	// Point Buffers
	StructuredBuffer<PointData> g_sbPointBufferSrc<Attribute("SrcPoints");>;
	RWStructuredBuffer<PointData> g_sbPointBufferDst;

	// Source Mesh
	int g_iCachedMeshVerticesSrc;
	float4 g_MeshTransformSrc;

	StructuredBuffer<MeshData> g_sbMeshDataSrc<Attribute("SrcMeshData");>;
	StructuredBuffer<MeshDataUV> g_sbMeshDataTexCoordsSrc<Attribute("SrcMeshDataUV");>;
	
	RWTexture2D<float4> g_tMeshTexSrc;

	// Destination Mesh
	int g_iCachedMeshVerticesDst;
	float4 g_MeshTransformDst;

	StructuredBuffer<MeshData> g_sbMeshDataDst<Attribute("DstMeshData");>;
	StructuredBuffer<MeshDataUV> g_sbMeshDataTexCoordsDst<Attribute("DstMeshDataUV");>;
	
	RWTexture2D<float4> g_tMeshTexDst;

	[numthreads(10,1,1)]
	void MainCs(uint uGroupIndex: SV_GroupIndex, uint3 vThreadId : SV_DispatchThreadID)
	{
		PointData ptSrc = g_sbPointBufferSrc[vThreadId.x];

		// Source Mesh
		float stride = ((float) g_iCachedMeshVerticesSrc) / (float) 1024;
		stride = max(1., stride);
		stride = vThreadId.x * floor(stride);

		float3 cachedPosSrc = g_sbMeshDataSrc[floor(stride)].Position * g_MeshTransformSrc.w + g_MeshTransformSrc.xyz;
		float2 uv = g_sbMeshDataTexCoordsSrc[floor(stride)].UV;

		// Destination Mesh
		stride = ((float) g_iCachedMeshVerticesDst) / (float) 1024;
		stride = max(1., stride);
		stride = vThreadId.x * floor(stride);

		float3 cachedPosDst = g_sbMeshDataDst[floor(stride)].Position * g_MeshTransformDst.w + g_MeshTransformDst.xyz;
		uv = g_sbMeshDataTexCoordsDst[floor(stride)].UV;

		ptSrc.Position.xyz = lerp(cachedPosSrc, cachedPosDst, g_fMeshTransition);

		g_sbPointBufferDst[vThreadId.x] = ptSrc;
	}
}