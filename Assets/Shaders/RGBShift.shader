Shader "Custom/FullScreen_RGB_Shift_Corrected"
{
    Properties
    {
        _ShiftStrength("Shift Strength", Range(0, 0.1)) = 0.005
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZWrite Off Cull Off

        Pass
        {
            Name "ChromaticAberrationPass"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment Frag

            float _ShiftStrength;

            float4 Frag (Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                
                // 1. Calculate Aspect Ratio (Width / Height)
                float aspect = _ScreenParams.x / _ScreenParams.y;
                
                // 2. Adjust the 'virtual' UV for distance calculation only
                // This makes the "radial" calculation perfectly circular
                float2 centeredUV = uv - 0.5;
                float2 aspectCorrectedUV = centeredUV;
                aspectCorrectedUV.x *= aspect; 
                
                // 3. Get the actual physical distance from center
                float dist = length(aspectCorrectedUV);
                
                // 4. Calculate shift direction (still based on normal UVs)
                // We use pow(dist, 2.0) here to make the center even cleaner
                float2 shiftDir = centeredUV * pow(dist, 2.0) * _ShiftStrength;

                // 5. Sample the texture
                float r = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv + shiftDir).r;
                float g = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv).g;
                float b = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv - shiftDir).b;

                return float4(r, g, b, 1.0);
            }
            ENDHLSL
        }
    }
}