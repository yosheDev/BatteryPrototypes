Shader "Custom/S_ColorUnlit"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [Opacity] _Opacity("Opacity", Float) = .2
        [IntRange] _StencilRef ("Stencil Reference Value", Range(0,255)) = 1
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        
        Cull Back
        Blend SrcAlpha OneMinusSrcAlpha
        ZTest LEqual
        ZWrite On

        Stencil {
            Ref [_StencilRef]
            Comp Always
            Pass Replace
            Fail Keep
            ZFail Keep
        }

        Pass
        {
            

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float _Opacity;
                float4 _BaseMap_ST;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                //half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                return float4(_BaseColor.r, _BaseColor.g, _BaseColor.b, 1);//_Opacity);
            }
            ENDHLSL
        }
    }
}
