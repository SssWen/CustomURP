using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 每个相机单独渲染，为了方便扩展使用，比如人物相机，地图相机，forward rendering, deferred rendering;
/// </summary>
public class CameraRenderer
{

    ScriptableRenderContext context;

    Camera camera;

    /// <summary>
    /// 渲染所有相机可见部分
    /// </summary>
    /// <param name="context"></param>
    /// <param name="camera"></param>
    public void Render(ScriptableRenderContext context, Camera camera)
    {
        this.context = context;
        this.camera = camera;

        DrawVisibleGeometry();
        Submit();
    }
    void DrawVisibleGeometry()
    {
        context.DrawSkybox(camera);
    }
    /// <summary>
    /// 将 buffere 提交给 context
    /// </summary>
    void Submit()
    {
        context.Submit();
    }
}