Shader "Hidden/PostProcessing/Fisheye"
{
    Properties
    {
        _Strength("Distortion Strength", Float) = 1.5
    }
    
    HLSLINCLUDE
    // Include essential URP and Blit libraries
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

    float _Strength;

    float4 FisheyeFragment(Varyings input) : SV_Target
    {
        // 1. Get the current screen UV coordinates
        float2 uv = input.texcoord;
        
        // 2. Shift UVs to range [-0.5, 0.5] so center is at (0,0)
        float2 centeredUV = uv - 0.5;

        // 3. Calculate distance from the center
        float dist = length(centeredUV);

        // 4. Calculate base distortion multiplier
        float distortion = 1.0 + (_Strength * (dist * dist));

        // --- THE NEW MATH ---
        // Calculate the maximum distortion (which happens at the screen corners).
        // The squared distance from the center to a corner (0.5, 0.5) is exactly 0.5.
        float maxDistortion = 1.0 + (_Strength * 0.5);

        // 5. Apply distortion, but divide by maxDistortion to auto-zoom the image.
        // This perfectly locks the corners of the distortion to the corners of the screen.
        float2 distortedUV = (centeredUV * (distortion / maxDistortion)) + 0.5;

        // 6. Sample the screen texture (No more black edges!)
        return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, distortedUV);
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        ZWrite Off Cull Off ZTest Always // Standard setup for post-processing

        Pass
        {
            Name "FisheyePass"

            HLSLPROGRAM
            // Use the built-in Vertex shader from Blit.hlsl
            #pragma vertex Vert 
            #pragma fragment FisheyeFragment
            ENDHLSL
        }
    }
}