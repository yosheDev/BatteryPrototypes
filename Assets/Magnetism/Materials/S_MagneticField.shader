// Shader "Custom/S_MagneticField"
// {
//     Properties
//     {
//         _MainTex("MainTex", 2D) = "white" {}
//         _FieldBaseColor("FieldBaseColor", Color) = (1, 0, 0, 1)
//         _Opacity("Opacity", Float) = 0.25
//         _OpacityRemapDistanceRange("OpacityRemapDistanceRange", Vector, 2) = (1.5, 4, 0, 0)
//         _NegWaveFrequency("NegWaveFrequency", Float) = 15
//         _NegWaveSpeed("NegWaveSpeed", Float) = 10
//         [HideInInspector]_QueueOffset("_QueueOffset", Float) = 0
//         [HideInInspector]_QueueControl("_QueueControl", Float) = -1
//         [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
//         [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
//         [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
//     }
//     SubShader
//     {
//         Tags
//         {
//             "RenderPipeline"="UniversalPipeline"
//             "RenderType"="Transparent"
//             "UniversalMaterialType" = "Unlit"
//             "Queue"="Transparent"
//             "DisableBatching"="False"
//             "ShaderGraphShader"="true"
//             "ShaderGraphTargetId"="UniversalUnlitSubTarget"
//         }
//         Pass
//         {
//             Name "Universal Forward"
//             Tags
//             {
//                 "LightMode" = "UniversalForward"
//             }
        
//         // Render State
//         Cull Back
//         Blend SrcAlpha OneMinusSrcAlpha
//         ZTest LEqual
//         ZWrite Off
        
//         Stencil {
//             Ref 0
//             Comp Equal
//             Pass IncrSat 
//             Fail IncrSat 
//         }

//         // Debug
//         // <None>
        
//         // --------------------------------------------------
//         // Pass
        
//         HLSLPROGRAM
        
//         // Pragmas
//         #pragma target 2.0
//         #pragma multi_compile_instancing
//         #pragma instancing_options renderinglayer
//         #pragma vertex vert
//         #pragma fragment frag
        
//         // Keywords
//         #pragma multi_compile _ LIGHTMAP_ON
//         #pragma multi_compile _ DIRLIGHTMAP_COMBINED
//         #pragma multi_compile _ USE_LEGACY_LIGHTMAPS
//         #pragma multi_compile _ LIGHTMAP_BICUBIC_SAMPLING
//         #pragma shader_feature _ _SAMPLE_GI
//         #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
//         #pragma multi_compile_fragment _ DEBUG_DISPLAY
//         #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
//         // GraphKeywords: <None>
        
//         // Defines
        
//         #define ATTRIBUTES_NEED_NORMAL
//         #define ATTRIBUTES_NEED_TANGENT
//         #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
//         #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
//         #define VARYINGS_NEED_POSITION_WS
//         #define VARYINGS_NEED_NORMAL_WS
//         #define FEATURES_GRAPH_VERTEX
//         /* WARNING: $splice Could not find named fragment 'PassInstancing' */
//         #define SHADERPASS SHADERPASS_UNLIT
//         #define _FOG_FRAGMENT 1
//         #define _SURFACE_TYPE_TRANSPARENT 1
        
        
//         // custom interpolator pre-include
//         /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
//         // Includes
//         #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
//         #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"
//         #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
//         #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
//         #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
//         #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
//         #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
//         #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
//         #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
//         // --------------------------------------------------
//         // Structs and Packing
        
//         // custom interpolators pre packing
//         /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
//         struct Attributes
//         {
//              float3 positionOS : POSITION;
//              float3 normalOS : NORMAL;
//              float4 tangentOS : TANGENT;
//             #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
//              uint instanceID : INSTANCEID_SEMANTIC;
//             #endif
//         };
//         struct Varyings
//         {
//              float4 positionCS : SV_POSITION;
//              float3 positionWS;
//              float3 normalWS;
//             #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
//              uint instanceID : CUSTOM_INSTANCE_ID;
//             #endif
//             #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
//              uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
//             #endif
//             #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
//              uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
//             #endif
//             #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
//              FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
//             #endif
//         };
//         struct SurfaceDescriptionInputs
//         {
//         };
//         struct VertexDescriptionInputs
//         {
//              float3 ObjectSpaceNormal;
//              float3 ObjectSpaceTangent;
//              float3 ObjectSpacePosition;
//         };
//         struct PackedVaryings
//         {
//              float4 positionCS : SV_POSITION;
//              float3 positionWS : INTERP0;
//              float3 normalWS : INTERP1;
//             #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
//              uint instanceID : CUSTOM_INSTANCE_ID;
//             #endif
//             #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
//              uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
//             #endif
//             #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
//              uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
//             #endif
//             #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
//              FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
//             #endif
//         };
        
//         PackedVaryings PackVaryings (Varyings input)
//         {
//             PackedVaryings output;
//             ZERO_INITIALIZE(PackedVaryings, output);
//             output.positionCS = input.positionCS;
//             output.positionWS.xyz = input.positionWS;
//             output.normalWS.xyz = input.normalWS;
//             #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
//             output.instanceID = input.instanceID;
//             #endif
//             #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
//             output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
//             #endif
//             #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
//             output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
//             #endif
//             #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
//             output.cullFace = input.cullFace;
//             #endif
//             return output;
//         }
        
//         Varyings UnpackVaryings (PackedVaryings input)
//         {
//             Varyings output;
//             output.positionCS = input.positionCS;
//             output.positionWS = input.positionWS.xyz;
//             output.normalWS = input.normalWS.xyz;
//             #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
//             output.instanceID = input.instanceID;
//             #endif
//             #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
//             output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
//             #endif
//             #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
//             output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
//             #endif
//             #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
//             output.cullFace = input.cullFace;
//             #endif
//             return output;
//         }
        
        
//         // --------------------------------------------------
//         // Graph
        
//         // Graph Properties
//         CBUFFER_START(UnityPerMaterial)
//         float4 _FieldBaseColor;
//         float _Opacity;
//         float2 _OpacityRemapDistanceRange;
//         float _NegWaveFrequency;
//         float _NegWaveSpeed;
//         UNITY_TEXTURE_STREAMING_DEBUG_VARS;
//         CBUFFER_END
        
//         #if defined(DOTS_INSTANCING_ON)
//         // DOTS instancing definitions
//         UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
//             UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float, _Opacity)
//         UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)
//         // DOTS instancing usage macros
//         #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(type, var)
//         #elif defined(UNITY_INSTANCING_ENABLED)
//         // Unity instancing definitions
//         UNITY_INSTANCING_BUFFER_START(SGPerInstanceData)
//             UNITY_DEFINE_INSTANCED_PROP(float, _Opacity)
//         UNITY_INSTANCING_BUFFER_END(SGPerInstanceData)
//         // Unity instancing usage macros
//         #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) UNITY_ACCESS_INSTANCED_PROP(SGPerInstanceData, var)
//         #else
//         #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) var
//         #endif
        
//         // Object and Global properties
//         float3 _PlayerPosMag;
//         float3 _PlayerNegMag;
//         float3 _PlayerNegMagScreen;
        
//         // Graph Includes
//         // GraphIncludes: <None>
        
//         // -- Property used by ScenePickingPass
//         #ifdef SCENEPICKINGPASS
//         float4 _SelectionID;
//         #endif
        
