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
            #pragma instancing_options procedural:setup

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

            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                StructuredBuffer<float2> positionBuffer;
                StructuredBuffer<float> rotationBuffer;
            #endif

            void setup()
            {
        
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                float2 position = positionBuffer[unity_InstanceID];
                float rotation = UNITY_PI * rotationBuffer[unity_InstanceID];

                float cosr = cos(rotation);
                float sinr = sin(rotation);

                unity_ObjectToWorld = float4x4(
                    cosr, 0, sinr, position.x,
                       0, 1,    0,          0,
                   -sinr, 0, cosr, position.y,
                       0, 0,    0,          1
                );

                // unity_WorldToObject = float4x4(
                //     cosr, 0, -sinr, -position.x,
                //        0, 1,     0, -position.y,
                //     sinr, 0,  cosr,           0,
                //        0, 0,     0,           1
                // );
            #endif
            }

			
			v2f vert (appdata v)
			{
                UNITY_SETUP_INSTANCE_ID(v);

				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
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
