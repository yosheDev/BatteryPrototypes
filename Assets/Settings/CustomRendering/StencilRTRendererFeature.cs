using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;
using static UnityEngine.Rendering.Universal.ShaderInput;
using UnityEngine.Scripting.APIUpdating;

// Use this class to pass around settings from the feature to the pass
[System.Serializable]
public class StencilRTRendererFeatureSettings
{
    [Header("Properties")]

    public string _passName = "Stencil Mask Pass";          // This is used for pass name in Render Graph API.

    public string _outputGlobalPropertyName = "_MagFieldMask"; // This is used for the global texture ID.

    public string _shaderTagID = "SRPDefaultUnlit";             // This is used for the shaderTag used in drawing settings. Should(?) match the shader pass LightMode tag being evaluated.(?)

    public int _stencilRef = 0;                             // This is the stencil ref value that will result in the render texture mask.

    public Material _stencilWriteMaterial;                  // This one writes _stencilRef to the stencil buffer over magnetic fields.

    public Material _overrideMaterial;                   // This one is pure white where stencil value passes ZBuffer pass and is equal to _stencilRef.

    public LayerMask _layerMask = ~0;                       // Might become deprecated eventually? Right now it is using layer mask in the drawSettings.

    public RenderQueueRange _renderQueueRange = RenderQueueRange.all; // Might become deprecated? Being used in filtering settings.
}

public class StencilRTRendererFeature : ScriptableRendererFeature
{
    StencilRTRendererFeatureWritePass m_writePass;
    StencilRTRendererFeatureDrawPass m_drawPass;
    [SerializeField] private StencilRTRendererFeatureSettings settings = new StencilRTRendererFeatureSettings();

    /// <inheritdoc/>
    public override void Create()
    {
        // Construct the pass classes with proper settings.
        m_writePass = new StencilRTRendererFeatureWritePass(settings);
        m_drawPass = new StencilRTRendererFeatureDrawPass(settings);

        // Configures where the render pass should be injected.
        m_writePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        m_drawPass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing + 1;

        #region Include Extra Stuff? Might need this.
        // You can request URP color texture and depth buffer as inputs by uncommenting the line below,
        // URP will ensure copies of these resources are available for sampling before executing the render pass.
        // Only uncomment it if necessary, it will have a performance impact, especially on mobiles and other TBDR GPUs where it will break render passes.
        //m_writePass.ConfigureInput(ScriptableRenderPassInput.Depth);
        //m_drawPass.ConfigureInput(ScriptableRenderPassInput.Depth);

        // You can request URP to render to an intermediate texture by uncommenting the line below.
        // Use this option for passes that do not support rendering directly to the backbuffer.
        // Only uncomment it if necessary, it will have a performance impact, especially on mobiles and other TBDR GPUs where it will break render passes.
        //m_writePass.requiresIntermediateTexture = true;
        #endregion
    }

    protected override void Dispose(bool disposing)
    {
        //m_writePass.rendererStateBlocks.Dispose();
        //m_writePass.rendererStateBlockIDs.Dispose();
        //m_drawPass.rendererStateBlocks.Dispose();
        //m_drawPass.rendererStateBlockIDs.Dispose();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        //renderer.EnqueuePass(m_writePass); /// Draws _stencilWriteMaterial over magnetic fields. This material sets their stencil value to 1.
        //m_drawPass.ConfigureInput(ScriptableRenderPassInput.Depth);
        renderer.EnqueuePass(m_drawPass); /// Draws a white unlit shader over any pixel in stencil buffer with a value of 1.
    }

    #region Assign Pass
    class StencilRTRendererFeatureWritePass : ScriptableRenderPass
    {
        readonly StencilRTRendererFeatureSettings settings;

        #region Mine
        //private int globalTextureID = Shader.PropertyToID(outputGlobalPropertyName);
        //public NativeArray<RenderStateBlock> rendererStateBlocks = new NativeArray<RenderStateBlock>(1, Allocator.Persistent);
        //public NativeArray<ShaderTagId> rendererStateBlockIDs = new NativeArray<ShaderTagId>(1, Allocator.Persistent);
        //public StencilRTRendererFeatureWritePass(StencilRTRendererFeatureSettings settings)
        //{
        //    this.settings = settings;
        //}

        //private class PassData
        //{
        //    // This class is passed as a parameter to the delegate function that executes the RenderGraph pass.
        //    public TextureHandle source;
        //    public TextureHandle mask;
        //    public RendererListHandle rendererListHandle;
        //    public Material overrideMaterial;
        //}

