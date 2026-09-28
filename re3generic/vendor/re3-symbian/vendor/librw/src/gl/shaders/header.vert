
//#define DIRECTIONALS
//#define POINTLIGHTS
//#define SPOTLIGHTS

#define ATTRIB_POS	0
#define ATTRIB_NORMAL	1
#define ATTRIB_COLOR	2
#define ATTRIB_WEIGHTS	3
#define ATTRIB_INDICES	4
#define ATTRIB_TEXCOORDS0	5
#define ATTRIB_TEXCOORDS1	6


VSIN(ATTRIB_NORMAL)	vec3 in_normal;
VSIN(ATTRIB_COLOR)	vec4 in_color;
VSIN(ATTRIB_WEIGHTS)	vec4 in_weights;
VSIN(ATTRIB_INDICES)	vec4 in_indices;
VSIN(ATTRIB_TEXCOORDS0)	vec2 in_tex0;
VSIN(ATTRIB_TEXCOORDS1)	vec2 in_tex1;


#ifdef USE_UBOS
layout(std140) uniform State
{
	vec2 u_alphaRef;
	vec4  u_fogData;
	vec4  u_fogColor;
};
#else
uniform vec4 u_alphaRef;
uniform vec4  u_fogData;
uniform vec4  u_fogColor;
#endif

#define u_fogStart (u_fogData.x)
#define u_fogEnd (u_fogData.y)
#define u_fogRange (u_fogData.z)
#define u_fogDisable (u_fogData.w)

#ifdef USE_UBOS
layout(std140) uniform Scene
{
	mat4 u_proj;
	mat4 u_view;
};
#else
uniform mat4 u_proj;
uniform mat4 u_view;
#endif

uniform mat4 u_world;
uniform vec4 u_ambLight;

uniform vec4 u_matColor;
uniform vec4 u_surfProps;	// amb, spec, diff, extra

#define surfAmbient (u_surfProps.x)
#define surfSpecular (u_surfProps.y)
#define surfDiffuse (u_surfProps.z)

float DoFog(float w)
{
	return clamp((w - u_fogEnd)*u_fogRange, u_fogDisable, 1.0);
}
