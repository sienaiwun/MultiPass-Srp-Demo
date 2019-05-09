using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

[ExecuteInEditMode]
public class MultiPassAssetPipe : RenderPipelineAsset
{
#if UNITY_EDITOR
    [UnityEditor.MenuItem("MultiPass-Demo/03 - Create Multi Pass Asset Pipeline")]
    static void CreateBasicAssetPipeline()
    {
        var instance = ScriptableObject.CreateInstance<MultiPassAssetPipe>();
        UnityEditor.AssetDatabase.CreateAsset(instance, "Assets/MultiPass-Demo/3-MultiPassAssetPipe/MultiPassPipe.asset");
    }
#endif

    protected override IRenderPipeline InternalCreatePipeline()
    {
        return new MultiPassAssetPipeInstance();
    }
}

public class MultiPassAssetPipeInstance : RenderPipeline
{
    public override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        base.Render(context, cameras);

        foreach (var camera in cameras)
        {
            // Culling
            ScriptableCullingParameters cullingParams;
            if (!CullResults.GetCullingParameters(camera, out cullingParams))
                continue;

            CullResults cull = CullResults.Cull(ref cullingParams, context);

            // Setup camera for rendering (sets render target, view/projection matrices and other
            // per-camera built-in shader variables).
            context.SetupCameraProperties(camera);

            // clear depth buffer
            var cmd = new CommandBuffer();
            cmd.ClearRenderTarget(true, false, Color.black);
            context.ExecuteCommandBuffer(cmd);
            cmd.Release();

            // Draw opaque objects using BasicPass shader pass
            var settings = new DrawRendererSettings(camera, new ShaderPassName("BasicPass"));
            // Second Pass's tage is AddtionalPass
            settings.SetShaderPassName(1, new ShaderPassName("AddtionalPass"));
            settings.sorting.flags = SortFlags.CommonOpaque;


            var filterSettings = new FilterRenderersSettings(true) { renderQueueRange = RenderQueueRange.opaque };
            context.DrawRenderers(cull.visibleRenderers, ref settings, filterSettings);

            // Draw skybox
            context.DrawSkybox(camera);

            context.Submit();
        }
    }
}
