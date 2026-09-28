const char *simple_frag_src =
"uniform sampler2D tex0;\n"

"FSIN vec4 v_color;\n"
"FSIN vec3 v_tex0_fog;\n"

"void\n"
"main(void)\n"
"{\n"
"	vec4 color = v_color*texture2D(tex0, vec2(v_tex0_fog.x, 1.0 - v_tex0_fog.y));\n"
"	color.rgb = mix(u_fogColor.rgb, color.rgb, v_tex0_fog.z);\n"
"	DoAlphaTest(color.a);\n"
"	FRAGCOLOR(color);\n"
"}\n"

;
