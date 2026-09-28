const char *im2d_vert_src =
"uniform vec4 u_xform;\n"

"VSIN(ATTRIB_POS) vec4 in_pos;\n"

"VSOUT vec4 v_color;\n"
"VSOUT vec3 v_tex0_fog;\n"

"void\n"
"main(void)\n"
"{\n"
"	gl_Position = in_pos;\n"
"	gl_Position.xy = gl_Position.xy * u_xform.xy + u_xform.zw;\n"
"	v_tex0_fog.z = DoFog(gl_Position.w);\n"
"	gl_Position.xyz *= gl_Position.w;\n"
"	v_color = in_color;\n"
"	v_tex0_fog.xy = in_tex0;\n"
"}\n"
;
