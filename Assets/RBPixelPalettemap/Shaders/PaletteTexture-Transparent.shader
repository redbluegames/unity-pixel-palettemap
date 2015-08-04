// Unlit alpha-blended shader, combined with a color index (palette) texture.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "RBTools/Palettized Image/Palette Texture (UnlitTransparent)" {
Properties {
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	_Palette ("Palette Texture", 2D) = "white" {}
}

SubShader {
	Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	LOD 100
	
	ZWrite Off
	Blend SrcAlpha OneMinusSrcAlpha 
	
	Pass {  
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _Palette;
			
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}
			
			float4 _Palette_ST;
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 paletteMapColor = tex2D(_MainTex, i.texcoord);
				
				// The alpha channel of the palette map points to UVs in the palette key.
				float paletteX = paletteMapColor.a;
				float2 paletteUV = float2(paletteX, 0.0f);
				// Get the palette's UV accounting for texture tiling and offset
				float2 paletteUVTransformed = TRANSFORM_TEX(paletteUV, _Palette);
				
				// Get the color from the palette key
				fixed4 outColor = tex2D(_Palette, paletteUVTransformed);
				
				return outColor;
			}
		ENDCG
	}
}

}