//         // -- Properties used by SceneSelectionPass
//         #ifdef SCENESELECTIONPASS
//         int _ObjectId;
//         int _PassValue;
//         #endif
        
//         // Graph Functions
//         // GraphFunctions: <None>
        
//         // Custom interpolators pre vertex
//         /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
//         // Graph Vertex
//         struct VertexDescription
//         {
//             float3 Position;
//             float3 Normal;
//             float3 Tangent;
//         };
        
//         VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
//         {
//             VertexDescription description = (VertexDescription)0;
//             description.Position = IN.ObjectSpacePosition;
//             description.Normal = IN.ObjectSpaceNormal;
//             description.Tangent = IN.ObjectSpaceTangent;
//             return description;
//         }
        
//         // Custom interpolators, pre surface
//         #ifdef FEATURES_GRAPH_VERTEX
//         Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
//         {
//         return output;
//         }
//         #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
//         #endif
        
//         // Graph Pixel
//         struct SurfaceDescription
//         {
//             float3 BaseColor;
//             float Alpha;
//         };
        
//         SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
//         {
//             SurfaceDescription surface = (SurfaceDescription)0;
//             float4 _Property_51d1e656146441f4a966a18881f32006_Out_0_Vector4 = _FieldBaseColor;
//             surface.BaseColor = (_Property_51d1e656146441f4a966a18881f32006_Out_0_Vector4.xyz);
//             surface.Alpha = float(0.25);
//             return surface;
//         }
        
//         // --------------------------------------------------
//         // Build Graph Inputs
//         #ifdef HAVE_VFX_MODIFICATION
//         #define VFX_SRP_ATTRIBUTES Attributes
//         #define VFX_SRP_VARYINGS Varyings
//         #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
//         #endif
//         VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
//         {
//             VertexDescriptionInputs output;
//             ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
//             output.ObjectSpaceNormal =                          input.normalOS;
//             output.ObjectSpaceTangent =                         input.tangentOS.xyz;
//             output.ObjectSpacePosition =                        input.positionOS;
//         #if UNITY_ANY_INSTANCING_ENABLED
//         #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
//         #endif
        
//             return output;
//         }
//         SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
//         {
//             SurfaceDescriptionInputs output;
//             ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
//         #ifdef HAVE_VFX_MODIFICATION
//         #if VFX_USE_GRAPH_VALUES
//             uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
//             /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
//         #endif
//             /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
//         #endif
        
            
        
        
        
        
        
        
//             #if UNITY_UV_STARTS_AT_TOP
//             #else
//             #endif
        
        
//         #if UNITY_ANY_INSTANCING_ENABLED
//         #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
//         #endif
//         #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
//         #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
//         #else
//         #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
//         #endif
//         #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
//                 return output;
//         }
        
//         // --------------------------------------------------
//         // Main
        
//         #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/UnlitPass.hlsl"
        
//         // --------------------------------------------------
//         // Visual Effect Vertex Invocations
//         #ifdef HAVE_VFX_MODIFICATION
//         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
//         #endif
        
//         ENDHLSL
//         }
//         Pass
//         {
//             Name "MotionVectors"
//             Tags
//             {
//                 "LightMode" = "MotionVectors"
//             }
        
//         // Render State
//         Cull Back
//         ZTest LEqual
//         ZWrite On
//         ColorMask RG
        
//         // Debug
//         // <None>
        
//         // --------------------------------------------------
//         // Pass
        
//         HLSLPROGRAM
        
//         // Pragmas
//         #pragma target 3.5
//         #pragma multi_compile_instancing
//         #pragma vertex vert
//         #pragma fragment frag
        
//         // Keywords
//         // PassKeywords: <None>
//         // GraphKeywords: <None>
        
//         // Defines
        
//         #define FEATURES_GRAPH_VERTEX
//         /* WARNING: $splice Could not find named fragment 'PassInstancing' */
//         #define SHADERPASS SHADERPASS_MOTION_VECTORS
        
        
//         // custom interpolator pre-include
//         /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
//         // Includes
//         #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
//         #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
//         #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
//         #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
//         #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
//         #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
//         #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
//         #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
//         // --------------------------------------------------
//         // Structs and Packing
        
//         // custom interpolators pre packing
//         /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
//         struct Attributes
//         {
//              float3 positionOS : POSITION;
//             #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
//              uint instanceID : INSTANCEID_SEMANTIC;
//             #endif
//         };
//         struct Varyings
//         {
//              float4 positionCS : SV_POSITION;
//             #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
//              uint instanceID : CUSTOM_INSTANCE_ID;
//             #endif
//             #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
//              uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
//             #endif
//             #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
//              uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
//             #endif
//             #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
//              FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
//             #endif
//         };
//         struct SurfaceDescriptionInputs
//         {
//         };
//         struct VertexDescriptionInputs
//         {
//              float3 ObjectSpacePosition;
//         };
//         struct PackedVaryings
//         {
//              float4 positionCS : SV_POSITION;
//             #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
//              uint instanceID : CUSTOM_INSTANCE_ID;
//             #endif
//             #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
//              uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
//             #endif
//             #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
//              uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
//             #endif
//             #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
//              FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
//             #endif
//         };
        
//         PackedVaryings PackVaryings (Varyings input)
//         {
//             PackedVaryings output;
//             ZERO_INITIALIZE(PackedVaryings, output);
//             output.positionCS = input.positionCS;
//             #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
//             output.instanceID = input.instanceID;
//             #endif
//             #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
//             output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
//             #endif
//             #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
//             output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
//             #endif
//             #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
//             output.cullFace = input.cullFace;
//             #endif
//             return output;
//         }
        
//         Varyings UnpackVaryings (PackedVaryings input)
//         {
//             Varyings output;
//             output.positionCS = input.positionCS;
//             #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
//             output.instanceID = input.instanceID;
//             #endif
//             #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
//             output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
//             #endif
//             #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
//             output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
//             #endif
//             #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
//             output.cullFace = input.cullFace;
//             #endif
//             return output;
//         }
        
        
//         // --------------------------------------------------
//         // Graph
        
//         // Graph Properties
//         CBUFFER_START(UnityPerMaterial)
//         float4 _FieldBaseColor;
//         float _Opacity;
//         float2 _OpacityRemapDistanceRange;
//         float _NegWaveFrequency;
//         float _NegWaveSpeed;
//         UNITY_TEXTURE_STREAMING_DEBUG_VARS;
//         CBUFFER_END
        
//         #if defined(DOTS_INSTANCING_ON)
//         // DOTS instancing definitions
//         UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
//             UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float, _Opacity)
//         UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)
//         // DOTS instancing usage macros
//         #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(type, var)
//         #elif defined(UNITY_INSTANCING_ENABLED)
//         // Unity instancing definitions
//         UNITY_INSTANCING_BUFFER_START(SGPerInstanceData)
//             UNITY_DEFINE_INSTANCED_PROP(float, _Opacity)
//         UNITY_INSTANCING_BUFFER_END(SGPerInstanceData)
//         // Unity instancing usage macros
//         #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) UNITY_ACCESS_INSTANCED_PROP(SGPerInstanceData, var)
//         #else
//         #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) var
//         #endif
        
//         // Object and Global properties
//         float3 _PlayerPosMag;
//         float3 _PlayerNegMag;
//         float3 _PlayerNegMagScreen;
        
