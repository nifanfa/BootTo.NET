uniform sampler2D tex0;
uniform sampler2D tex1;

uniform vec4 u_fxparams;

#define shininess (u_fxparams.x)
#define disableFBA (u_fxparams.y)

FSIN vec4 v_color;
FSIN vec4 v_envColor;
FSIN vec3 v_tex0_fog;
FSIN vec2 v_tex1;

void
main(void)
{
	vec4 pass1 = v_color;
	pass1 *= texture(tex0, vec2(v_tex0_fog.x, 1.0-v_tex0_fog.y));

	vec4 pass2 = v_envColor*shininess*texture(tex1, vec2(v_tex1.x, 1.0-v_tex1.y));

	pass1.rgb = mix(u_fogColor.rgb, pass1.rgb, v_tex0_fog.z);
	pass2.rgb = mix(vec3(0.0, 0.0, 0.0), pass2.rgb, v_tex0_fog.z);

	float fba = max(pass1.a, disableFBA);
	vec4 color;
	color.rgb = pass1.rgb*pass1.a + pass2.rgb*fba;
	color.a = pass1.a;

	DoAlphaTest(color.a);

	FRAGCOLOR(color);
}
