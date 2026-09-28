uniform mat4 u_boneMatrices[24];

VSIN(ATTRIB_POS) vec3 in_pos;

VSOUT vec4 v_color;
VSOUT vec3 v_tex0_fog;

void
main(void)
{
	vec3 SkinVertex = vec3(0.0, 0.0, 0.0);
	
	SkinVertex += (u_boneMatrices[int(in_indices.x)] * vec4(in_pos, 1.0)).xyz * in_weights.x;
	SkinVertex += (u_boneMatrices[int(in_indices.y)] * vec4(in_pos, 1.0)).xyz * in_weights.y;
	SkinVertex += (u_boneMatrices[int(in_indices.z)] * vec4(in_pos, 1.0)).xyz * in_weights.z;
	//SkinVertex += (u_boneMatrices[int(in_indices.w)] * vec4(in_pos, 1.0)).xyz * in_weights.w;

	vec4 Vertex = u_world * vec4(SkinVertex, 1.0);
	gl_Position = u_proj * u_view * Vertex;

	v_color = in_color;
	v_color.rgb += u_ambLight.rgb*surfAmbient;
	v_color = clamp(v_color, 0.0, 1.0);
	v_color *= u_matColor;

	v_tex0_fog = vec3(in_tex0.x, in_tex0.y, DoFog(gl_Position.w));
}
