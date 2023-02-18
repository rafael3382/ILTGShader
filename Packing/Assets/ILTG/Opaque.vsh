
#ifdef HLSL


#endif
#ifdef GLSL


// <Semantic Name='POSITION' Attribute='a_position' />
// <Semantic Name='COLOR' Attribute='a_color' />
// <Semantic Name='TEXCOORD' Attribute='a_texcoord' />
#define aNormal vec3(1.0);


uniform lowp vec2 u_origin;
uniform lowp mat4 u_viewProjectionMatrix;
uniform highp vec3 u_viewPosition;
uniform lowp vec2 u_fogStartInvLength;
uniform lowp float u_fogYMultiplier;
uniform highp float u_time;
uniform lowp float u_waterDepth;


attribute vec3 a_position;
attribute lowp vec4 a_color;
attribute vec2 a_texcoord;

varying lowp vec4 v_color;
varying mediump vec2 v_texcoord;
varying lowp float v_fog;
varying highp vec3 v_pos;
varying highp vec3 v_screenpos;



#define Mult 1500.0
void main()
{
	// Texture
	v_texcoord = a_texcoord;
	
	v_color = a_color;
	
	
	
	
	
    lowp float useless = u_waterDepth * u_time;
	// Vertex color
	
	
	
	// Fog
	vec3 fogDelta = u_viewPosition - a_position;
	fogDelta.y *= u_fogYMultiplier;
	v_fog = clamp((length(fogDelta) - u_fogStartInvLength.x) * u_fogStartInvLength.y, 0.0, 1.0);
	vec3 sunPos = vec3(a_position.x+sin(u_time*360.0), a_position.y*cos(u_time*360.0), a_position.z);
	// Position,
	
	
	lowp float timeOfDayPi = 2.0 * u_time * 3.1415;
	
    lowp float angle = timeOfDayPi + 3.1415;
    lowp vec3 LightPos = vec3(sin(angle), cos(angle), 0.0);
    
	
	vec4 vtx = vec4(a_position.x - u_origin.x, a_position.y, a_position.z - u_origin.y, 1.0);
	
	
	/*
    vtx.z -= 4.0;
    vtx.xz += fract(u_viewPosition.xz);
    vtx.xz -= vec2(1.0)-fract(u_viewPosition.xz);
    */
    /*
    vec4 vtx = vec4(a_position.x - (u_origin.x + (LightPos.x*u_fogStartInvLength.x)), a_position.y, a_position.z - u_origin.y, 1.0);
    //vtx.xz += fract(u_viewPosition.xz);
    vtx.y -= LightPos.y * 255.0;*/
    
    
	
   
   
    
    if (u_waterDepth > 0.0) {
    
    float elevation = (sin(2.0 * a_position.x + u_time*Mult ) * (cos(1.5 * a_position.z + u_time*Mult) * 0.2)*sin(a_position.z+(u_time*Mult)))*0.1;
    vtx.xz += elevation;
    
    
    }
    
    
    
    gl_Position = u_viewProjectionMatrix * vtx;
    v_pos = a_position.xyz;
    v_screenpos = vtx.xyz;
    
    
    
    
    
    
	// Fix gl_Position
	OPENGL_POSITION_FIX;
}

#endif