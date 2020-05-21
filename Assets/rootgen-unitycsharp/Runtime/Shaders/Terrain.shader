Shader "Custom/Terrain" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Terrain Texture Array", 2DArray) = "white" {}
		_GridTex("Grid Texture", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Specular("Specular", Color) = (0.2, 0.2, 0.2)
		_BackgroundColor("Background Color", Color) = (0,0,0)
		[Toggle(SHOW_MAP_DATA)] _ShowMapData ("Show Map Data", Float) = 0
	}
		SubShader{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma surface surf StandardSpecular fullforwardshadows vertex:vert
			#pragma multi_compile _ HEX_MAP_EDIT_MODE

			#pragma shader_feature SHOW_MAP_DATA

			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.5

			// Add toggle for grid texture
			#pragma multi_compile _ GRID_ON

			//sampler2D _MainTex;
			sampler2D _GridTex;

			#include "HexMetrics.cginc"	
			#include "HexData.cginc"
			
			UNITY_DECLARE_TEX2DARRAY(_MainTex);

		struct Input {
			//float2 uv_MainTex;
			float4 color : COLOR;
			float3 worldPos;
			float3 terrain;
			float4 visibility;

			#if defined(SHOW_MAP_DATA)
				float mapData;
			#endif
		};

		void vert(inout appdata_full v, out Input data) {
			UNITY_INITIALIZE_OUTPUT(Input, data);

			float4 hex0 = GetHexData(v, 0);
			float4 hex1 = GetHexData(v, 1);
			float4 hex2 = GetHexData(v, 2);

			data.terrain.x = hex0.w;
			data.terrain.y = hex1.w;
			data.terrain.z = hex2.w;

			data.visibility.x = hex0.x;
			data.visibility.y = hex1.x;
			data.visibility.z = hex2.x;
			data.visibility.xyz = lerp(0.25, 1, data.visibility.xyz);
			data.visibility.w =
				hex0.y * v.color.x + hex1.y * v.color.y + hex2.y * v.color.z;

			#if defined(SHOW_MAP_DATA)
				data.mapData = hex0.z * v.color.x + hex1.z * v.color.y +
					hex2.z * v.color.z;
			#endif
		}

		half _Glossiness;
		fixed3 _Specular;
		fixed4 _Color;
		half3 _BackgroundColor;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		float4 GetTerrainColor(Input IN, int index) {
			float3 uvw = float3
			(
				IN.worldPos.xz * (2 * TILING_SCALE), 
				IN.terrain[index]
			);

			float4 c = UNITY_SAMPLE_TEX2DARRAY(_MainTex, uvw);
			return c * (IN.color[index] * IN.visibility[index]);
		}

		void surf (Input IN, inout SurfaceOutputStandardSpecular o) {
			/* Scale the texture by 0.02, whcih tiles the texture roughly
			 * every 4 hexes.*/
			// float2 uv = IN.worldPos.xz * 0.02;

			fixed4 c =
				GetTerrainColor(IN, 0) +
				GetTerrainColor(IN, 1) +
				GetTerrainColor(IN, 2);

			fixed4 grid = 1;

			/* Have to scale gridUV so that it matches the texture, which
			 * is approximately a 2 Hex x 2 Hex square. Therfore, adjust
			 * the "u" coord of the UV by four times the inner radius and
			 * the "v" coord by twice the distance between adjacent hexes.
			 */

			#if defined(GRID_ON)
				float2 gridUV = IN.worldPos.xz;
				gridUV.x *= 1 / (4 * 8.66025404);
				gridUV.y *= 1 / (2 * 15.0);
				grid = tex2D(_GridTex, gridUV);
			#endif

			// Albedo comes from a texture tinted by color
			float explored = IN.visibility.w;
			o.Albedo = c.rgb *  grid * _Color * explored;

			#if defined(SHOW_MAP_DATA)
				o.Albedo = IN.mapData * grid;
			#endif

			o.Specular = _Specular * explored;
			o.Smoothness = _Glossiness;
			o.Occlusion = explored;
			o.Emission = _BackgroundColor * (1 - explored);
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