//         // Graph Includes
//         // GraphIncludes: <None>
        
//         // -- Property used by ScenePickingPass
//         #ifdef SCENEPICKINGPASS
//         float4 _SelectionID;
//         #endif
        
//         // -- Properties used by SceneSelectionPass
//         #ifdef SCENESELECTIONPASS
//         int _ObjectId;
//         int _PassValue;
//         #endif
        
//         // Graph Functions
//         // GraphFunctions: <None>
        
//         // Custom interpolators pre vertex
//         /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
//         // Graph Vertex
//         struct VertexDescription
//         {
//             float3 Position;
//         };
        
//         VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
//         {
//             VertexDescription description = (VertexDescription)0;
//             description.Position = IN.ObjectSpacePosition;
//             return description;
//         }
        
//         // Custom interpolators, pre surface
//         #ifdef FEATURES_GRAPH_VERTEX
//         Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
//         {
//         return output;
//         }
//         #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
//         #endif
        
//         // Graph Pixel
//         struct SurfaceDescription
//         {
//             float Alpha;
//         };
        
//         SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
//         {
//             SurfaceDescription surface = (SurfaceDescription)0;
//             surface.Alpha = float(0.25);
//             return surface;
//         }
        
//         // --------------------------------------------------
//         // Build Graph Inputs
//         #ifdef HAVE_VFX_MODIFICATION
//         #define VFX_SRP_ATTRIBUTES Attributes
//         #define VFX_SRP_VARYINGS Varyings
//         #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
//         #endif
//         VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
//         {
//             VertexDescriptionInputs output;
//             ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
//             output.ObjectSpacePosition =                        input.positionOS;
//         #if UNITY_ANY_INSTANCING_ENABLED
//         #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
//         #endif
        
//             return output;
//         }
//         SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
//         {
//             SurfaceDescriptionInputs output;
//             ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
//         #ifdef HAVE_VFX_MODIFICATION
//         #if VFX_USE_GRAPH_VALUES
//             uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
//             /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
//         #endif
//             /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
//         #endif
        
            
        
        
        
        
        
        
//             #if UNITY_UV_STARTS_AT_TOP
//             #else
//             #endif
        
        
//         #if UNITY_ANY_INSTANCING_ENABLED
//         #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
//         #endif
//         #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
//         #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
//         #else
//         #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
//         #endif
//         #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
//                 return output;
//         }
        
//         // --------------------------------------------------
//         // Main
        
//         #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/MotionVectorPass.hlsl"
        
//         // --------------------------------------------------
//         // Visual Effect Vertex Invocations
//         #ifdef HAVE_VFX_MODIFICATION
//         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
//         #endif
        
//         ENDHLSL
//         }
//         Pass
//         {
//             Name "DepthNormalsOnly"
//             Tags
//             {
//                 "LightMode" = "DepthNormalsOnly"
//             }
        
//         // Render State
//         Cull Back
//         ZTest LEqual
//         ZWrite On
        
//         // Debug
//         // <None>
        
//         // --------------------------------------------------
//         // Pass
        
//         HLSLPROGRAM
        
//         // Pragmas
//         #pragma target 2.0
//         #pragma multi_compile_instancing
//         #pragma vertex vert
//         #pragma fragment frag
        
//         // Keywords
//         #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
//         // GraphKeywords: <None>
        
//         // Defines
        
//         #define ATTRIBUTES_NEED_NORMAL
//         #define ATTRIBUTES_NEED_TANGENT
//         #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
//         #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
//         #define VARYINGS_NEED_NORMAL_WS
//         #define FEATURES_GRAPH_VERTEX
//         /* WARNING: $splice Could not find named fragment 'PassInstancing' */
//         #define SHADERPASS SHADERPASS_DEPTHNORMALSONLY
//         #define _SURFACE_TYPE_TRANSPARENT 1
        
        
//         // custom interpolator pre-include
//         /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
//         // Includes
//         #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
//         #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
//         #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
//         #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
//         #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
//         #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
//         #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
//         #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
//         // --------------------------------------------------
//         // Structs and Packing
        
//         // custom interpolators pre packing
//         /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
//         struct Attributes
//         {
//              float3 positionOS : POSITION;
//              float3 normalOS : NORMAL;
//              float4 tangentOS : TANGENT;
//             #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
//              uint instanceID : INSTANCEID_SEMANTIC;
//             #endif
//         };
//         struct Varyings
//         {
//              float4 positionCS : SV_POSITION;
//              float3 normalWS;
//             #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
//              uint instanceID : CUSTOM_INSTANCE_ID;
//             #endif
//             #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
//              uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
//             #endif
//             #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
//              uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
//             #endif
//             #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
//              FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
//             #endif
//         };
//         struct SurfaceDescriptionInputs
//         {
//         };
//         struct VertexDescriptionInputs
//         {
//              float3 ObjectSpaceNormal;
//              float3 ObjectSpaceTangent;
//              float3 ObjectSpacePosition;
//         };
//         struct PackedVaryings
//         {
//              float4 positionCS : SV_POSITION;
//              float3 normalWS : INTERP0;
//             #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
//              uint instanceID : CUSTOM_INSTANCE_ID;
//             #endif
//             #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
//              uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
//             #endif
//             #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
//              uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
//             #endif
//             #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
//              FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
//             #endif
//         };
        
//         PackedVaryings PackVaryings (Varyings input)
//         {
//             PackedVaryings output;
//             ZERO_INITIALIZE(PackedVaryings, output);
//             output.positionCS = input.positionCS;
//             output.normalWS.xyz = input.normalWS;
//             #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
//             output.instanceID = input.instanceID;
//             #endif
//             #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
//             output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
//             #endif
//             #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
//             output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
//             #endif
//             #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
//             output.cullFace = input.cullFace;
//             #endif
//             return output;
//         }
        
//         Varyings UnpackVaryings (PackedVaryings input)
//         {
//             Varyings output;
//             output.positionCS = input.positionCS;
//             output.normalWS = input.normalWS.xyz;
//             #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
//             output.instanceID = input.instanceID;
//             #endif
//             #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
//             output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
//             #endif
//             #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
//             output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
//             #endif
//             #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
//             output.cullFace = input.cullFace;
//             #endif
//             return output;
//         }
        
        
//         // --------------------------------------------------
//         // Graph
        
//         // Graph Properties
//         CBUFFER_START(UnityPerMaterial)
//         float4 _FieldBaseColor;
//         float _Opacity;
//         float2 _OpacityRemapDistanceRange;
//         float _NegWaveFrequency;
//         float _NegWaveSpeed;
//         UNITY_TEXTURE_STREAMING_DEBUG_VARS;
//         CBUFFER_END
        
//         #if defined(DOTS_INSTANCING_ON)
//         // DOTS instancing definitions
//         UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
//             UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float, _Opacity)
//         UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)
//         // DOTS instancing usage macros
//         #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(type, var)
//         #elif defined(UNITY_INSTANCING_ENABLED)
//         // Unity instancing definitions
//         UNITY_INSTANCING_BUFFER_START(SGPerInstanceData)
//             UNITY_DEFINE_INSTANCED_PROP(float, _Opacity)
//         UNITY_INSTANCING_BUFFER_END(SGPerInstanceData)
//         // Unity instancing usage macros
//         #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) UNITY_ACCESS_INSTANCED_PROP(SGPerInstanceData, var)
//         #else
//         #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) var
//         #endif
        
