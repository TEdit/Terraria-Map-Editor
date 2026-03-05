#if OPENGL
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0
	#define PS_SHADERMODEL ps_4_0
#endif

// Mask texture (bound by SpriteBatch — Alpha8 or Color)
Texture2D SpriteTexture;
sampler2D MaskSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

// World render target (set as shader parameter)
Texture2D WorldTexture;
sampler2D WorldSampler = sampler_state
{
	Texture = <WorldTexture>;
	Filter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};

// SpriteBatch projection matrix
float4x4 MatrixTransform;

float2 ViewportSize;
float DarkenAmount;      // 0.0 = no darkening, 1.0 = fully dark
float DesaturateAmount;  // 0.0 = full color, 1.0 = fully grayscale

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
	float2 ScreenUV : TEXCOORD1;
};

VertexShaderOutput MainVS(VertexShaderInput input)
{
	VertexShaderOutput output;
	output.Position = mul(input.Position, MatrixTransform);
	output.Color = input.Color;
	output.TextureCoordinates = input.TextureCoordinates;

	// Compute screen UV from clip space position: [-1,1] → [0,1]
	// Y is flipped because clip space Y is up but UV Y is down
	output.ScreenUV = float2(
		(output.Position.x + 1.0) * 0.5,
		(1.0 - output.Position.y) * 0.5
	);

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	// Read mask alpha from the SpriteBatch-bound texture
	float mask = tex2D(MaskSampler, input.TextureCoordinates).a * input.Color.a;

	// Read world color from render target using screen UV
	float4 world = tex2D(WorldSampler, input.ScreenUV);

	// BT.601 luminance for desaturation
	float lum = dot(world.rgb, float3(0.299, 0.587, 0.114));
	float3 gray = float3(lum, lum, lum);

	// Desaturate: lerp from original color toward grayscale
	float3 desaturated = lerp(world.rgb, gray, mask * DesaturateAmount);

	// Darken: multiply brightness down
	float3 result = desaturated * (1.0 - mask * DarkenAmount);

	return float4(result, world.a);
}

technique SpriteDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
