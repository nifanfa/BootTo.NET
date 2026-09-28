VSIN(ATTRIB_POS) vec3 in_pos;

VSOUT vec4 v_color;
VSOUT vec3 v_tex0_fog;

void
main(void)
{
	vec4 Vertex = u_world * vec4(in_pos, 1.0);
	gl_Position = u_proj * u_view * Vertex;
	v_color = in_color;
	v_tex0_fog.xy = in_tex0;
	v_tex0_fog.z = DoFog(gl_Position.w);
}
