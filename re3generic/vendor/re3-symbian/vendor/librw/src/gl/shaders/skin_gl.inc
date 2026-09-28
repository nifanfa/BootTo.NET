const char *skin_vert_src =
"uniform mat4 u_boneMatrices[24];\n"

"VSIN(ATTRIB_POS) vec3 in_pos;\n"

"VSOUT vec4 v_color;\n"
"VSOUT vec3 v_tex0_fog;\n"

"void\n"
"main(void)\n"
"{\n"
"	vec3 SkinVertex = vec3(0.0, 0.0, 0.0);\n"
"	\n"
"	SkinVertex += (u_boneMatrices[int(in_indices.x)] * vec4(in_pos, 1.0)).xyz * in_weights.x;\n"
"	SkinVertex += (u_boneMatrices[int(in_indices.y)] * vec4(in_pos, 1.0)).xyz * in_weights.y;\n"
"	SkinVertex += (u_boneMatrices[int(in_indices.z)] * vec4(in_pos, 1.0)).xyz * in_weights.z;\n"
"	//SkinVertex += (u_boneMatrices[int(in_indices.w)] * vec4(in_pos, 1.0)).xyz * in_weights.w;\n"

"	vec4 Vertex = u_world * vec4(SkinVertex, 1.0);\n"
"	gl_Position = u_proj * u_view * Vertex;\n"

"	v_color = in_color;\n"
"	v_color.rgb += u_ambLight.rgb*surfAmbient;\n"
"	v_color = clamp(v_color, 0.0, 1.0);\n"
"	v_color *= u_matColor;\n"

"	v_tex0_fog = vec3(in_tex0.x, in_tex0.y, DoFog(gl_Position.w));\n"
"}\n"
;
