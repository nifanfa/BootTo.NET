uniform vec4 u_xform;

VSIN(ATTRIB_POS) vec4 in_pos;

VSOUT vec4 v_color;
VSOUT vec3 v_tex0_fog;

void
main(void)
{
	gl_Position = in_pos;
	gl_Position.xy = gl_Position.xy * u_xform.xy + u_xform.zw;
	v_tex0_fog.z = DoFog(gl_Position.w);
	gl_Position.xyz *= gl_Position.w;
	v_color = in_color;
	v_tex0_fog.xy = in_tex0;
}