        //static void ExecutePass(PassData data, RasterGraphContext context)
        //{
        //    // Used to execute draw commands.
        //    //context.cmd.ClearRenderTarget(true, true, Color.black); // Draw black background.
        //    context.cmd.DrawRendererList(data.rendererListHandle); // Draw rendererList as override material (material is white).
        //}

        //public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        //{
        //    #region Old AddCopyPass Test
        //    // Alternatively, using AddCopyPass
        //    // Can be more performant, but requires source/destination to be the same size in pixels and same number of MSAA samples and array slices
        //    //if (RenderGraphUtils.CanAddCopyPassMSAA())
        //    //{
        //    //    renderGraph.AddCopyPass(resourceData.cameraColor, cameraCopyTexture, passName: "Copy Color Alt Pass");
        //    //}
        //    //else
        //    //{
        //    //    // cannot copy due to MSAA, I guess just fallback to blit
        //    //    renderGraph.AddBlitPass(resourceData.cameraColor, cameraCopyTexture, Vector2.one, Vector2.zero, passName: "Copy Color Alt Pass");
        //    //}
        //    #endregion

        //    using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData))
        //    {
        //        // Access camera and resource data.
        //        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
        //        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
        //        UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>(); // May not need this one?
        //        UniversalLightData lightData = frameData.Get<UniversalLightData>(); // May not need this one?

        //        #region Setup
        //        // Create descriptor for occluders texture with scaled resolution
        //        RenderTextureDescriptor maskDesc = cameraData.cameraTargetDescriptor;
        //        maskDesc.depthBufferBits = 0;
        //        maskDesc.width = Mathf.RoundToInt(maskDesc.width * 1f); // float here affects resolution. 1f means 1x the screen resolution.
        //        maskDesc.height = Mathf.RoundToInt(maskDesc.height * 1f);

        //        // Create the occluders texture handle
        //        TextureHandle maskTexture = UniversalRenderer.CreateRenderGraphTexture(
        //            renderGraph,
        //            maskDesc,
        //            outputGlobalPropertyName,
        //            false,
        //            FilterMode.Bilinear,
        //            TextureWrapMode.Clamp
        //        );

        //        #region Drawing Settings (Material, Target Shaders, Sorting Criteria)
        //        DrawingSettings drawingSettings = RenderingUtils.CreateDrawingSettings(
        //            // "UniversalForward" will not work, as the material is created with ShaderGraph (which has NO lightmode tags for main pass.) ShadowCaster is one that is included, so I am doing that one.
        //            // Other potential ones?, "MotionVectors", "DepthNormalsOnly", "UniversalGBuffer", 
        //            new ShaderTagId(shaderTagID), //https://docs.unity3d.com/6000.3/Documentation/Manual/SL-PassTags.html    https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@10.1/manual/urp-shaders/urp-shaderlab-pass-tags.html
        //            renderingData,
        //            cameraData,
        //            lightData,
        //            SortingCriteria.BackToFront
        //        );
        //        drawingSettings.overrideMaterial = settings._stencilWriteMaterial;
        //        #endregion

        //        #region Create RendererListParams
        //        RendererListParams rendererListParams = new RendererListParams(
        //            renderingData.cullResults,
        //            drawingSettings,
        //            new FilteringSettings(settings._renderQueueRange, settings._layerMask)
        //        );
        //        #endregion

        //        #region State Blocks (Stencil Mask)

        //        // Setup Stencil State
        //        StencilState stencilState = StencilState.defaultValue;
        //        stencilState.enabled = true;
        //        stencilState.SetCompareFunction(CompareFunction.Equal);
        //        stencilState.SetPassOperation(StencilOp.Replace);
        //        stencilState.SetFailOperation(StencilOp.Keep);
        //        stencilState.SetZFailOperation(StencilOp.Zero);
        //        //stencilState.readMask = (byte)settings._stencilRef; /// Commented out as this line is probably pointless. RenderObjects doesnt use this.

        //        // Setup Stencil Block
        //        var renderStateBlock = new RenderStateBlock(RenderStateMask.Stencil);
        //        renderStateBlock.mask |= RenderStateMask.Stencil; /// This line may be pointless. Delete later if functional and can test if works without this.
        //        renderStateBlock.stencilState = stencilState;
        //        renderStateBlock.stencilReference = settings._stencilRef;

