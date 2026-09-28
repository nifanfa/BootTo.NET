const char *im3d_vert_src =
"VSIN(ATTRIB_POS) vec3 in_pos;\n"

"VSOUT vec4 v_color;\n"
"VSOUT vec3 v_tex0_fog;\n"

"void\n"
"main(void)\n"
"{\n"
"	vec4 Vertex = u_world * vec4(in_pos, 1.0);\n"
"	gl_Position = u_proj * u_view * Vertex;\n"
"	v_color = in_color;\n"
"	v_tex0_fog.xy = in_tex0;\n"
"	v_tex0_fog.z = DoFog(gl_Position.w);\n"
"}\n"
;
