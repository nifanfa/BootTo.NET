const char *header_vert_src =

"//#define DIRECTIONALS\n"
"//#define POINTLIGHTS\n"
"//#define SPOTLIGHTS\n"

"#define ATTRIB_POS	0\n"
"#define ATTRIB_NORMAL	1\n"
"#define ATTRIB_COLOR	2\n"
"#define ATTRIB_WEIGHTS	3\n"
"#define ATTRIB_INDICES	4\n"
"#define ATTRIB_TEXCOORDS0	5\n"
"#define ATTRIB_TEXCOORDS1	6\n"


"VSIN(ATTRIB_NORMAL)	vec3 in_normal;\n"
"VSIN(ATTRIB_COLOR)	vec4 in_color;\n"
"VSIN(ATTRIB_WEIGHTS)	vec4 in_weights;\n"
"VSIN(ATTRIB_INDICES)	vec4 in_indices;\n"
"VSIN(ATTRIB_TEXCOORDS0)	vec2 in_tex0;\n"
"VSIN(ATTRIB_TEXCOORDS1)	vec2 in_tex1;\n"


"#ifdef USE_UBOS\n"
"layout(std140) uniform State\n"
"{\n"
"	vec2 u_alphaRef;\n"
"	vec4  u_fogData;\n"
"	vec4  u_fogColor;\n"
"};\n"
"#else\n"
"uniform vec4 u_alphaRef;\n"
"uniform vec4  u_fogData;\n"
"uniform vec4  u_fogColor;\n"
"#endif\n"

"#define u_fogStart (u_fogData.x)\n"
"#define u_fogEnd (u_fogData.y)\n"
"#define u_fogRange (u_fogData.z)\n"
"#define u_fogDisable (u_fogData.w)\n"

"#ifdef USE_UBOS\n"
"layout(std140) uniform Scene\n"
"{\n"
"	mat4 u_proj;\n"
"	mat4 u_view;\n"
"};\n"
"#else\n"
"uniform mat4 u_proj;\n"
"uniform mat4 u_view;\n"
"#endif\n"

"uniform mat4 u_world;\n"
"uniform vec4 u_ambLight;\n"

"uniform vec4 u_matColor;\n"
"uniform vec4 u_surfProps;	// amb, spec, diff, extra\n"

"#define surfAmbient (u_surfProps.x)\n"
"#define surfSpecular (u_surfProps.y)\n"
"#define surfDiffuse (u_surfProps.z)\n"

"float DoFog(float w)\n"
"{\n"
"	return clamp((w - u_fogEnd)*u_fogRange, u_fogDisable, 1.0);\n"
"}\n"
;