        //        // Assign stateBlocks to rendererListParams.
        //        /// rendererStateBlocks is declared in Scriptable Render Pass class.
        //        rendererStateBlocks[0] = renderStateBlock;
        //        rendererStateBlockIDs[0] = ShaderTagId.none;// new ShaderTagId(shaderTagID);

        //        rendererListParams.stateBlocks = rendererStateBlocks;
        //        rendererListParams.tagValues = rendererStateBlockIDs;
        //        rendererListParams.isPassTagName = false;

        //        // ---------------------------------------------------------------

        //        #region Render State Block Assign from Render Objects Pass Test
        //        //// Test trying to use unity macro instead of assigning myself. Difference isbetween shader tag and native array handling.
        //        //ShaderTagId[] s_ShaderTagValues = new ShaderTagId[1];
        //        //RenderStateBlock[] s_RenderStateBlocks = new RenderStateBlock[1];

        //        //s_ShaderTagValues[0] = ShaderTagId.none;
        //        //s_RenderStateBlocks[0] = renderStateBlock;
        //        //NativeArray<ShaderTagId> tagValues = new NativeArray<ShaderTagId>(s_ShaderTagValues, Allocator.Temp);
        //        //NativeArray<RenderStateBlock> stateBlocks = new NativeArray<RenderStateBlock>(s_RenderStateBlocks, Allocator.Temp);
        //        //rendererListParams.tagValues = tagValues;
        //        //rendererListParams.stateBlocks = stateBlocks;
        //        //rendererListParams.isPassTagName = false;
        //        #endregion

        //        // ---------------------------------------------------------------
        //        #endregion

        //        // Set up pass data.
        //        passData.source = resourceData.activeColorTexture;
        //        passData.mask = maskTexture;
        //        passData.rendererListHandle = renderGraph.CreateRendererList(rendererListParams);
        //        passData.overrideMaterial = settings._stencilWriteMaterial;
        //        #endregion

        //        // Declare texture usage 
        //        builder.UseTexture(resourceData.activeColorTexture, AccessFlags.Read);
        //        builder.UseRendererList(passData.rendererListHandle);
        //        builder.SetRenderAttachment(maskTexture, 0);

        //        // Set a texture to the global texture
        //        //builder.SetGlobalTextureAfterPass(maskTexture, globalTextureID); /// Allows to sample this shader later in the rendering process.

        //        builder.AllowPassCulling(false); /// Prevent RenderGraph from removing this RasterRenderPass

        //        // Execute the render pass.
        //        builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
        //    }
        //}
        #endregion

        #region Quick Tests

        private int globalTextureID;

        public StencilRTRendererFeatureWritePass(StencilRTRendererFeatureSettings settings)
        {
            this.settings = settings;
        }

        private class PassData
        {
            public TextureHandle maskTexture;
            public Material material;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            globalTextureID = Shader.PropertyToID(settings._outputGlobalPropertyName);

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            var descriptor = cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            descriptor.msaaSamples = 1;
            descriptor.graphicsFormat = GraphicsFormat.R8_UNorm;

            TextureHandle maskTexture = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph,
                descriptor,
                settings._outputGlobalPropertyName,
                false
            );