//         // Object and Global properties
//         float3 _PlayerPosMag;
//         float3 _PlayerNegMag;
//         float3 _PlayerNegMagScreen;
        
//         // Graph Includes
//         // GraphIncludes: <None>
        
//         // -- Property used by ScenePickingPass
//         #ifdef SCENEPICKINGPASS
//         float4 _SelectionID;
//         #endif
        
//         // -- Properties used by SceneSelectionPass
//         #ifdef SCENESELECTIONPASS
//         int _ObjectId;
//         int _PassValue;
//         #endif
        
//         // Graph Functions
//         // GraphFunctions: <None>
        
//         // Custom interpolators pre vertex
//         /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
//         // Graph Vertex
//         struct VertexDescription
//         {
//             float3 Position;
//             float3 Normal;
//             float3 Tangent;
//         };
        
//         VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
//         {
//             VertexDescription description = (VertexDescription)0;
//             description.Position = IN.ObjectSpacePosition;
//             description.Normal = IN.ObjectSpaceNormal;
//             description.Tangent = IN.ObjectSpaceTangent;
//             return description;
//         }
        
//         // Custom interpolators, pre surface
//         #ifdef FEATURES_GRAPH_VERTEX
//         Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
//         {
//         return output;
//         }
//         #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
//         #endif
        
//         // Graph Pixel
//         struct SurfaceDescription
//         {
//             float Alpha;
//         };
        
//         SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
//         {
//             SurfaceDescription surface = (SurfaceDescription)0;
//             surface.Alpha = float(0.25);
//             return surface;
//         }
        
//         // --------------------------------------------------
//         // Build Graph Inputs
//         #ifdef HAVE_VFX_MODIFICATION
//         #define VFX_SRP_ATTRIBUTES Attributes
//         #define VFX_SRP_VARYINGS Varyings
//         #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
//         #endif
//         VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
//         {
//             VertexDescriptionInputs output;
//             ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
//             output.ObjectSpaceNormal =                          input.normalOS;
//             output.ObjectSpaceTangent =                         input.tangentOS.xyz;
//             output.ObjectSpacePosition =                        input.positionOS;
//         #if UNITY_ANY_INSTANCING_ENABLED
//         #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
//         #endif
        
//             return output;
//         }
//         SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
//         {
//             SurfaceDescriptionInputs output;
//             ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
//         #ifdef HAVE_VFX_MODIFICATION
//         #if VFX_USE_GRAPH_VALUES
//             uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
//             /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
//         #endif
//             /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
//         #endif
        
            
        
        
        
        
        
        
//             #if UNITY_UV_STARTS_AT_TOP
//             #else
//             #endif
        
        
//         #if UNITY_ANY_INSTANCING_ENABLED
//         #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
//         #endif
//         #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
//         #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
//         #else
//         #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
//         #endif
//         #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
//                 return output;
//         }
        
//         // --------------------------------------------------
//         // Main
        
//         #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthNormalsOnlyPass.hlsl"
        
//         // --------------------------------------------------
//         // Visual Effect Vertex Invocations
//         #ifdef HAVE_VFX_MODIFICATION
//         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
//         #endif
        
//         ENDHLSL
//         }
//         Pass
//         {
//             Name "ShadowCaster"
//             Tags
//             {
//                 "LightMode" = "ShadowCaster"
//             }
        
//         // Render State
//         Cull Back
//         ZTest LEqual
//         ZWrite On
//         ColorMask 0
        
//         // Debug
//         // <None>
        
//         // --------------------------------------------------
//         // Pass
        
//         HLSLPROGRAM
        
//         // Pragmas
//         #pragma target 2.0
//         #pragma multi_compile_instancing
//         #pragma vertex vert
//         #pragma fragment frag
        
//         // Keywords
//         #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
//         // GraphKeywords: <None>
        
//         // Defines
        
//         #define ATTRIBUTES_NEED_NORMAL
//         #define ATTRIBUTES_NEED_TANGENT
//         #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
//         #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
//         #define VARYINGS_NEED_NORMAL_WS
//         #define FEATURES_GRAPH_VERTEX
//         /* WARNING: $splice Could not find named fragment 'PassInstancing' */
//         #define SHADERPASS SHADERPASS_SHADOWCASTER
        
        
//         // custom interpolator pre-include
//         /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
//         // Includes
//         #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
//         #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
//         #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
//         #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
//         #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
//         #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
//         #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
//         // --------------------------------------------------
//         // Structs and Packing
        
//         // custom interpolators pre packing
//         /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
//         struct Attributes
//         {
//              float3 positionOS : POSITION;
//              float3 normalOS : NORMAL;
//              float4 tangentOS : TANGENT;
//             #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
//              uint instanceID : INSTANCEID_SEMANTIC;
//             #endif
//         };
//         struct Varyings
//         {
//              float4 positionCS : SV_POSITION;
//              float3 normalWS;
//             #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
//              uint instanceID : CUSTOM_INSTANCE_ID;
//             #endif
//             #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
//              uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
//             #endif
//             #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
//              uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
//             #endif
//             #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
//              FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
//             #endif
//         };
//         struct SurfaceDescriptionInputs
//         {
//         };
//         struct VertexDescriptionInputs
//         {
//              float3 ObjectSpaceNormal;
//              float3 ObjectSpaceTangent;
//              float3 ObjectSpacePosition;
//         };
//         struct PackedVaryings
//         {
//              float4 positionCS : SV_POSITION;
//              float3 normalWS : INTERP0;
//             #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
//              uint instanceID : CUSTOM_INSTANCE_ID;
//             #endif
//             #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
//              uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
//             #endif
//             #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
//              uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
//             #endif
//             #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
//              FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
//             #endif
//         };
        
//         PackedVaryings PackVaryings (Varyings input)
//         {
//             PackedVaryings output;
//             ZERO_INITIALIZE(PackedVaryings, output);
//             output.positionCS = input.positionCS;
//             output.normalWS.xyz = input.normalWS;
//             #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
//             output.instanceID = input.instanceID;
//             #endif
//             #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
//             output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
//             #endif
//             #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
//             output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
//             #endif
//             #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
//             output.cullFace = input.cullFace;
//             #endif
//             return output;
//         }
        
//         Varyings UnpackVaryings (PackedVaryings input)
//         {
//             Varyings output;
//             output.positionCS = input.positionCS;
//             output.normalWS = input.normalWS.xyz;
//             #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
//             output.instanceID = input.instanceID;
//             #endif
//             #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
//             output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
//             #endif
//             #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
//             output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
//             #endif
//             #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
//             output.cullFace = input.cullFace;
//             #endif
//             return output;
//         }
        
        
//         // --------------------------------------------------
//         // Graph
        
//         // Graph Properties
//         CBUFFER_START(UnityPerMaterial)
//         float4 _FieldBaseColor;
//         float _Opacity;
//         float2 _OpacityRemapDistanceRange;
//         float _NegWaveFrequency;
//         float _NegWaveSpeed;
//         UNITY_TEXTURE_STREAMING_DEBUG_VARS;
//         CBUFFER_END
        
