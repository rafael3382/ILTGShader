// <Semantic Name='POSITION' Attribute='a_position' />
// <Semantic Name='TEXCOORD' Attribute='a_texcoord' />

uniform mat4 u_worldViewProjectionMatrix;
uniform vec2 u_origin;

attribute vec3 a_position;
attribute vec2 a_texcoord;


varying vec2 v_texcoord;



void main()
{
	// Color
	v_color = u_color;

	// Texcoord
	v_texcoord = a_texcoord;


	// Position
	gl_Position = u_worldViewProjectionMatrix * vec4(a_position.xyz-vec3(u_origin.x, 0.0, u_origin.y), 1.0);

	// Fix gl_Position
	OPENGL_POSITION_FIX;
}