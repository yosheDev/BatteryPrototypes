using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public class MagFieldRTRendererFeatuer : ScriptableRendererFeature
{
    public enum FilterRenderQueueRange
    {
        All,
        Opaque,
        Transparent
    }
    [SerializeField] MagFieldRTRendererFeatureSettings settings;
    MagFieldRTRendererFeaturePass m_ScriptablePass;
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
        m_ScriptablePass = new MagFieldRTRendererFeaturePass(settings);

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;

        // You can request URP color texture and depth buffer as inputs by uncommenting the line below,
        // URP will ensure copies of these resources are available for sampling before executing the render pass.
        // Only uncomment it if necessary, it will have a performance impact, especially on mobiles and other TBDR GPUs where it will break render passes.
        //m_ScriptablePass.ConfigureInput(ScriptableRenderPassInput.Color | ScriptableRenderPassInput.Depth);

        // You can request URP to render to an intermediate texture by uncommenting the line below.
        // Use this option for passes that do not support rendering directly to the backbuffer.
        // Only uncomment it if necessary, it will have a performance impact, especially on mobiles and other TBDR GPUs where it will break render passes.
        //m_ScriptablePass.requiresIntermediateTexture = true;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }

    // Use this class to pass around settings from the feature to the pass
    [Serializable]
    public class MagFieldRTRendererFeatureSettings
    {
        public Material _maskMaterial { get; set; }
        public LayerMask _layerMask { get; set; }

        public RenderQueueRange _renderQueueRange { get; set; }
    }

    class MagFieldRTRendererFeaturePass : ScriptableRenderPass
    {
        readonly MagFieldRTRendererFeatureSettings settings;
        private int globalTextureID = Shader.PropertyToID("_MagFieldMask");

        public MagFieldRTRendererFeaturePass(MagFieldRTRendererFeatureSettings settings)
        {
            this.settings = settings;
        }

        // This class stores the data needed by the RenderGraph pass.
        // It is passed as a parameter to the delegate function that executes the RenderGraph pass.
        // NOTE: This might be left blank?
        private class PassData
        {
            
        }

        // This static method is passed as the RenderFunc delegate to the RenderGraph render pass.
        // It is used to execute draw commands.
        // NOTE: I don't think I will need to execute any draw commands for this so itll probably be left blank.
        // ACTUALLY: If I am trying to mask, I will probably need to redraw with a specific material for white/black based on the stencil value.
        static void ExecutePass(PassData data, RasterGraphContext context)
        {
            
        }

        // RecordRenderGraph is where the RenderGraph handle can be accessed, through which render passes can be added to the graph.
        // FrameData is a context container through which URP resources can be accessed and managed.
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            #region Setup
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            // Create Destination Texture
            var desc = renderGraph.GetTextureDesc(resourceData.cameraColor);
            desc.name = "_MagFieldMask";
            desc.clearBuffer = false;
            TextureHandle cameraCopyTexture = renderGraph.CreateTexture(desc);

            // Blit
            renderGraph.AddBlitPass(resourceData.cameraColor, cameraCopyTexture, Vector2.one, Vector2.zero, passName: "Mag Field Mask");

            // Alternatively, using AddCopyPass
            // Can be more performant, but requires source/destination to be the same size in pixels and same number of MSAA samples and array slices
            if (RenderGraphUtils.CanAddCopyPassMSAA())
            {
                renderGraph.AddCopyPass(resourceData.cameraColor, cameraCopyTexture, passName: "Copy Color");
            }
            else
            {
                // cannot copy due to MSAA, I guess just fallback to blit
                renderGraph.AddBlitPass(resourceData.cameraColor, cameraCopyTexture, Vector2.one, Vector2.zero, passName: "Copy Color");
            }

            #region Renderer List (Material, Layermask)

            //===[ Settings for which GameObjects to render in this pass. ]===

            #region Create Render Settings

            // Culling Settings
            var cullContextData = frameData.Get<CullContextData>();
            cameraData.camera.TryGetCullingParameters(false, out var cullingParams);
            CullingResults cullingResults = cullContextData.Cull(ref cullingParams);

            // Drawing Settings (Override Material)
            // NOTE: I am unsure of what string to pass through when creating the ShaderTagID. https://docs.unity3d.com/6000.3/Documentation/Manual/SL-PassTags.html
            DrawingSettings drawingSettings = new DrawingSettings(new ShaderTagId("Always"), new SortingSettings(cameraData.camera));
            drawingSettings.overrideMaterial = settings._maskMaterial; /// Apply the mask material.

            // Filtering Settings (LayerMask)
            FilteringSettings filteringSettings = new FilteringSettings(settings._renderQueueRange, settings._layerMask); // Set to transparent since this is intended for the magnetic fields.

            #endregion

            // Create RendererListParams struct consisting of settings above.
            RendererListParams rendererListParams = new RendererListParams(cullingResults, drawingSettings, filteringSettings);

            // Create renderer list for this pass.
            RendererListHandle rendererListHandle = renderGraph.CreateRendererList(rendererListParams);

            #endregion

            #endregion

            // Use texture in another pass, or set as global texture. e.g.
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Set Global Texture", out var passData))
            {
                // Use the renderer list defined above.
                builder.UseRendererList(rendererListHandle);

                // Set a texture to the global texture
                builder.SetGlobalTextureAfterPass(cameraCopyTexture, globalTextureID);

                builder.AllowPassCulling(false);
                // this is to prevent RenderGraph from removing this RasterRenderPass
                // If another pass uses builder.UseGlobalTexture(globalTextureID) this might not be required

                builder.SetRenderFunc((PassData data, RasterGraphContext context) => { });
            }
        }
    }
}
