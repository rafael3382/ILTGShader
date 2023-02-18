#ifdef GL_ES
#ifdef GL_FRAGMENT_PRECISION_HIGH
precision highp float;
#else
precision mediump float;
#endif
#endif

// <Semantic Name='POSITION' Attribute='a_position' />
// <Semantic Name='COLOR' Attribute='a_color' />
// <Semantic Name='TEXCOORD' Attribute='a_texcoord' />

uniform vec2 u_origin;
uniform mat4 u_projectionMatrix;
uniform mat4 u_viewMatrix;
uniform mat4 u_projectionMatrixInverted;
uniform vec3 u_viewPosition;
uniform vec2 u_fogStartInvLength;
uniform float u_fogYMultiplier;
uniform float u_time;
uniform float u_waterDepth;

attribute vec3 a_position;
attribute vec4 a_color;
attribute vec2 a_texcoord;

varying vec4 v_color;
varying vec2 v_texcoord;
varying float v_fog;
varying float wavelevel;

varying vec3 v_pos;
varying vec3 viewPos;

#define PI 3.1415926538

void main()
{
	// Texture
	v_texcoord = a_texcoord;
	
	// Vertex color
	vec3 direction = u_viewPosition - a_position;
	float l = length(direction);
	float incidence = abs(direction.y / l);
	float topAlpha = clamp(mix(1.0, 0.65, incidence*1.5), 0.65, 1.0);
	float sideAlpha = 0.85;
	float alpha = mix(topAlpha, sideAlpha, a_color.w);		// Alpha component of a_color encodes whether this is top (0) or side (1) vertex
	v_color = vec4(a_color.xyz * alpha, alpha);
    
	// Fog
	vec3 fogDelta = u_viewPosition - a_position;
	fogDelta.y *= u_fogYMultiplier;
	v_fog = clamp((length(fogDelta) - u_fogStartInvLength.x) * u_fogStartInvLength.y, 0.0, 1.0);
	
	// Position
	vec4 vtx = vec4(a_position.x - u_origin.x, a_position.y, a_position.z - u_origin.y, 1.0);
    float elevation = 0.0;
    
    #ifdef cfg_WaterWaving
    if (abs(v_color.r - v_color.b) > 0.2)
        elevation = 0.05 * sin(2.0 * PI * (u_time*600.0 + a_position.x /  2.5 + a_position.z / 5.0))
				   + 0.05 * sin(2.0 * PI * (u_time*400.0 + a_position.x / 6.0 + a_position.z /  12.0));
    #endif
    wavelevel = elevation;
    
    v_pos = vtx.xyz + vec3(u_origin.x, 0.0, u_origin.y);
	gl_Position = u_projectionMatrix * u_viewMatrix * (vtx - vec4(0.0, elevation, 0.0, 0.0));
    
	vec4 viewPos4 =  u_viewMatrix * vec4(vtx.x, vtx.y, vtx.z, vtx.w);
	viewPos = viewPos4.xyz/viewPos4.w;
	
    // Fix gl_Position
	OPENGL_POSITION_FIX;
}