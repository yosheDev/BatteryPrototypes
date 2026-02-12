Shader "Custom/S_FullscreenStencilTest"
{
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            ZTest Always
            ZWrite Off
            ColorMask R
            Cull Off

            Stencil
            {
                Ref 1
                Comp Equal
                Pass Keep
                Fail Keep
                ZFail Keep
            }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionHCS : SV_POSITION; };

            Varyings Vert(Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                return o;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                return half4(1,0,0,0); // write white in R
            }
            ENDHLSL
        }
    }
}