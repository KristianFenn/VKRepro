Texture2D<float3> Texture : register(t0);
SamplerState TextureSampler : register(s0)
{
    Texture = (Texture);
    Filter = Anisotropic;
    MaxAnisotropy = 16;
};

Texture2D<float3> NormalMap : register(t1);
SamplerState NormalMapSampler : register(s1)
{
    Texture = (NormalMap);
    Filter = None;
};

float3x3 ModelToViewNormal;
float4x4 ModelToScreen;

struct VertexInput
{
    float4 Position : POSITION;
    float3 Normal : NORMAL;
    float3 Binormal : BINORMAL;
    float3 Tangent : TANGENT;
    float2 TexCoords : TEXCOORD;
};

struct VertexToPixel
{
    float4 Position : SV_Position;
    float2 TexCoords : TEXCOORD0;
    float3x3 TBN : TEXCOORD1;
};

struct PixelBuffers
{
    float3 Albedo : SV_Target0;
    float3 Normal : SV_Target1;
    float Depth : SV_Target2;
};

VertexToPixel VertShader(VertexInput input)
{
    VertexToPixel output;
    
    output.Position = mul(input.Position, ModelToScreen);
    output.TexCoords = input.TexCoords;
    
    output.TBN = float3x3(
        normalize(mul(input.Tangent, ModelToViewNormal)),
        normalize(mul(input.Binormal, ModelToViewNormal)),
        normalize(mul(input.Normal, ModelToViewNormal))
    );
    
    return output;
};

PixelBuffers PixShader(VertexToPixel input)
{
    PixelBuffers output;
    
    output.Albedo = Texture.Sample(TextureSampler, input.TexCoords);
    output.Depth = input.Position.z;
    
    float3 normalSample = mad(NormalMap.Sample(NormalMapSampler, input.TexCoords), 2.0f, -1.0f);
    output.Normal = normalize(mul(normalSample, input.TBN));
   
    return output;
};

technique DrawBuffers
{
    pass p0
    {
        VertexShader = compile vs_6_0 VertShader();
        PixelShader = compile ps_6_0 PixShader();
    }
}