Shader "Custom/Sprite_MultiGlow_Unlit"
{
    Properties
    {
        [MainTexture] _MainTex ("Base Sprite Texture", 2D) = "white" {}
        
        [Header(Glow Layer 1)]
        _GlowMap1 ("Glow Mask 1 (RGB)", 2D) = "black" {}
        [HDR] _GlowColor1 ("Glow Color 1", Color) = (1,0,0,1)
        
        [Header(Glow Layer 2)]
        _GlowMap2 ("Glow Mask 2 (RGB)", 2D) = "black" {}
        [HDR] _GlowColor2 ("Glow Color 2", Color) = (0,1,0,1)
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_GlowMap1); SAMPLER(sampler_GlowMap1);
            TEXTURE2D(_GlowMap2); SAMPLER(sampler_GlowMap2);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _GlowColor1;
                float4 _GlowColor2;
            CBUFFER_END

            Varyings vert(Attributes IN) {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color = IN.color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target {
                half4 base = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * IN.color;
                
                // Sample masks
                half3 mask1 = SAMPLE_TEXTURE2D(_GlowMap1, sampler_GlowMap1, IN.uv).rgb;
                half3 mask2 = SAMPLE_TEXTURE2D(_GlowMap2, sampler_GlowMap2, IN.uv).rgb;

                // Calculate glow additions
                half3 glow1 = mask1 * _GlowColor1.rgb * _GlowColor1.a;
                half3 glow2 = mask2 * _GlowColor2.rgb * _GlowColor2.a;

                // Combine: Base + Glows
                return half4(base.rgb + glow1 + glow2, base.a);
            }
            ENDHLSL
        }
    }
}