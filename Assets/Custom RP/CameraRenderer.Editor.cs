using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

partial class CameraRenderer {

    partial void DrawUnsupportedShaders(); // 避免非Editor平台报错,也可以用这种方法在release builds地方消失，在Development build出现。    
    // 使用  UNITY_EDITOR || DEVELOPMENT_BUILD
    partial void DrawGizmos();
    partial void PrepareForSceneWindow();
#if UNITY_EDITOR
    static ShaderTagId[] legacyShaderTagIds = {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM") // 老版本 lightmap pass
    };

    static Material errorMaterial;

    partial void DrawGizmos()
    {
        if (Handles.ShouldRenderGizmos())
        {
            context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
            context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
        }
    }

    /// <summary>
    /// 渲染报错物体，设置默认设置
    /// </summary>
    partial void DrawUnsupportedShaders()
    {
        if (errorMaterial == null)
        {
            errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
        }
        // 渲染多个pass设置,如何遇到老版本shader，替换成errorShader
        var drawingSettings = new DrawingSettings(legacyShaderTagIds[0], new SortingSettings(camera)){ overrideMaterial = errorMaterial };
        // 这里不替换
//        var drawingSettings = new DrawingSettings(legacyShaderTagIds[0], new SortingSettings(camera));
        
        for (int i = 1; i < legacyShaderTagIds.Length; i++)
        {
            drawingSettings.SetShaderPassName(i, legacyShaderTagIds[i]);
        }
        var filteringSettings = FilteringSettings.defaultValue;
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
    }

    partial void PrepareForSceneWindow()
    {
        if (camera.cameraType == CameraType.SceneView)
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(camera); // 渲染UI
        }
    }

#endif
}