// Retro3DPipeline
// A minimal example of a custom render pipeline with the Retro3D shader.
// https://github.com/keijiro/Retro3DPipeline

using UnityEngine.Experimental.Rendering;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
#endif

namespace Retro3D
{
    public enum RenderConstraintAxis
    {
        None,
        Vertical,
        Horizontal,
        Both
    }

    // Render pipeline asset for Retro3D
    // Nothing special here. Just a boilerplate thing.
    class Retro3DPipelineAsset : RenderPipelineAsset
    {
#if UNITY_EDITOR
        [MenuItem("Assets/Create/Render Pipeline/Retro3D/Pipeline Asset")]
        static void CreateRetro3DPipeline()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0, CreateInstance<CreateRetro3DPipelineAsset>(),
                "Retro3D Pipeline.asset", null, null
            );
        }

        class CreateRetro3DPipelineAsset : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                var instance = CreateInstance<Retro3DPipelineAsset>();
                AssetDatabase.CreateAsset(instance, pathName);
            }
        }
#endif

        static Retro3DPipelineAssets k_PipelineAssets;

        #region Pipeline Settings

        [Header("Internal Resolution")]
        [SerializeField] public RenderConstraintAxis m_fixedRenderResolution;
        [SerializeField] public Vector2Int m_renderResolution = new Vector2Int(320, 240);

        [Header("Shader Features")]
        [SerializeField] public bool m_perspectiveCorrection;
        [SerializeField] public bool m_simulateVertexPrecision;
        [SerializeField] public float m_vertexPrecision;

        #endregion

        public override Shader GetDefaultShader()
        {
            if (k_PipelineAssets == null)
            {
                k_PipelineAssets = Resources.Load<Retro3DPipelineAssets>("Default Retro3D Assets");
            }

            if (k_PipelineAssets != null)
            {
                return k_PipelineAssets.m_defaultShader;
            }
            else
            {
                return Shader.Find("Retro3D/Surface");
            }
        }

        public override Material GetDefaultMaterial()
        {
            if (k_PipelineAssets == null)
            {
                k_PipelineAssets = Resources.Load<Retro3DPipelineAssets>("Default Retro3D Assets");
            }

            if (k_PipelineAssets != null)
            {
                return k_PipelineAssets.m_defaultMaterial;
            }
            else
            {
                return new Material(GetDefaultShader());
            }
        }

        protected override IRenderPipeline InternalCreatePipeline()
        {
            return new Retro3DPipeline(this);
        }
    }
}