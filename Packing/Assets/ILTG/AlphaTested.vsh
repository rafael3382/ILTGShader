#ifdef HLSL

float2 u_origin;
float4x4 u_viewProjectionMatrix;
float3 u_viewPosition;
float2 u_fogStartInvLength;
float u_fogYMultiplier;

void main(
	in float3 a_position: POSITION,
	in float4 a_color: COLOR,
	in float2 a_texcoord: TEXCOORD,
	out float4 v_color : COLOR,
	out float2 v_texcoord : TEXCOORD,
	out float v_fog : FOG,
	out float4 sv_position: SV_POSITION
)
{
	// Texture
	v_texcoord = a_texcoord;

	// Vertex color
	v_color = a_color;
	
	// Fog
	float3 fogDelta = u_viewPosition - a_position;
	fogDelta.y *= u_fogYMultiplier;
	v_fog = saturate((length(fogDelta) - u_fogStartInvLength.x) * u_fogStartInvLength.y);
	
	// Position
	sv_position = mul(float4(a_position.x - u_origin.x, a_position.y, a_position.z - u_origin.y, 1.0), u_viewProjectionMatrix);
}

#endif
#ifdef GLSL


// <Semantic Name='POSITION' Attribute='a_position' />
// <Semantic Name='COLOR' Attribute='a_color' />
// <Semantic Name='TEXCOORD' Attribute='a_texcoord' />

uniform lowp vec2 u_origin;
uniform lowp mat4 u_viewProjectionMatrix;
uniform lowp vec3 u_viewPosition;
uniform lowp vec2 u_fogStartInvLength;
uniform lowp float u_fogYMultiplier;
uniform highp float u_time;
uniform lowp float u_waterDepth;


attribute vec3 a_position;
attribute vec4 a_color;
attribute vec2 a_texcoord;

varying lowp vec4 v_color;
varying highp vec2 v_texcoord;
varying lowp float v_fog;

varying lowp vec3 v_pos;
varying highp vec3 v_screenpos;

#define LEAVES vec2(4.0, 3.0)
#define OTHERLEAVES vec2(4.0, 8.0)
#define IVY vec2(10.0, 0.0)
#define SEAWEED1 vec2(8.0, 6.0)
#define SEAWEED2 vec2(9.0, 6.0)
#define FLOWERS_RANGE_START vec2(12.0, 0.0)
#define FLOWERS_RANGE_END vec2(12.0+3.0, 1.0)
#define FURNACE_RANGE_START vec2(12.0, 2.0)
#define FURNACE_RANGE_END vec2(12.0+3.0, 2.0)
bool is_greater(vec2 a, vec2 b) {
return a.x >= b.x && a.y >= b.y;
}
bool is_less(vec2 a, vec2 b) {
return a.x <= b.x && a.y <= b.y;
}
bool in_range(vec2 a, vec2 range_start, vec2 range_end) {
return is_greater(a, range_start) && is_less(a, range_end);

}
bool is_plant(vec2 uv) {
vec2 block = floor(uv/(1.0/16.0));
if (uv.y > (1.99/16.0) && uv.y <= (3.1/16.0) && block.x >= 12.0 && block.x <= 15.0) return false;
if (block.y == 5.0 && block.x > 4.0) { 
  if (fract((uv-vec2(5.0, 4.0))/(1.0/16.0)).y >= 0.5) {
  	return false;
  }
}
if (block == LEAVES || block == OTHERLEAVES || (block.y == 5.0 && block.x > 4.0) || block == IVY || block == SEAWEED1 || block == SEAWEED2 || in_range(block, FLOWERS_RANGE_START, FLOWERS_RANGE_END)) {
return true;
}


return false;
}


void main()
{
	// Texture
	v_texcoord = a_texcoord;
    float useless2 = u_time + u_waterDepth;
	// Vertex color
	v_color = a_color;
	
	
	// Fog
	vec3 fogDelta = u_viewPosition - a_position;
	fogDelta.y *= u_fogYMultiplier;
	v_fog = clamp((length(fogDelta) - u_fogStartInvLength.x) * u_fogStartInvLength.y, 0.0, 1.0);
	
	
	v_pos = a_position;
	
	// Position
	vec4 vtx = vec4(a_position.x - u_origin.x, a_position.y, a_position.z - u_origin.y, 1.0);
	
	
	
	
	
	if (is_plant(a_texcoord)) {
    float Mult = mix(650.0, 680.0, ((sin(u_time*25.0)+1.0)/2.0)+u_fogYMultiplier);
    float Strength = mix(0.5, 0.54, ((sin(u_time*80.0)+1.0)/2.0)+u_fogYMultiplier);
    vec2 block = floor(a_texcoord/(1.0/16.0));
    if (block != LEAVES && block != OTHERLEAVES)
    {
        Mult = mix(700.0, 720.0, ((sin(u_time*120.0)+1.0)/2.0));
        Strength = mix(0.6, 0.7, ((sin(u_time*140.0)+1.0)/2.0)+u_fogYMultiplier);
    }
	float elevation = (sin(2.0 * a_position.x + u_time*Mult ) * (cos(1.5 * a_position.z + u_time*Mult) * 0.2)*sin(a_position.y+(u_time*Mult))*cos(a_position.y*90.0+(u_time*Mult)))*Strength;
    vtx.xyz += elevation;
    v_pos += elevation;
    }
    v_pos = a_position;
    v_screenpos = vtx.xyz;

    
    gl_Position = u_viewProjectionMatrix * vtx;
	// Fix gl_Position
	OPENGL_POSITION_FIX;
}

#endif