//         #if defined(DOTS_INSTANCING_ON)
//         // DOTS instancing definitions
//         UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
//             UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float, _Opacity)
//         UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)
//         // DOTS instancing usage macros
//         #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(type, var)
//         #elif defined(UNITY_INSTANCING_ENABLED)
//         // Unity instancing definitions
//         UNITY_INSTANCING_BUFFER_START(SGPerInstanceData)
//             UNITY_DEFINE_INSTANCED_PROP(float, _Opacity)
//         UNITY_INSTANCING_BUFFER_END(SGPerInstanceData)
//         // Unity instancing usage macros
//         #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) UNITY_ACCESS_INSTANCED_PROP(SGPerInstanceData, var)
//         #else
//         #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) var
//         #endif
        
//         // Object and Global properties
//         float3 _PlayerPosMag;
//         float3 _PlayerNegMag;
//         float3 _PlayerNegMagScreen;
        
//         // Graph Includes
//         // GraphIncludes: <None>
        
//         // -- Property used by ScenePickingPass
//         #ifdef SCENEPICKINGPASS
//         float4 _SelectionID;
//         #endif
        
//         // -- Properties used by SceneSelectionPass
//         #ifdef SCENESELECTIONPASS
//         int _ObjectId;
//         int _PassValue;
//         #endif
        
//         // Graph Functions
//         // GraphFunctions: <None>
        
//         // Custom interpolators pre vertex
//         /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
//         // Graph Vertex
//         struct VertexDescription
//         {
//             float3 Position;
//             float3 Normal;
//             float3 Tangent;
//         };
        
//         VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
//         {
//             VertexDescription description = (VertexDescription)0;
//             description.Position = IN.ObjectSpacePosition;
//             description.Normal = IN.ObjectSpaceNormal;
//             description.Tangent = IN.ObjectSpaceTangent;
//             return description;
//         }
        
//         // Custom interpolators, pre surface
//         #ifdef FEATURES_GRAPH_VERTEX
//         Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
//         {
//         return output;
//         }
//         #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
//         #endif
        
//         // Graph Pixel
//         struct SurfaceDescription
//         {
//             float Alpha;
//         };
        
//         SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
//         {
//             SurfaceDescription surface = (SurfaceDescription)0;
//             surface.Alpha = float(0.25);
//             return surface;
//         }
        
//         // --------------------------------------------------
//         // Build Graph Inputs
//         #ifdef HAVE_VFX_MODIFICATION
//         #define VFX_SRP_ATTRIBUTES Attributes
//         #define VFX_SRP_VARYINGS Varyings
//         #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
//         #endif
//         VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
//         {
//             VertexDescriptionInputs output;
//             ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
//             output.ObjectSpaceNormal =                          input.normalOS;
//             output.ObjectSpaceTangent =                         input.tangentOS.xyz;
//             output.ObjectSpacePosition =                        input.positionOS;
//         #if UNITY_ANY_INSTANCING_ENABLED
//         #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
//         #endif
        
//             return output;
//         }
//         SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
//         {
//             SurfaceDescriptionInputs output;
//             ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
//         #ifdef HAVE_VFX_MODIFICATION
//         #if VFX_USE_GRAPH_VALUES
//             uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
//             /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
//         #endif
//             /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
//         #endif
        
            
        
        
        
        
        
        
//             #if UNITY_UV_STARTS_AT_TOP
//             #else
//             #endif
        
        
//         #if UNITY_ANY_INSTANCING_ENABLED
//         #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
//         #endif
//         #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
//         #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
//         #else
//         #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
//         #endif
//         #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
//                 return output;
//         }
        
//         // --------------------------------------------------
//         // Main
        
//         #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShadowCasterPass.hlsl"
        
//         // --------------------------------------------------
//         // Visual Effect Vertex Invocations
//         #ifdef HAVE_VFX_MODIFICATION
//         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
//         #endif
        
//         ENDHLSL
//         }
//         Pass
//         {
//             Name "GBuffer"
//             Tags
//             {
//                 "LightMode" = "UniversalGBuffer"
//             }
        
//         // Render State
//         Cull Back
//         Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
//         ZTest LEqual
//         ZWrite Off
        
//         // Debug
//         // <None>
        
//         // --------------------------------------------------
//         // Pass
        
//         HLSLPROGRAM
        
//         // Pragmas
//         #pragma target 4.5
//         #pragma exclude_renderers gles3 glcore
//         #pragma multi_compile_instancing
//         #pragma instancing_options renderinglayer
//         #pragma vertex vert
//         #pragma fragment frag
        
//         // Keywords
//         #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
//         #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
//         #pragma multi_compile_fragment _ _RENDER_PASS_ENABLED
//         #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
//         #pragma multi_compile _ SHADOWS_SHADOWMASK
//         // GraphKeywords: <None>
        
//         // Defines
        
//         #define ATTRIBUTES_NEED_NORMAL
//         #define ATTRIBUTES_NEED_TANGENT
//         #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
//         #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
//         #define VARYINGS_NEED_POSITION_WS
//         #define VARYINGS_NEED_NORMAL_WS
//         #define FEATURES_GRAPH_VERTEX
//         /* WARNING: $splice Could not find named fragment 'PassInstancing' */
//         #define SHADERPASS SHADERPASS_GBUFFER
//         #define _SURFACE_TYPE_TRANSPARENT 1
        
        
//         // custom interpolator pre-include
//         /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
//         // Includes
//         #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
//         #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
//         #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
//         #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
//         #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
//         #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
//         #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
//         #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
//         // --------------------------------------------------
//         // Structs and Packing
        
//         // custom interpolators pre packing
//         /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
//         struct Attributes
//         {
//              float3 positionOS : POSITION;
//              float3 normalOS : NORMAL;
//              float4 tangentOS : TANGENT;
//             #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
//              uint instanceID : INSTANCEID_SEMANTIC;
//             #endif
//         };
//         struct Varyings
//         {
//              float4 positionCS : SV_POSITION;
//              float3 positionWS;
//              float3 normalWS;
//             #if !defined(LIGHTMAP_ON)
//              float3 sh;
//             #endif
//             #if defined(USE_APV_PROBE_OCCLUSION)
//              float4 probeOcclusion;
//             #endif
//             #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
//              uint instanceID : CUSTOM_INSTANCE_ID;
//             #endif
//             #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
//              uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
//             #endif
//             #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
//              uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
//             #endif
//             #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
//              FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
//             #endif
//         };
//         struct SurfaceDescriptionInputs
//         {
//         };
//         struct VertexDescriptionInputs
//         {
//              float3 ObjectSpaceNormal;
//              float3 ObjectSpaceTangent;
//              float3 ObjectSpacePosition;
//         };
//         struct PackedVaryings
//         {
//              float4 positionCS : SV_POSITION;
//             #if !defined(LIGHTMAP_ON)
//              float3 sh : INTERP0;
//             #endif
//             #if defined(USE_APV_PROBE_OCCLUSION)
//              float4 probeOcclusion : INTERP1;
//             #endif
//              float3 positionWS : INTERP2;
//              float3 normalWS : INTERP3;
//             #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
//              uint instanceID : CUSTOM_INSTANCE_ID;
//             #endif
//             #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
//              uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
//             #endif
//             #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
//              uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
//             #endif
//             #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
//              FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
//             #endif
//         };
        