            using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData))
            {
                passData.maskTexture = maskTexture;
                passData.material = settings._overrideMaterial;

                builder.SetRenderAttachment(maskTexture, 0);
                builder.SetRenderAttachmentDepth(resourceData.cameraDepth, AccessFlags.Read);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    var cmd = context.cmd;

                    // Clear to black
                    cmd.ClearRenderTarget(false, true, Color.black);

                    // Draw fullscreen quad with stencil test
                    CoreUtils.DrawFullScreen(cmd, data.material);
                });

                builder.SetGlobalTextureAfterPass(maskTexture, globalTextureID);
            }
        }
        #endregion
    }
    #endregion

    // TO DO: Make assign pass not write to global texture when done. It doesn't need to, right?

    #region Draw Pass
    class StencilRTRendererFeatureDrawPass : ScriptableRenderPass
    {
        readonly StencilRTRendererFeatureSettings settings;
        private int globalTextureID;
        public NativeArray<RenderStateBlock> rendererStateBlocks = new NativeArray<RenderStateBlock>(1, Allocator.Temp);
        public NativeArray<ShaderTagId> rendererStateBlockIDs = new NativeArray<ShaderTagId>(1, Allocator.Temp);
        public StencilRTRendererFeatureDrawPass(StencilRTRendererFeatureSettings settings)
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
            globalTextureID = Shader.PropertyToID(settings._outputGlobalPropertyName);

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
                maskDesc.depthBufferBits = 24; /// Needs to be 24 or 32 to include stencil data.
                maskDesc.width = Mathf.RoundToInt(maskDesc.width * 1f); // float here affects resolution. 1f means 1x the screen resolution.
                maskDesc.height = Mathf.RoundToInt(maskDesc.height * 1f);

                // Create the occluders texture handle
                TextureHandle maskTexture = UniversalRenderer.CreateRenderGraphTexture(
                    renderGraph,
                    maskDesc,
                    settings._outputGlobalPropertyName,
                    false,
                    FilterMode.Bilinear,
                    TextureWrapMode.Clamp
                );

                #region Drawing Settings (Material, Target Shaders, Sorting Criteria)
                DrawingSettings drawingSettings = RenderingUtils.CreateDrawingSettings(
                    // Shadergraph doesn't specify a LightMode tag, meaning the default is used. Default is "SRPDefaultUnlit". This should be the default shaderTagID used in settings._shaderTagID.
                    new ShaderTagId(settings._shaderTagID),
                    renderingData,
                    cameraData,
                    lightData,
                    SortingCriteria.CommonTransparent
                );
                drawingSettings.overrideMaterial = settings._overrideMaterial;
                #endregion

                #region Create Renderer List Params
                RendererListParams rendererListParams = new RendererListParams(
                    renderingData.cullResults,
                    drawingSettings,
                    new FilteringSettings(settings._renderQueueRange, settings._layerMask)
                );
                #endregion

                #region Render State Blocks + Assign to RendererListParams (Stencil Mask, ZBuffer/Depth Testing)

                // Create blank Render State Block. 
                RenderStateBlock renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);

                // TO DO: Make booleans to control whether or not to include stencil state or depth state (just like how Render Objects does it.)

                #region Stencil State
                // Setup Stencil State
                StencilState stencilState = StencilState.defaultValue;
                stencilState.enabled = true;
                stencilState.SetCompareFunction(CompareFunction.Equal);
                stencilState.SetPassOperation(StencilOp.Replace);
                stencilState.SetFailOperation(StencilOp.Keep);
                stencilState.SetZFailOperation(StencilOp.Keep);

                // Setup Stencil Block
                renderStateBlock.mask |= RenderStateMask.Stencil; /// Includes Stencil settings to the renderStateBlock include flags.
                renderStateBlock.stencilState = stencilState;
                renderStateBlock.stencilReference = settings._stencilRef;
                #endregion

                #region Depth State
                // Setup Depth State
                DepthState depthState = DepthState.defaultValue;
                depthState.writeEnabled = false;
                depthState.compareFunction = CompareFunction.LessEqual;

                // Setup Depth Block
                renderStateBlock.mask |= RenderStateMask.Depth; /// Includes Depth settings to the renderStateBlock include flags.
                renderStateBlock.depthState = depthState;
                #endregion

                #region Assign State Blocks to Renderer List Params
                /// rendererStateBlocks is declared in Scriptable Render Pass class. Temp Allocation.
                rendererStateBlocks[0] = renderStateBlock;
                rendererStateBlockIDs[0] = ShaderTagId.none; // RenderObjectsPass uses ShaderTagId.none

                rendererListParams.stateBlocks = rendererStateBlocks;
                rendererListParams.tagValues = rendererStateBlockIDs;
                rendererListParams.isPassTagName = false; // RenderObjectsPass uses this set to false as well.
                #endregion

                // ---------------------------------------------------------------
                #endregion

                // Set up pass data.
                passData.source = resourceData.activeColorTexture;
                passData.mask = maskTexture;
                passData.rendererListHandle = renderGraph.CreateRendererList(rendererListParams);
                passData.overrideMaterial = settings._overrideMaterial;
                #endregion

                // Declare texture usage 
                builder.UseTexture(resourceData.activeColorTexture, AccessFlags.Read);
                builder.UseRendererList(passData.rendererListHandle);
                builder.SetRenderAttachmentDepth(maskTexture, 0);

                // Set a texture to the global texture
                builder.SetGlobalTextureAfterPass(maskTexture, globalTextureID); /// Allows to sample this shader later in the rendering process.

                builder.AllowPassCulling(false); /// Prevent RenderGraph from removing this RasterRenderPass

                // Execute the render pass.
                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
            }
        }
    }
    #endregion
}
