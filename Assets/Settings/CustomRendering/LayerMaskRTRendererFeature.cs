using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;
using static UnityEngine.Rendering.Universal.ShaderInput;

// Use this class to pass around settings from the feature to the pass
[System.Serializable]
public class LayerMaskRTRendererFeatureSettings
{
    [Header("Properties")]
    public Material _maskMaterial { get; set; }
    public LayerMask _layerMask { get; set; }
    public RenderQueueRange _renderQueueRange { get; set; }
}

public class LayerMaskRTRendererFeature : ScriptableRendererFeature
{
    public enum FilterRenderQueueRange
    {
        All,
        Opaque,
        Transparent
    }
    public const string passName = "Layer Mask Pass";
    public const string outputGlobalPropertyName = "_MagFieldMask";
    public const string shaderTagID = "ShadowCaster";
    public LayerMaskRTRendererFeatureSettings settings = new LayerMaskRTRendererFeatureSettings();
    LayerMaskRTRendererFeaturePass m_ScriptablePass;
    public Material maskMaterial;
    public LayerMask layerMask;
    public FilterRenderQueueRange filterRenderQueueRange;
    // This is unable to show in the editor for some reason, so I used enum.
    private RenderQueueRange renderQueueRange = RenderQueueRange.all;
    

    /// <inheritdoc/>
    public override void Create()
    {
        // Assign settings values to be what is set in the editor inspector.
        settings._maskMaterial = maskMaterial;
        settings._layerMask = layerMask;
        settings._renderQueueRange = renderQueueRange;
        switch (filterRenderQueueRange)
        {
            case FilterRenderQueueRange.All:
                settings._renderQueueRange = RenderQueueRange.all;
                break;
            case FilterRenderQueueRange.Opaque:
                settings._renderQueueRange = RenderQueueRange.opaque;
                break;
            case FilterRenderQueueRange.Transparent:
                settings._renderQueueRange = RenderQueueRange.transparent;
                break;
            default:
                settings._renderQueueRange = RenderQueueRange.all;
                break;
        }
        m_ScriptablePass = new LayerMaskRTRendererFeaturePass(settings);

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

        #region Possible Extra Stuff?
        // You can request URP color texture and depth buffer as inputs by uncommenting the line below,
        // URP will ensure copies of these resources are available for sampling before executing the render pass.
        // Only uncomment it if necessary, it will have a performance impact, especially on mobiles and other TBDR GPUs where it will break render passes.
        //m_ScriptablePass.ConfigureInput(ScriptableRenderPassInput.Color | ScriptableRenderPassInput.Depth);

        // You can request URP to render to an intermediate texture by uncommenting the line below.
        // Use this option for passes that do not support rendering directly to the backbuffer.
        // Only uncomment it if necessary, it will have a performance impact, especially on mobiles and other TBDR GPUs where it will break render passes.
        //m_ScriptablePass.requiresIntermediateTexture = true;
        #endregion
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        renderer.EnqueuePass(m_ScriptablePass);
    }

    class LayerMaskRTRendererFeaturePass : ScriptableRenderPass
    {
        readonly LayerMaskRTRendererFeatureSettings settings;
        private int globalTextureID = Shader.PropertyToID(outputGlobalPropertyName);

        public LayerMaskRTRendererFeaturePass(LayerMaskRTRendererFeatureSettings settings)
        {
            this.settings = settings;
        }
        
        private class PassData
        {
            // This class is passed as a parameter to the delegate function that executes the RenderGraph pass.
            public TextureHandle source;
            public TextureHandle mask;
            public RendererListHandle rendererListHandle;
            public Material overrideMaterial;
        }

        static void ExecutePass(PassData data, RasterGraphContext context)
        {
            // Used to execute draw commands.
            context.cmd.ClearRenderTarget(true, true, Color.black); // Draw black background.
            context.cmd.DrawRendererList(data.rendererListHandle); // Draw rendererList as override material (material is white).
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            #region Old AddCopyPass Test
            // Alternatively, using AddCopyPass
            // Can be more performant, but requires source/destination to be the same size in pixels and same number of MSAA samples and array slices
            //if (RenderGraphUtils.CanAddCopyPassMSAA())
            //{
            //    renderGraph.AddCopyPass(resourceData.cameraColor, cameraCopyTexture, passName: "Copy Color Alt Pass");
            //}
            //else
            //{
            //    // cannot copy due to MSAA, I guess just fallback to blit
            //    renderGraph.AddBlitPass(resourceData.cameraColor, cameraCopyTexture, Vector2.one, Vector2.zero, passName: "Copy Color Alt Pass");
            //}
            #endregion

            using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData))
            {
                // Access camera and resource data.
                UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
                UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>(); // May not need this one?
                UniversalLightData lightData = frameData.Get<UniversalLightData>(); // May not need this one?

                #region Setup
                // Create descriptor for occluders texture with scaled resolution
                RenderTextureDescriptor maskDesc = cameraData.cameraTargetDescriptor;
                maskDesc.depthBufferBits = 0;
                maskDesc.width = Mathf.RoundToInt(maskDesc.width * 1f);
                maskDesc.height = Mathf.RoundToInt(maskDesc.height * 1f);

                // Create the occluders texture handle
                TextureHandle maskTexture = UniversalRenderer.CreateRenderGraphTexture(
                    renderGraph,
                    maskDesc,
                    outputGlobalPropertyName,
                    false,
                    FilterMode.Bilinear,
                    TextureWrapMode.Clamp
                );

                #region Drawing Settings (Material, Target Shaders, Sorting Criteria)
                DrawingSettings drawingSettings = RenderingUtils.CreateDrawingSettings(
                    // "UniversalForward" will not work, as the material is created with ShaderGraph (which has NO lightmode tags for main pass.) ShadowCaster is one that is included, so I am doing that one.
                    // Other potential ones?, "MotionVectors", "DepthNormalsOnly", "UniversalGBuffer", 
                    new ShaderTagId(shaderTagID), //https://docs.unity3d.com/6000.3/Documentation/Manual/SL-PassTags.html    https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@10.1/manual/urp-shaders/urp-shaderlab-pass-tags.html
                    renderingData,
                    cameraData,
                    lightData,
                    SortingCriteria.BackToFront
                );
                drawingSettings.overrideMaterial = settings._maskMaterial;
                #endregion

                RendererListParams rendererListParams = new RendererListParams(
                    renderingData.cullResults,
                    drawingSettings,
                    new FilteringSettings(settings._renderQueueRange, settings._layerMask)
                );

                // Set up pass data.
                passData.source = resourceData.activeColorTexture;
                passData.mask = maskTexture;
                passData.rendererListHandle = renderGraph.CreateRendererList(rendererListParams);
                passData.overrideMaterial = settings._maskMaterial;
                #endregion

                // Declare texture usage 
                builder.UseTexture(resourceData.activeColorTexture, AccessFlags.Read);
                builder.UseRendererList(passData.rendererListHandle);
                builder.SetRenderAttachment(maskTexture, 0);

                // Set a texture to the global texture
                builder.SetGlobalTextureAfterPass(maskTexture, globalTextureID); /// Allows to sample this shader later in the rendering process.

                builder.AllowPassCulling(false); /// Prevent RenderGraph from removing this RasterRenderPass

                // Execute the render pass.
                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
            }
        }
    }
}
