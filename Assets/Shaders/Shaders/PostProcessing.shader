Shader "Custom/PostProcessing" {
    Properties {
        // --- current camera output texture ---
        //[MainTexture] _CameraColorTexture("Texture", 2D) = "white" {}

        // --- blur params ---
        _TargetCenter("Target Center", Vector) = (0.5, 0.5, 0, 0)
        _Power("    ", Range(-10.0, 10.0)) = 2.0
        _Iterations("Iterations", Range(1.0, 20.0)) = 10.0
        _RadialNoise("Radial Noise", 2D) = "white" {}
        _K("K", Range(0.0, 1.0)) = 0.0

        // --- gradient 2x colors params ---
        _GradPivotX("Gradient Pivot X", Range(0.0, 10.0)) = 0.5
        _GradPivotY("Gradient Pivot Y", Range(0.0, 10.0)) = 0.5
        _GradColorPivot("Gradient Color Pivot", Color) = (1, 1, 1, 1)
        _GradColorEdge("Gradient Color Edge", Color) = (1, 1, 1, 1)
        _GradClampDistance("Gradient Clapm Distance", Float) = 0.0

        // --- multiplay gradient 2x color invert params ---
        _MultGradPivotX("Mult Gradient Pivot X", Range(0.0, 10.0)) = 0.5
        _MultGradPivotY("Mult Gradient Pivot Y", Range(0.0, 10.0)) = 0.5
        _MultGradColorPivot("Mult Gradient Color Pivot", Color) = (1, 1, 1, 1)
        _MultGradColorEdge("Mult Gradient Color Edge", Color) = (1, 1, 1, 1)
        _MultGradClampX("Mult Gradient Clapm X", Float) = 0.0
        _MultGradClampY("Mult Gradient Clapm Y", Float) = 0.0



        // --- animation distortion params ---
        _DistortionAnimationSpeed("Distortion Animation Speed", Range(-0.5, 0.5)) = 0.005


        // --- fade effect params ---
        _FadeColor("Fade Color", Color) = (1, 1, 1, 1)
        _FadeAlpha("Fade Alpha", Range(0.0, 1.0)) = 0.0
    }

    SubShader {
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        ENDHLSL

        Tags { "RenderType" = "Opaque" }
        LOD 100
        ZWrite Off Cull Off

        Pass {
            Name "PostProcessing"

            HLSLPROGRAM
            
            #pragma vertex Vert
            #pragma fragment Frag



           
            // --- current camera output texture ---
            //sampler2D _CameraColorTexture;
            
            //TEXTURE2D(_CameraColorTexture);
            //SAMPLER(sampler_CameraColorTexture);
            
            // --- blur params ---
            float2 _TargetCenter;
            float _Power;
            float _Iterations;
            //sampler2D _RadialNoise;
            TEXTURE2D(_RadialNoise);
            SAMPLER(sampler_RadialNoise);
            float _K;


            // --- gradient 2x colors params ---
            float _GradPivotX;
            float _GradPivotY;
            float4 _GradColorPivot;
            float4 _GradColorEdge;
            float _GradClampDistance;

            // --- multiplay gradient 2x color invert params ---
            float _MultGradPivotX;
            float _MultGradPivotY;
            float4 _MultGradColorPivot;
            float4 _MultGradColorEdge;
            float _MultGradClampX;
            float _MultGradClampY;




            // --- animation distortion params ---
            float _DistortionAnimationSpeed;



            // --- fade params ---
            float4 _FadeColor;
            float _FadeAlpha;



            //float2 _PlayerWorldPos;

            float4 addOn(float4 A, float4 B){ // A ... bellow, B ... upper
	            float aa = A.a, ab = B.a;
	
	            float3 rgb = B.rgb * ab + A.rgb * aa * (1.0 - ab);
	            float a = ab + aa * (1.0 - ab);
	
	            return float4(rgb, a);
            }

            float4 gradientRGBA(float2 uv){
                float dist = distance(uv, float2(_GradPivotX, _GradPivotY));
                float a = min(dist / _GradClampDistance, 1.);
                return lerp(_GradColorPivot, _GradColorEdge, a);
            }

            float4 multGradientRGBA(float4 current, float2 uv){
                float dx = abs(uv.x - _MultGradPivotX);
	            float dy = abs(uv.y - _MultGradPivotY);
	
	            float clampX = min(dx / _MultGradClampX, 1.0);
	            float clampY = min(dy / _MultGradClampY, 1.0);
	
	            float len = length(float2(clampX, clampY));
                float a = min(len, 1.0);
                float4 grad = lerp(_MultGradColorPivot, _MultGradColorEdge, a);
	
	            return float4(lerp(current.rgb, current.rgb * grad.rgb, grad.a), 1.0);
            }

            float4 sample_radial_noise_color(float2 uv){
	            float2 dir = _TargetCenter - uv;
	            float2 blur = dir * _Power * 0.01;

                float dist = length(dir);
	            float ripple = sin(dist * 20.0 - _Time * 5.0) * _DistortionAnimationSpeed * dist;
	
	            //float4 noise_offset = SAMPLE_TEXTURE2D(_RadialNoise, sampler_RadialNoise, uv + ripple);
                //uv += (noise_offset.rg - 0.5) * _K * dist;
	            float speed = 0.0006;
	            float3 color = float3(0.0, 0.0, 0.0);

	            for(float i = 0.0; i < _Iterations; i++){
		            // --- simple sin animation applied for every blur seperatedly ---
		            float T = i + 1.0;
		            float2 targetUV = uv + ripple;
		            targetUV.x += sin(uv.y * 10.0 + _Time * T) * speed * T;
                    //targetUV = clamp(targetUV, 0.0, 1.0);
		
                    color += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, targetUV).rgb;
		            uv += blur;
	            }
	
	            return float4(clamp(color / _Iterations, 0.0, 1.0), 1.0);
            }

            float4 Frag(Varyings IN) : SV_Target
            {
                float2 UV = IN.texcoord;
                float4 blurDistortion = sample_radial_noise_color(UV);
	            float4 gradRGBA = gradientRGBA(UV);
	
	            float4 out_color = addOn(blurDistortion, gradRGBA);
	            float4 red_color_effect = multGradientRGBA(out_color, UV);
                float4 fade = lerp(red_color_effect, _FadeColor, _FadeAlpha);

                return fade;
            }
            ENDHLSL
        }
    }
}
