uniform sampler2D tex0;

FSIN vec4 v_color;
FSIN vec3 v_tex0_fog;

void
main(void)
{
	vec4 color = v_color*texture2D(tex0, vec2(v_tex0_fog.x, 1.0 - v_tex0_fog.y));
	color.rgb = mix(u_fogColor.rgb, color.rgb, v_tex0_fog.z);
	DoAlphaTest(color.a);
	FRAGCOLOR(color);
}