//         PackedVaryings PackVaryings (Varyings input)
//         {
//             PackedVaryings output;
//             ZERO_INITIALIZE(PackedVaryings, output);
//             output.positionCS = input.positionCS;
//             #if !defined(LIGHTMAP_ON)
//             output.sh = input.sh;
//             #endif
//             #if defined(USE_APV_PROBE_OCCLUSION)
//             output.probeOcclusion = input.probeOcclusion;
//             #endif
//             output.positionWS.xyz = input.positionWS;
//             output.normalWS.xyz = input.normalWS;
//             #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
//             output.instanceID = input.instanceID;
//             #endif
//             #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
//             output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
//             #endif
//             #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
//             output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
//             #endif
//             #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
//             output.cullFace = input.cullFace;
//             #endif
//             return output;
//         }
        
//         Varyings UnpackVaryings (PackedVaryings input)
//         {
//             Varyings output;
//             output.positionCS = input.positionCS;
//             #if !defined(LIGHTMAP_ON)
//             output.sh = input.sh;
//             #endif
//             #if defined(USE_APV_PROBE_OCCLUSION)
//             output.probeOcclusion = input.probeOcclusion;
//             #endif
//             output.positionWS = input.positionWS.xyz;
//             output.normalWS = input.normalWS.xyz;
//             #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
//             output.instanceID = input.instanceID;
//             #endif
//             #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
//             output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
//             #endif
//             #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
//             output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
//             #endif
//             #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
//             output.cullFace = input.cullFace;
//             #endif
//             return output;
//         }
        
        
//         // --------------------------------------------------
//         // Graph
        
//         // Graph Properties
//         CBUFFER_START(UnityPerMaterial)
//         float4 _FieldBaseColor;
//         float _Opacity;
//         float2 _OpacityRemapDistanceRange;
//         float _NegWaveFrequency;
//         float _NegWaveSpeed;
//         UNITY_TEXTURE_STREAMING_DEBUG_VARS;
//         CBUFFER_END
        
//         #if defined(DOTS_INSTANCING_ON)
//         // DOTS instancing definitions
//         UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
//             UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float, _Opacity)
//         UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)
//         // DOTS instancing usage macros
//         #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(type, var)
//         #elif defined(UNITY_INSTANCING_ENABLED)
//         // Unity instancing definitions
//         UNITY_INSTANCING_BUFFER_START(SGPerInstanceData)
//             UNITY_DEFINE_INSTANCED_PROP(float, _Opacity)
//         UNITY_INSTANCING_BUFFER_END(SGPerInstanceData)
//         // Unity instancing usage macros
//         #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) UNITY_ACCESS_INSTANCED_PROP(SGPerInstanceData, var)
//         #else
//         #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) var
//         #endif
        
//         // Object and Global properties
//         float3 _PlayerPosMag;
//         float3 _PlayerNegMag;
//         float3 _PlayerNegMagScreen;
        
//         // Graph Includes
//         // GraphIncludes: <None>
        
//         // -- Property used by ScenePickingPass
//         #ifdef SCENEPICKINGPASS
//         float4 _SelectionID;
//         #endif
        
//         // -- Properties used by SceneSelectionPass
//         #ifdef SCENESELECTIONPASS
//         int _ObjectId;
//         int _PassValue;
//         #endif
        
//         // Graph Functions
//         // GraphFunctions: <None>
        
//         // Custom interpolators pre vertex
//         /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
//         // Graph Vertex
//         struct VertexDescription
//         {
//             float3 Position;
//             float3 Normal;
//             float3 Tangent;
//         };
        
//         VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
//         {
//             VertexDescription description = (VertexDescription)0;
//             description.Position = IN.ObjectSpacePosition;
//             description.Normal = IN.ObjectSpaceNormal;
//             description.Tangent = IN.ObjectSpaceTangent;
//             return description;
//         }
        
//         // Custom interpolators, pre surface
//         #ifdef FEATURES_GRAPH_VERTEX
//         Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
//         {
//         return output;
//         }
//         #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
//         #endif
        
//         // Graph Pixel
//         struct SurfaceDescription
//         {
//             float3 BaseColor;
//             float Alpha;
//         };
        
//         SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
//         {
//             SurfaceDescription surface = (SurfaceDescription)0;
//             float4 _Property_51d1e656146441f4a966a18881f32006_Out_0_Vector4 = _FieldBaseColor;
//             surface.BaseColor = (_Property_51d1e656146441f4a966a18881f32006_Out_0_Vector4.xyz);
//             surface.Alpha = float(0.25);
//             return surface;
//         }
        
//         // --------------------------------------------------
//         // Build Graph Inputs
//         #ifdef HAVE_VFX_MODIFICATION
//         #define VFX_SRP_ATTRIBUTES Attributes
//         #define VFX_SRP_VARYINGS Varyings
//         #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
//         #endif
//         VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
//         {
//             VertexDescriptionInputs output;
//             ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
//             output.ObjectSpaceNormal =                          input.normalOS;
//             output.ObjectSpaceTangent =                         input.tangentOS.xyz;
//             output.ObjectSpacePosition =                        input.positionOS;
//         #if UNITY_ANY_INSTANCING_ENABLED
//         #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
//         #endif
        
//             return output;
//         }
//         SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
//         {
//             SurfaceDescriptionInputs output;
//             ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
//         #ifdef HAVE_VFX_MODIFICATION
//         #if VFX_USE_GRAPH_VALUES
//             uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
//             /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
//         #endif
//             /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
//         #endif
        
            
        
        
        
        
        
        
//             #if UNITY_UV_STARTS_AT_TOP
//             #else
//             #endif
        
        
//         #if UNITY_ANY_INSTANCING_ENABLED
//         #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
//         #endif
//         #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
//         #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
//         #else
//         #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
//         #endif
//         #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
//                 return output;
//         }
        
//         // --------------------------------------------------
//         // Main
        
//         #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/UnlitGBufferPass.hlsl"
        
//         // --------------------------------------------------
//         // Visual Effect Vertex Invocations
//         #ifdef HAVE_VFX_MODIFICATION
//         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
//         #endif
        
//         ENDHLSL
//         }
//         Pass
//         {
//             Name "SceneSelectionPass"
//             Tags
//             {
//                 "LightMode" = "SceneSelectionPass"
//             }
        
//         // Render State
//         Cull Off
        
//         // Debug
//         // <None>
        
//         // --------------------------------------------------
//         // Pass
        
//         HLSLPROGRAM
        
//         // Pragmas
//         #pragma target 2.0
//         #pragma vertex vert
//         #pragma fragment frag
        
//         // Keywords
//         // PassKeywords: <None>
//         // GraphKeywords: <None>
        
//         // Defines
        
//         #define ATTRIBUTES_NEED_NORMAL
//         #define ATTRIBUTES_NEED_TANGENT
//         #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
//         #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
//         #define FEATURES_GRAPH_VERTEX
//         /* WARNING: $splice Could not find named fragment 'PassInstancing' */
//         #define SHADERPASS SHADERPASS_DEPTHONLY
//         #define SCENESELECTIONPASS 1
//         #define ALPHA_CLIP_THRESHOLD 1
        
        
//         // custom interpolator pre-include
//         /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
//         // Includes
//         #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
//         #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
//         #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
//         #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
//         #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
//         #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
//         #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
//         // --------------------------------------------------
//         // Structs and Packing
        
