using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 每个相机单独渲染，为了方便扩展使用，比如人物相机，地图相机，forward rendering, deferred rendering;
/// </summary>
public partial class CameraRenderer
{
    const string bufferName = "Render Camera";
    static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    // 天空盒可独立的buffer，其他的Geometry需要使用commandbuffer定义
    CommandBuffer buffer = new CommandBuffer{ name = bufferName };
    ScriptableRenderContext context;

    Camera camera;
    CullingResults cullingResults;
    /// <summary>
    /// 渲染所有相机可见部分
    /// </summary>
    /// <param name="context"></param>
    /// <param name="camera"></param>
    public void Render(ScriptableRenderContext context, Camera camera)
    {
        this.context = context;
        this.camera = camera;
        PrepareForSceneWindow(); // 渲染UI
        // 剔除相机外的物体
        if (!Cull())
        {
            return;
        }

        Setup();
        DrawVisibleGeometry(); // 渲染可物体
        DrawUnsupportedShaders(); // 渲染报错物体
        DrawGizmos();
        Submit();
    }
    /// <summary>
    /// 渲染可被相机看见的物体,顺序 opaque->skybox->transparent
    /// </summary>
    void DrawVisibleGeometry()
    {
        // 不透明物体
        var sortingSettings = new SortingSettings(camera) // 渲染顺序 front to back
        {
            criteria = SortingCriteria.CommonOpaque 
        };        
        var drawingSettings = new DrawingSettings(
            unlitShaderTagId, sortingSettings
        );
        var filteringSettings = new FilteringSettings(RenderQueueRange.all);
        context.DrawRenderers(
            cullingResults, ref drawingSettings, ref filteringSettings
        );
        // skybox
        context.DrawSkybox(camera);//天空盒比较特殊，可以直接使用，渲染其他几何物体要用CommandBuffer

        // 透明物体
        sortingSettings.criteria = SortingCriteria.CommonTransparent; //渲染顺序 back to front
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        context.DrawRenderers(
            cullingResults, ref drawingSettings, ref filteringSettings
        );
    }
    /// <summary>
    /// 设置好Camera相关矩阵,unity_MatrixVP 等
    /// </summary>
    void Setup()
    {
        // inject profiler samples
        buffer.ClearRenderTarget(true, true, Color.clear);//每次进行渲染都需要清除一次，增加一次 DrawGL
        buffer.BeginSample(bufferName);
        ExecuteBuffer();
        context.SetupCameraProperties(camera);
    }
    /// <summary>
    /// 最后一步，将 buffer 提交给 context
    /// </summary>
    void Submit()
    {
        buffer.EndSample(bufferName);
        ExecuteBuffer();
        context.Submit();
    }

    /// <summary>
    /// 执行buffer
    /// </summary>
    void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    bool Cull()
    {
        if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
        {
            cullingResults = context.Cull(ref p);
            return true;
        }
        return false;
    }


}