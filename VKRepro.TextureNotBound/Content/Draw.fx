Texture2D<float3> Albedo : register(t0);
SamplerState AlbedoSampler : register(s0)
{
    Texture = (Albedo);
    Filter = None;
};

Texture2D<float3> Normal : register(t1);
SamplerState NormalSampler : register(s1)
{
    Texture = (Normal);
    Filter = None;
};

Texture2D<float> Depth : register(t2);
SamplerState DepthSampler : register(s2)
{
    Texture = (Depth);
    Filter = None;
};

struct VertexInput
{
    float4 Position : POSITION;
    float2 TexCoords : TEXCOORD;
};

struct VertexToPixel
{
    float4 Position : SV_Position;
    float2 TexCoords : TEXCOORD;
};

float3 LightDirection;
float4x4 ScreenSpaceToView;

VertexToPixel VertShader(VertexInput input)
{
    VertexToPixel output;
    
    output.Position = input.Position;
    output.TexCoords = input.TexCoords;
    
    return output;
}

float4 PixShader(VertexToPixel input) : SV_Target
{
    float3 color = Albedo.Sample(AlbedoSampler, input.TexCoords);
    float3 normal = Normal.Sample(NormalSampler, input.TexCoords);
    float depth = Depth.Sample(DepthSampler, input.TexCoords);
    
    float3 lightDir = normalize(LightDirection);
    
    float4 viewPos = mul(float4(input.TexCoords, depth, 1.0f), ScreenSpaceToView);
    viewPos.xyz = viewPos.xyz / viewPos.w;
    
    float3 viewDir = -normalize(viewPos.xyz);
    float3 halfVector = normalize(lightDir + viewDir);
    
    float3 diffuseColor = (max(dot(normal, lightDir), 0) + 0.3f) * color;
    
    float3 specularColor = pow(max(dot(normal, halfVector), 0), 16) * float3(1.xxx);
    
    return float4(diffuseColor + specularColor, 1);
};

float4 PixShaderTexCoords(VertexToPixel input) : SV_Target
{
    return float4(input.TexCoords, 0, 1);
}

technique Draw
{
    pass p0
    {
        VertexShader = compile vs_6_0 VertShader();
        PixelShader = compile ps_6_0 PixShader();
    }
}

technique DrawTexCoords
{
    pass p0
    {
        VertexShader = compile vs_6_0 VertShader();
        PixelShader = compile ps_6_0 PixShaderTexCoords();
    }
}