//         // custom interpolators pre packing
//         /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
//         struct Attributes
//         {
//              float3 positionOS : POSITION;
//              float3 normalOS : NORMAL;
//              float4 tangentOS : TANGENT;
//             #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
//              uint instanceID : INSTANCEID_SEMANTIC;
//             #endif
//         };
//         struct Varyings
//         {
//              float4 positionCS : SV_POSITION;
//             #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
//              uint instanceID : CUSTOM_INSTANCE_ID;
//             #endif
//             #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
//              uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
//             #endif
//             #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
//              uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
//             #endif
//             #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
//              FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
//             #endif
//         };
//         struct SurfaceDescriptionInputs
//         {
//         };
//         struct VertexDescriptionInputs
//         {
//              float3 ObjectSpaceNormal;
//              float3 ObjectSpaceTangent;
//              float3 ObjectSpacePosition;
//         };
//         struct PackedVaryings
//         {
//              float4 positionCS : SV_POSITION;
//             #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
//              uint instanceID : CUSTOM_INSTANCE_ID;
//             #endif
//             #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
//              uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
//             #endif
//             #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
//              uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
//             #endif
//             #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
//              FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
//             #endif
//         };
        
//         PackedVaryings PackVaryings (Varyings input)
//         {
//             PackedVaryings output;
//             ZERO_INITIALIZE(PackedVaryings, output);
//             output.positionCS = input.positionCS;
//             #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
//             output.instanceID = input.instanceID;
//             #endif
//             #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
//             output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
//             #endif
//             #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
//             output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
//             #endif
//             #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
//             output.cullFace = input.cullFace;
//             #endif
//             return output;
//         }
        
//         Varyings UnpackVaryings (PackedVaryings input)
//         {
//             Varyings output;
//             output.positionCS = input.positionCS;
//             #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
//             output.instanceID = input.instanceID;
//             #endif
//             #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
//             output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
//             #endif
//             #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
//             output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
//             #endif
//             #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
//             output.cullFace = input.cullFace;
//             #endif
//             return output;
//         }
        
        
//         // --------------------------------------------------
//         // Graph
        
//         // Graph Properties
//         CBUFFER_START(UnityPerMaterial)
//         float4 _FieldBaseColor;
//         float _Opacity;
//         float2 _OpacityRemapDistanceRange;
//         float _NegWaveFrequency;
//         float _NegWaveSpeed;
//         UNITY_TEXTURE_STREAMING_DEBUG_VARS;
//         CBUFFER_END
        
//         #if defined(DOTS_INSTANCING_ON)
//         // DOTS instancing definitions
//         UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
//             UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float, _Opacity)
//         UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)
//         // DOTS instancing usage macros
//         #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(type, var)
//         #elif defined(UNITY_INSTANCING_ENABLED)
//         // Unity instancing definitions
//         UNITY_INSTANCING_BUFFER_START(SGPerInstanceData)
//             UNITY_DEFINE_INSTANCED_PROP(float, _Opacity)
//         UNITY_INSTANCING_BUFFER_END(SGPerInstanceData)
//         // Unity instancing usage macros
//         #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) UNITY_ACCESS_INSTANCED_PROP(SGPerInstanceData, var)
//         #else
//         #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) var
//         #endif
        
//         // Object and Global properties
//         float3 _PlayerPosMag;
//         float3 _PlayerNegMag;
//         float3 _PlayerNegMagScreen;
        
//         // Graph Includes
//         // GraphIncludes: <None>
        
//         // -- Property used by ScenePickingPass
//         #ifdef SCENEPICKINGPASS
//         float4 _SelectionID;
//         #endif
        
//         // -- Properties used by SceneSelectionPass
//         #ifdef SCENESELECTIONPASS
//         int _ObjectId;
//         int _PassValue;
//         #endif
        
//         // Graph Functions
//         // GraphFunctions: <None>
        
//         // Custom interpolators pre vertex
//         /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
//         // Graph Vertex
//         struct VertexDescription
//         {
//             float3 Position;
//             float3 Normal;
//             float3 Tangent;
//         };
        
//         VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
//         {
//             VertexDescription description = (VertexDescription)0;
//             description.Position = IN.ObjectSpacePosition;
//             description.Normal = IN.ObjectSpaceNormal;
//             description.Tangent = IN.ObjectSpaceTangent;
//             return description;
//         }
        
//         // Custom interpolators, pre surface
//         #ifdef FEATURES_GRAPH_VERTEX
//         Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
//         {
//         return output;
//         }
//         #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
//         #endif
        
//         // Graph Pixel
//         struct SurfaceDescription
//         {
//             float Alpha;
//         };
        
//         SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
//         {
//             SurfaceDescription surface = (SurfaceDescription)0;
//             surface.Alpha = float(0.25);
//             return surface;
//         }
        
//         // --------------------------------------------------
//         // Build Graph Inputs
//         #ifdef HAVE_VFX_MODIFICATION
//         #define VFX_SRP_ATTRIBUTES Attributes
//         #define VFX_SRP_VARYINGS Varyings
//         #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
//         #endif
//         VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
//         {
//             VertexDescriptionInputs output;
//             ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
//             output.ObjectSpaceNormal =                          input.normalOS;
//             output.ObjectSpaceTangent =                         input.tangentOS.xyz;
//             output.ObjectSpacePosition =                        input.positionOS;
//         #if UNITY_ANY_INSTANCING_ENABLED
//         #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
//         #endif
        
//             return output;
//         }
//         SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
//         {
//             SurfaceDescriptionInputs output;
//             ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
//         #ifdef HAVE_VFX_MODIFICATION
//         #if VFX_USE_GRAPH_VALUES
//             uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
//             /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
//         #endif
//             /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
//         #endif
        
            
        
        
        
        
        
        
//             #if UNITY_UV_STARTS_AT_TOP
//             #else
//             #endif
        
        
//         #if UNITY_ANY_INSTANCING_ENABLED
//         #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
//         #endif
//         #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
//         #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
//         #else
//         #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
//         #endif
//         #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
//                 return output;
//         }
        
//         // --------------------------------------------------
//         // Main
        
//         #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SelectionPickingPass.hlsl"
        
//         // --------------------------------------------------
//         // Visual Effect Vertex Invocations
//         #ifdef HAVE_VFX_MODIFICATION
//         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
//         #endif
        
//         ENDHLSL
//         }
//         Pass
//         {
//             Name "ScenePickingPass"
//             Tags
//             {
//                 "LightMode" = "Picking"
//             }
        
//         // Render State
//         Cull Back
        
//         // Debug
//         // <None>
        
//         // --------------------------------------------------
//         // Pass
        
//         HLSLPROGRAM
        
//         // Pragmas
//         #pragma target 2.0
//         #pragma vertex vert
//         #pragma fragment frag
        
//         // Keywords
//         // PassKeywords: <None>
//         // GraphKeywords: <None>
        
//         // Defines
        
//         #define ATTRIBUTES_NEED_NORMAL
//         #define ATTRIBUTES_NEED_TANGENT
//         #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
//         #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
//         #define FEATURES_GRAPH_VERTEX
//         /* WARNING: $splice Could not find named fragment 'PassInstancing' */
//         #define SHADERPASS SHADERPASS_DEPTHONLY
//         #define SCENEPICKINGPASS 1
//         #define ALPHA_CLIP_THRESHOLD 1
        
        
//         // custom interpolator pre-include
//         /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
//         // Includes
//         #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
//         #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
//         #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
//         #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
//         #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
//         #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
//         #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
//         // --------------------------------------------------
//         // Structs and Packing
        
