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

bool UseNormalMap;
float3 LightDirection;
float4x4 ModelToView;
float3x3 ModelToViewNormal;
float4x4 Projection;

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
    float4 ViewPosition : TEXCOORD0;
    float3 Normal : TEXCOORD1;
    float2 TexCoords : TEXCOORD2;
    float3x3 TBN : TEXCOORD3;
};

VertexToPixel VertShader(VertexInput input)
{
    VertexToPixel output;
    
    output.ViewPosition = mul(input.Position, ModelToView);
    output.Position = mul(output.ViewPosition, Projection);
    output.TexCoords = input.TexCoords;
    output.Normal = input.Normal;
    output.TBN = float3x3(
        normalize(mul(input.Tangent, ModelToViewNormal)),
        normalize(mul(input.Binormal, ModelToViewNormal)),
        normalize(mul(input.Normal, ModelToViewNormal))
    );  
    
    return output;
};

float4 PixShaderNormalMap(VertexToPixel input) : SV_Target
{
    float3 color = Texture.Sample(TextureSampler, input.TexCoords);
    float3 normal = input.Normal;
    
    if (UseNormalMap == true)
    {
        float3 normalSample = mad(NormalMap.Sample(NormalMapSampler, input.TexCoords), 2.0f, -1.0f);
        normal = normalize(mul(normalSample, input.TBN));
    }
    
    float3 lightDir = normalize(LightDirection);
    float3 viewDir = -normalize(input.ViewPosition.xyz);
    float3 halfVector = normalize(lightDir + viewDir);
    
    float3 diffuseColor = (max(dot(normal, lightDir), 0) + 0.3f) * color;
    
    float3 specularColor = pow(max(dot(normal, halfVector), 0), 16) * float3(1.xxx);
    
    return float4(diffuseColor + specularColor, 1);
};

float4 PixShaderVertexNormal(VertexToPixel input) : SV_Target
{
    float3 color = Texture.Sample(TextureSampler, input.TexCoords);
    float3 normal = input.Normal;    
    float3 lightDir = normalize(LightDirection);
    float3 viewDir = -normalize(input.ViewPosition.xyz);
    float3 halfVector = normalize(lightDir + viewDir);
    
    float3 diffuseColor = (max(dot(normal, lightDir), 0) + 0.3f) * color;
    
    float3 specularColor = pow(max(dot(normal, halfVector), 0), 16) * float3(1.xxx);
    
    return float4(diffuseColor + specularColor, 1);
};

technique DrawNormalMap
{
    pass p0
    {
        VertexShader = compile vs_6_0 VertShader();
        PixelShader = compile ps_6_0 PixShaderNormalMap();
    }
}

technique DrawVertexNormal
{
    pass p0
    {
        VertexShader = compile vs_6_0 VertShader();
        PixelShader = compile ps_6_0 PixShaderVertexNormal();
    }
}