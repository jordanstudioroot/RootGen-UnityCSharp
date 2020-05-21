﻿Shader "Custom/Water" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Specular("Specular", Color) = (0.2, 0.2, 0.2)
	}
		SubShader{
			Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
			LOD 200

			CGPROGRAM
			#pragma surface surf StandardSpecular alpha vertex:vert // fullforwardshadows
			#pragma multi_compile _ HEX_MAP_EDIT_MODE
			#pragma target 3.0

			#include "Water.cginc"
			#include "HexData.cginc"

			sampler2D _MainTex;

			struct Input {
				float2 uv_MainTex;
				float3 worldPos;
				float2 visibility;
			};

			half _Glossiness;
			half _Specular;
			fixed4 _Color;

			void vert(inout appdata_full v, out Input data) {
				UNITY_INITIALIZE_OUTPUT(Input, data);

				float4 hex0 = GetCellData(v, 0);
				float4 hex1 = GetCellData(v, 1);
				float4 hex2 = GetCellData(v, 2);

				data.visibility.x =
					hex0.x * v.color.x + hex1.x * v.color.y + hex2.x * v.color.z;
				data.visibility.x = lerp(0.25, 1, data.visibility.x);
				data.visibility.y =
					hex0.y * v.color.x + hex1.y * v.color.y + hex2.y * v.color.z;
			}

			void surf(Input IN, inout SurfaceOutputStandardSpecular o) {
				float waves = Waves(IN.worldPos.xz, _MainTex);

				fixed4 c = saturate(_Color + waves);

				float explored = IN.visibility.y;
				o.Albedo = c.rgb * IN.visibility.x;
				o.Specular = _Specular * explored;
				o.Smoothness = _Glossiness;
				o.Occlusion = explored;
				o.Alpha = c.a * explored;
			}
			ENDCG
		}
		FallBack "Diffuse"
}