//         // custom interpolators pre packing
//         /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
//         struct Attributes
//         {
//              float3 positionOS : POSITION;
//              float3 normalOS : NORMAL;
//              float4 tangentOS : TANGENT;
//             #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
//              uint instanceID : INSTANCEID_SEMANTIC;
//             #endif
//         };
//         struct Varyings
//         {
//              float4 positionCS : SV_POSITION;
//             #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
//              uint instanceID : CUSTOM_INSTANCE_ID;
//             #endif
//             #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
//              uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
//             #endif
//             #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
//              uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
//             #endif
//             #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
//              FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
//             #endif
//         };
//         struct SurfaceDescriptionInputs
//         {
//         };
//         struct VertexDescriptionInputs
//         {
//              float3 ObjectSpaceNormal;
//              float3 ObjectSpaceTangent;
//              float3 ObjectSpacePosition;
//         };
//         struct PackedVaryings
//         {
//              float4 positionCS : SV_POSITION;
//             #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
//              uint instanceID : CUSTOM_INSTANCE_ID;
//             #endif
//             #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
//              uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
//             #endif
//             #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
//              uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
//             #endif
//             #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
//              FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
//             #endif
//         };
        
//         PackedVaryings PackVaryings (Varyings input)
//         {
//             PackedVaryings output;
//             ZERO_INITIALIZE(PackedVaryings, output);
//             output.positionCS = input.positionCS;
//             #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
//             output.instanceID = input.instanceID;
//             #endif
//             #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
//             output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
//             #endif
//             #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
//             output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
//             #endif
//             #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
//             output.cullFace = input.cullFace;
//             #endif
//             return output;
//         }
        
//         Varyings UnpackVaryings (PackedVaryings input)
//         {
//             Varyings output;
//             output.positionCS = input.positionCS;
//             #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
//             output.instanceID = input.instanceID;
//             #endif
//             #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
//             output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
//             #endif
//             #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
//             output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
//             #endif
//             #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
//             output.cullFace = input.cullFace;
//             #endif
//             return output;
//         }
        
        
//         // --------------------------------------------------
//         // Graph
        
//         // Graph Properties
//         CBUFFER_START(UnityPerMaterial)
//         float4 _FieldBaseColor;
//         float _Opacity;
//         float2 _OpacityRemapDistanceRange;
//         float _NegWaveFrequency;
//         float _NegWaveSpeed;
//         UNITY_TEXTURE_STREAMING_DEBUG_VARS;
//         CBUFFER_END
        
//         #if defined(DOTS_INSTANCING_ON)
//         // DOTS instancing definitions
//         UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
//             UNITY_DOTS_INSTANCED_PROP_OVERRIDE_SUPPORTED(float, _Opacity)
//         UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)
//         // DOTS instancing usage macros
//         #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) UNITY_ACCESS_DOTS_INSTANCED_PROP_WITH_DEFAULT(type, var)
//         #elif defined(UNITY_INSTANCING_ENABLED)
//         // Unity instancing definitions
//         UNITY_INSTANCING_BUFFER_START(SGPerInstanceData)
//             UNITY_DEFINE_INSTANCED_PROP(float, _Opacity)
//         UNITY_INSTANCING_BUFFER_END(SGPerInstanceData)
//         // Unity instancing usage macros
//         #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) UNITY_ACCESS_INSTANCED_PROP(SGPerInstanceData, var)
//         #else
//         #define UNITY_ACCESS_HYBRID_INSTANCED_PROP(var, type) var
//         #endif
        
//         // Object and Global properties
//         float3 _PlayerPosMag;
//         float3 _PlayerNegMag;
//         float3 _PlayerNegMagScreen;
        
//         // Graph Includes
//         // GraphIncludes: <None>
        
//         // -- Property used by ScenePickingPass
//         #ifdef SCENEPICKINGPASS
//         float4 _SelectionID;
//         #endif
        
//         // -- Properties used by SceneSelectionPass
//         #ifdef SCENESELECTIONPASS
//         int _ObjectId;
//         int _PassValue;
//         #endif
        
//         // Graph Functions
//         // GraphFunctions: <None>
        
//         // Custom interpolators pre vertex
//         /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
//         // Graph Vertex
//         struct VertexDescription
//         {
//             float3 Position;
//             float3 Normal;
//             float3 Tangent;
//         };
        
//         VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
//         {
//             VertexDescription description = (VertexDescription)0;
//             description.Position = IN.ObjectSpacePosition;
//             description.Normal = IN.ObjectSpaceNormal;
//             description.Tangent = IN.ObjectSpaceTangent;
//             return description;
//         }
        
//         // Custom interpolators, pre surface
//         #ifdef FEATURES_GRAPH_VERTEX
//         Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
//         {
//         return output;
//         }
//         #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
//         #endif
        
//         // Graph Pixel
//         struct SurfaceDescription
//         {
//             float3 BaseColor;
//             float Alpha;
//         };
        
//         SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
//         {
//             SurfaceDescription surface = (SurfaceDescription)0;
//             float4 _Property_51d1e656146441f4a966a18881f32006_Out_0_Vector4 = _FieldBaseColor;
//             surface.BaseColor = (_Property_51d1e656146441f4a966a18881f32006_Out_0_Vector4.xyz);
//             surface.Alpha = float(0.25);
//             return surface;
//         }
        
//         // --------------------------------------------------
//         // Build Graph Inputs
//         #ifdef HAVE_VFX_MODIFICATION
//         #define VFX_SRP_ATTRIBUTES Attributes
//         #define VFX_SRP_VARYINGS Varyings
//         #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
//         #endif
//         VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
//         {
//             VertexDescriptionInputs output;
//             ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
//             output.ObjectSpaceNormal =                          input.normalOS;
//             output.ObjectSpaceTangent =                         input.tangentOS.xyz;
//             output.ObjectSpacePosition =                        input.positionOS;
//         #if UNITY_ANY_INSTANCING_ENABLED
//         #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
//         #endif
        
//             return output;
//         }
//         SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
//         {
//             SurfaceDescriptionInputs output;
//             ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
//         #ifdef HAVE_VFX_MODIFICATION
//         #if VFX_USE_GRAPH_VALUES
//             uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
//             /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
//         #endif
//             /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
//         #endif
        
            
        
        
        
        
        
        
//             #if UNITY_UV_STARTS_AT_TOP
//             #else
//             #endif
        
        
//         #if UNITY_ANY_INSTANCING_ENABLED
//         #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
//         #endif
//         #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
//         #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
//         #else
//         #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
//         #endif
//         #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
//                 return output;
//         }
        
//         // --------------------------------------------------
//         // Main
        
//         #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
//         #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SelectionPickingPass.hlsl"
        
//         // --------------------------------------------------
//         // Visual Effect Vertex Invocations
//         #ifdef HAVE_VFX_MODIFICATION
//         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
//         #endif
        
//         ENDHLSL
//         }
//     }
//     CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
//     CustomEditorForRenderPipeline "UnityEditor.ShaderGraphUnlitGUI" "UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset"
//     FallBack "Hidden/Shader Graph/FallbackError"
// }