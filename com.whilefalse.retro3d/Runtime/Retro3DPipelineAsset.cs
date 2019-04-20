// Retro3DPipeline
// A minimal example of a custom render pipeline with the Retro3D shader.
// https://github.com/keijiro/Retro3DPipeline

#if UNITY_2019_1_OR_NEWER
using UnityEngine.Rendering;
#else
using UnityEngine.Experimental.Rendering;
#endif
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
        [SerializeField] public bool m_enableDynamicBatching;

        [Header("Viewmodel")]
        [SerializeField] public float m_viewModelFOV = 40.0f;

        #endregion

#if UNITY_2019_1_OR_NEWER
        public override Shader defaultShader => GetDefaultShader();

        private Shader GetDefaultShader()
#else
        public override Shader GetDefaultShader()
#endif
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

#if UNITY_2019_1_OR_NEWER
        public override Material defaultMaterial => GetDefaultMaterial();

        private Material GetDefaultMaterial()
#else
        public override Material GetDefaultMaterial()
#endif
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

#if UNITY_2019_1_OR_NEWER
        protected override RenderPipeline CreatePipeline()
        {
            return new Retro3DPipeline(this);
        }
#else
        protected override IRenderPipeline InternalCreatePipeline()
        {
            return new Retro3DPipeline(this);
        }
#endif
    }
}
