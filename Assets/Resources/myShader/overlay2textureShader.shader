Shader "Custom/overlay2textureShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseTex("Noise Texture", 2D) = "white" {}
		_NoiseStrength("Noise Strength", Range(0, 1)) = 0.5
    }
    SubShader
    {
       Tags { "RenderType"="Opaque" }
       LOD 100
      
        Cull Off
		ZWrite Off
		ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _NoiseTex;
			float _NoiseStrength;
           
         
            fixed4 frag(v2f_img i) : COLOR
			{ 
				half4 base = tex2D(_MainTex, i.uv);
				half4 overlay = tex2D(_NoiseTex, i.uv);

				float4 effect = lerp(1 - 2 * (1 - base) * (1 - overlay), (2 * base) * overlay, step(base, 0.5f));
				// step(a, x) = 0 если x < a 
				// step(a, x) = 1 если x >= a 

				return lerp(base, effect, (overlay.w * _NoiseStrength));
			}
            ENDCG
        }
    }
}
