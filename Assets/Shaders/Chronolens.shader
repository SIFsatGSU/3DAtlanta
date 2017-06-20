// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/Chronolens"
{
	Properties
	{
		_CubeMap ("HDRI", CUBE) = "" {}
		_Mask("Mask", 2D) = "white" {}
		_HolographicTexture("Holographic Texture", 2D) = "black" {}
		_HolographicFactor("Holographic Factor", Float) = 0
		_HolographicInterval("Holographic Interval", Float) = 1
		_HolographicStep("Holographic Step", Float) = 1
		_YawOffset ("Yaw Offset", Float) = 0
		_Alpha ("Alpha", Range(0, 1)) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 100
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv: TEXCOORD0;
				float3 relativePosition: TEXCOORD1;
				float4 onScreenUV: TEXCOORD2;
			};

			samplerCUBE _CubeMap;
			sampler2D _Mask, _HolographicTexture;
			float _YawOffset, _Alpha, _HolographicInterval, _HolographicFactor, _HolographicStep;
			float4 _Mask_ST, _HolographicTexture_ST;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _Mask);
				o.relativePosition = mul(unity_ObjectToWorld, v.vertex).xyz - _WorldSpaceCameraPos;
				o.onScreenUV = ComputeScreenPos(o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				/*float hdriU = atan2(i.relativePosition.x, i.relativePosition.z) / 6.28318530718;

				//0.00277777777 = pi / (180 * 2pi)
				hdriU +=  _YawOffset * 0.00277777777;
				float hdriV = 1 - atan2(sqrt(i.relativePosition.x * i.relativePosition.x
					 	+ i.relativePosition.z * i.relativePosition.z), i.relativePosition.y) / 3.1516926535;
			 	*/
			 	float angle = _YawOffset * 0.01745329251;
			 	float sinAngle = sin(angle);
			 	float cosAngle = cos(angle);
			 	// Rotate the vector by the yaw offset.
			 	float3 viewDirection = float3(cosAngle * i.relativePosition.x - sinAngle * i.relativePosition.z,
			 			i.relativePosition.y, sinAngle * i.relativePosition.x + cosAngle * i.relativePosition.z);
				float3 rgb = texCUBE(_CubeMap, viewDirection).rgb;
				float mask = tex2D(_Mask, i.uv).r;
				float2 onScreenUV = i.onScreenUV.xy / i.onScreenUV.w;
				onScreenUV.x = onScreenUV.x * _HolographicTexture_ST.x + _HolographicTexture_ST.z;
				onScreenUV.y = onScreenUV.y * _HolographicTexture_ST.y + _HolographicTexture_ST.w;
				float4 hologram = tex2D(_HolographicTexture, onScreenUV 
						+ round(_Time[2] * _HolographicInterval) / _HolographicInterval * _HolographicStep % 1);
				fixed4 finalColor = float4(rgb + hologram.rgb * _HolographicFactor, mask * _Alpha);
				return finalColor;
			}
			ENDCG
		}
	}
}
