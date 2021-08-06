// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/clickWave"
{	
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_ClickPos("Click position",Vector) = (0,0,0,1.0)
		_WaveSpread("Wave spread", Range(0,11)) = 0.0
	}
	SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 normal : NORMAL0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : POSITION;
			};
			float3 _ClickPos;
			float _WaveSpread;

			v2f vert (appdata v)
			{
				v2f o;
				
				float dist = distance(_ClickPos, mul(unity_ObjectToWorld, v.vertex));
				float a = step(dist-0.8, _WaveSpread);
				float b = step(_WaveSpread, dist+0.8);

				v.vertex = lerp(v.vertex, v.vertex + v.normal / 6, min(a,b)) ;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;

			fixed4 frag (v2f i) : COLOR
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				return col;
			}
			ENDCG
		}
	}
}
