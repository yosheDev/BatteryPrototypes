Shader "Custom/S_StencilWrite"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}

        [IntRange] _StencilRef ("Stencil Reference Value", Range(0,255)) = 0
    }

    SubShader
    {
        Tags { "Queue"="Geometry-1" "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        //Blend Zero One
        ColorMask 0
        ZWrite Off

        Stencil {
            Ref [_StencilRef]
            Comp Always  // Always pass the comparison test. Try editing this later to prevent writing to buffer when objects are in front of this.
            Pass Replace // If this does not fail the comparison with the ZBuffer, then replace stencil buffer value with _StencilRef.
            //Fail Zero    // If this does pass the comparison with the ZBuffer, then replace stencil buffer value with 0.
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
                return 0;
            }
            ENDHLSL
        }
    }
}
