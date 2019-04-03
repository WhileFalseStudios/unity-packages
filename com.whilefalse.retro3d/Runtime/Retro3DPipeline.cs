// Retro3DPipeline
// A minimal example of a custom render pipeline with the Retro3D shader.
// https://github.com/keijiro/Retro3DPipeline

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Retro3D
{
    // Render pipeline runtime class
    public class Retro3DPipeline : RenderPipeline
    {
        // Temporary command buffer
        // Reused between frames to avoid GC allocation.
        // Rule: Clear commands right after calling ExecuteCommandBuffer.
        CommandBuffer _cb;
        Retro3DPipelineAsset _settings;

        internal Retro3DPipeline(Retro3DPipelineAsset settings)
        {
            _settings = settings;
        }

        public override void Dispose()
        {
            base.Dispose();

            if (_cb != null)
            {
                _cb.Dispose();
                _cb = null;
            }
        }

        public override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            base.Render(context, cameras);

            BeginFrameRendering(cameras);

            // Lazy initialization of the temporary command buffer.
            if (_cb == null) _cb = new CommandBuffer();

            // Constants used in the camera render loop.
            RenderTextureDescriptor rtDesc = new RenderTextureDescriptor(_settings.m_renderResolution.x, _settings.m_renderResolution.y, RenderTextureFormat.Default, 24);

            switch (_settings.m_fixedRenderResolution)
            {
                case RenderConstraintAxis.Horizontal:
                    rtDesc.height = (int)(((float)Screen.height / (float)Screen.width) * (float)rtDesc.width);
                    break;
                case RenderConstraintAxis.Vertical:
                    rtDesc.width = (int)(((float)Screen.width / (float)Screen.height) * (float)rtDesc.height);
                    break;
            }

            var rtID = Shader.PropertyToID("_LowResScreen");

            if (_settings.m_simulateVertexPrecision)
            {
                Shader.EnableKeyword("VERTEX_PRECISION_ON");
                Shader.SetGlobalFloat("_VertexPrecision", _settings.m_vertexPrecision);
            }
            else
            {
                Shader.DisableKeyword("VERTEX_PRECISION_ON");
            }

            if (_settings.m_perspectiveCorrection)
            {
                Shader.EnableKeyword("PERSPECTIVE_CORRECTION_ON");
            }
            else
            {
                Shader.DisableKeyword("PERSPECTIVE_CORRECTION_ON");
            }

            foreach (var camera in cameras)
            {
                BeginCameraRendering(camera);

                // Set the camera up.
                context.SetupCameraProperties(camera);

                float fov = _settings.m_viewModelFOV;
                bool isSceneView = false;
#if UNITY_EDITOR
                if (SceneView.lastActiveSceneView.camera == camera)
                {
                    fov = camera.fieldOfView;
                    isSceneView = true;
                }
#endif

                var vm_matrix = Matrix4x4.Perspective(fov, camera.aspect, camera.nearClipPlane, camera.farClipPlane);
                Shader.SetGlobalMatrix("_ViewmodelProjMatrix", GL.GetGPUProjectionMatrix(vm_matrix, _settings.m_fixedRenderResolution != RenderConstraintAxis.None || isSceneView));
                
                // Setup commands: Initialize the temporary render texture.
                if (_settings.m_fixedRenderResolution != RenderConstraintAxis.None && !isSceneView)
                {
                    _cb.name = "Setup";
                    _cb.GetTemporaryRT(rtID, rtDesc);
                    _cb.SetRenderTarget(rtID);
                    _cb.ClearRenderTarget(true, true, Color.black);
                    context.ExecuteCommandBuffer(_cb);
                    _cb.Clear();
                }
                else
                {
                    _cb.ClearRenderTarget(true, true, Color.black);
                    context.ExecuteCommandBuffer(_cb);
                    _cb.Clear();
                }

                context.DrawSkybox(camera);

                // Do basic culling.
                var culled = new CullResults();
                CullResults.Cull(camera, context, out culled);                

                // Render visible objects that has "Base" light mode tag.
                var settings = new DrawRendererSettings(camera, new ShaderPassName("Base"));
                settings.rendererConfiguration = RendererConfiguration.PerObjectLightmaps | RendererConfiguration.PerObjectLightProbe | RendererConfiguration.PerObjectReflectionProbes;
                var filter = new FilterRenderersSettings(true);
                filter.renderQueueRange = RenderQueueRange.all;

                context.DrawRenderers(culled.visibleRenderers, ref settings, filter); //Draw normal scene

                // Blit the render result to the camera destination.
                if (_settings.m_fixedRenderResolution != RenderConstraintAxis.None)
                {
                    _cb.name = "Blit";
                    _cb.Blit(rtID, BuiltinRenderTextureType.CameraTarget);
                    context.ExecuteCommandBuffer(_cb);
                }
                _cb.Clear();

                context.Submit();
            }            
        }
    }
}
