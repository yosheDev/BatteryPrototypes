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
public class MagFieldRTRendererFeatureSettings
{
    [Header("Properties")]
    public Material _maskMaterial { get; set; }
    public LayerMask _layerMask { get; set; }
    public RenderQueueRange _renderQueueRange { get; set; }
}

public class MagFieldRTRendererFeature : ScriptableRendererFeature
{
    public enum FilterRenderQueueRange
    {
        All,
        Opaque,
        Transparent
    }
    public MagFieldRTRendererFeatureSettings settings = new MagFieldRTRendererFeatureSettings();
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

    class MagFieldRTRendererFeaturePass : ScriptableRenderPass
    {
        readonly MagFieldRTRendererFeatureSettings settings;
        private int globalTextureID = Shader.PropertyToID("_MagFieldMask");

        public MagFieldRTRendererFeaturePass(MagFieldRTRendererFeatureSettings settings)
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
            const string passName = "Magnetic Field Mask Pass";
            const string globalShaderIdentifier = "_MagFieldMask";

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

                #region My Method First Attempt
                //// NOTE: The reason it is probably passing NOTHING through the render texture is probably related to how the render texture is created / setup. Its curing cameraColor? Is this fine? Learn more about this.
                //// Create Destination Texture
                //var desc = renderGraph.GetTextureDesc(resourceData.cameraColor);
                ////desc.width = Screen.width; // i added
                ////desc.height = Screen.height; //i added
                //desc.colorFormat = GraphicsFormat.R8_UInt; // I added
                //desc.name = passName;
                //desc.clearBuffer = false;
                //TextureHandle cameraCopyTexture = renderGraph.CreateTexture(desc);

                // Blit
                //renderGraph.AddBlitPass(resourceData.cameraColor, cameraCopyTexture, Vector2.one, Vector2.zero, passName: "Mag Field Mask");
                //var blitParams = new RenderGraphUtils.BlitMaterialParameters(resourceData.cameraColor, cameraCopyTexture, settings._maskMaterial, 0);
                //renderGraph.AddBlitPass(blitParams, passName: "Mag Field Mask");

                #region Renderer List (Material, Layermask)

                //===[ Settings for which GameObjects to render in this pass. ]===

                #region Create Render Settings

                //// Culling Settings
                //var cullContextData = frameData.Get<CullContextData>();
                //cameraData.camera.TryGetCullingParameters(false, out var cullingParams);
                //CullingResults cullingResults = cullContextData.Cull(ref cullingParams);

                //// Drawing Settings (Override Material)
                //// NOTE: I am unsure of what string to pass through when creating the ShaderTagID. https://docs.unity3d.com/6000.3/Documentation/Manual/SL-PassTags.html
                //DrawingSettings drawingSettings = new DrawingSettings(new ShaderTagId("Always"), new SortingSettings(cameraData.camera));
                //drawingSettings.overrideMaterial = settings._maskMaterial; /// Apply the mask material.

                //// Filtering Settings (LayerMask)
                //FilteringSettings filteringSettings = new FilteringSettings(settings._renderQueueRange, settings._layerMask); // Set to transparent since this is intended for the magnetic fields.

                #endregion

                //// Create RendererListParams struct consisting of settings above.
                //RendererListParams rendererListParams = new RendererListParams(cullingResults, drawingSettings, filteringSettings);

                //// Create renderer list for this pass.
                //RendererListHandle rendererListHandle = renderGraph.CreateRendererList(rendererListParams);

                #endregion

                //// Setup passData to have correct rendererListHandle.
                //passData.rendererListHandle = rendererListHandle;
                //passData.source = cameraCopyTexture;

                #endregion 

                #region Attempt #2
                // Create descriptor for occluders texture with scaled resolution
                RenderTextureDescriptor maskDesc = cameraData.cameraTargetDescriptor;
                maskDesc.depthBufferBits = 0;
                maskDesc.width = Mathf.RoundToInt(maskDesc.width * 1f);
                maskDesc.height = Mathf.RoundToInt(maskDesc.height * 1f);

                // Create the occluders texture handle
                TextureHandle maskTexture = UniversalRenderer.CreateRenderGraphTexture(
                    renderGraph,
                    maskDesc,
                    globalShaderIdentifier,
                    false,
                    FilterMode.Bilinear,
                    TextureWrapMode.Clamp
                );

                //create renderer list to draw occluders
                // NOTE: The ShaderTagID might be wrong and unable to do transparents?? idk this should be for drawing though not filtering so i doubt it.
                DrawingSettings drawingSettings = RenderingUtils.CreateDrawingSettings(
                    // "UniversalForward" will not work, as the material is created with ShaderGraph (which has NO lightmode tags for main pass.) ShadowCaster is one that is included, so I am doing that one.
                    // Other potential ones?, "MotionVectors", "DepthNormalsOnly", "UniversalGBuffer", 
                    new ShaderTagId("ShadowCaster"), //https://docs.unity3d.com/6000.3/Documentation/Manual/SL-PassTags.html    https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@10.1/manual/urp-shaders/urp-shaderlab-pass-tags.html
                    renderingData,
                    cameraData,
                    lightData,
                    SortingCriteria.BackToFront
                );

                drawingSettings.overrideMaterial = settings._maskMaterial;

                //// Culling Settings Attempt #1
                //var cullContextData = frameData.Get<CullContextData>();
                //cameraData.camera.TryGetCullingParameters(false, out var cullingParams);
                //CullingResults cullingResults = cullContextData.Cull(ref cullingParams);

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
