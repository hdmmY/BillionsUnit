Shader "BillionsUnit/TerrainTile"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}

	SubShader
	{
        Tags 
        {
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "PreviewType"="Plane"
            "RenderType"="Transparent"
        }

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
            #pragma target 4.5
            #pragma multi_compile_instancing
            #pragma instancing_options assumeuniformscaling nolodfade nolightprobe nolightmap

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

            #if SHADER_TARGET >= 45
                StructuredBuffer<float2> positionBuffer;
                float rotation;
            #endif

            void rotate2D(inout float2 v, float r)
            {
                float s, c;
                sincos(r, s, c);
                v = float2(v.x * c - v.y * s, v.x * s + v.y * c);
            }
			
			v2f vert (appdata v, uint instanceID : SV_InstanceID)
			{
            #if SHADER_TARGET >= 45
                float2 posData = positionBuffer[instanceID];
                float3 worldPos = float3(posData.x, 0, posData.y);
                float rot = rotation;
            #else
                float3 worldPos = 0;
                float rot = 0;
            #endif

				v2f o;
                
                v.vertex.xz -= 0.5;
                rotate2D(v.vertex.xz, rot);
				v.vertex.xz += 0.5;

                o.vertex = mul(UNITY_MATRIX_VP, float4(worldPos + v.vertex.xyz, 1.0f));
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				return col;
            }
			ENDCG
		}
	}
}
