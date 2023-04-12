Shader "ProceduralGeometry"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}

	SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma target 4.5  
			#pragma vertex vert
			#pragma fragment frag

			sampler2D _MainTex;

			struct Point
			{
				float3 vertex;
				float2 uv;
			};

			StructuredBuffer<Point> points;

			struct v2f
			{
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				v2f vs;
				float4 pos = float4(points[id].vertex,1.0f);
				pos.x += inst * 1;

				vs.position = mul(UNITY_MATRIX_VP, pos);
				vs.uv = points[id].uv;
				return vs;
			}

			float4 frag(v2f ps) : SV_Target
			{
				return tex2D(_MainTex, ps.uv);
			}

			ENDCG
		}
	}
}