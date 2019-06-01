// Retro3DPipeline
// A minimal example of a custom render pipeline with the Retro3D shader.
// https://github.com/keijiro/Retro3DPipeline

using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WhileFalse.Retro3D
{
    // Render pipeline runtime class
    public class Retro3DPipeline : RenderPipeline
    {
        Retro3DPipelineAsset _settings;
        Dictionary<Camera, VolumeStack> _volumeStacks;
        SortedDictionary<int, PostProcessRenderer> _renderersSorted;

        const int k_maxVisiblePixelLights = 4;
        static int k_visibleLightColorsId = Shader.PropertyToID("_VisibleLightColors");
        static int k_visibleLightDirectionsOrPositionsId = Shader.PropertyToID("_VisibleLightDirectionsOrPositions");
        static int k_visibleLightAttenuationsId = Shader.PropertyToID("_VisibleLightAttenuations");
        static int k_visibleLightSpotDirectionsId = Shader.PropertyToID("VisibleLightSpotDirections");
        Vector4[] visibleLightDirectionsOrPositions = new Vector4[k_maxVisiblePixelLights];
        Vector4[] visibleLightColors = new Vector4[k_maxVisiblePixelLights];
        Vector4[] visibleLightAttenuations = new Vector4[k_maxVisiblePixelLights];
        Vector4[] visibleLightSpotDirections = new Vector4[k_maxVisiblePixelLights];

        internal Retro3DPipeline(Retro3DPipelineAsset settings)
        {
            _volumeStacks = new Dictionary<Camera, VolumeStack>();
            _settings = settings;
            GraphicsSettings.lightsUseLinearIntensity = true;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            BeginFrameRendering(cameras);

            #region Rendering setup

            if (_renderersSorted == null)
            {
                GetRenderersForEffects();
            }

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
                case RenderConstraintAxis.None:                    
                    rtDesc.width = Screen.width;
                    rtDesc.height = Screen.height;
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

            #endregion

            foreach (var camera in cameras)
            {
                BeginCameraRendering(camera);

                var _cb = CommandBufferPool.Get();

                #region Per-camera setup

                // Set the camera up.
                context.SetupCameraProperties(camera);

                float fov = _settings.m_viewModelFOV;
                bool isSceneView = false;
#if UNITY_EDITOR
                if (camera.cameraType == CameraType.SceneView)
                {
                    fov = camera.fieldOfView;
                    isSceneView = true;
                }
#endif
                _cb.SetGlobalVectorArray(k_visibleLightColorsId, visibleLightColors);
                _cb.SetGlobalVectorArray(k_visibleLightDirectionsOrPositionsId, visibleLightDirectionsOrPositions);
                _cb.SetGlobalVectorArray(k_visibleLightAttenuationsId, visibleLightAttenuations);
                _cb.SetGlobalVectorArray(k_visibleLightSpotDirectionsId, visibleLightSpotDirections);

                #region Volume stack setup

                VolumeStack stack = null;

                if (_volumeStacks.ContainsKey(camera))
                {
                    stack = _volumeStacks[camera];
                }
                else
                {
                    stack = VolumeManager.instance.CreateStack();
                    _volumeStacks.Add(camera, stack);
                }

                var stackLayer = camera.GetComponent<Retro3DVolumeLayer>();
                VolumeManager.instance.Update(stack, camera.transform, stackLayer ? stackLayer.layers : _settings.m_defaultVolumeLayerMask);

                #endregion

                var vm_matrix = Matrix4x4.Perspective(fov, camera.aspect, camera.nearClipPlane, camera.farClipPlane);
                _cb.SetGlobalMatrix("_ViewmodelProjMatrix", GL.GetGPUProjectionMatrix(vm_matrix, true));

                // Setup commands: Initialize the temporary render texture.
                _cb.name = "Setup";
                _cb.GetTemporaryRT(rtID, rtDesc.width, rtDesc.height, rtDesc.depthBufferBits, FilterMode.Point, rtDesc.colorFormat, RenderTextureReadWrite.Default, (int)_settings.m_antialiasing);
                _cb.SetRenderTarget(rtID);
                CoreUtils.ClearRenderTarget(_cb, ClearFlag.All, camera.backgroundColor);
                context.ExecuteCommandBuffer(_cb);
                _cb.Clear();

                #endregion

                PerformSceneRender(context, camera);

                if (isSceneView)
                {
                    context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
                }

                PerformPostProcessing(context, camera, stack);

                if (isSceneView)
                {
                    context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
                }

                PerformFinalBlit(context, rtID, _cb);

                context.Submit();

                EndCameraRendering(context, camera);

                CommandBufferPool.Release(_cb);
            }

            EndFrameRendering(context, cameras);        
        }

        private void PerformFinalBlit(ScriptableRenderContext context, int rtID, CommandBuffer _cb)
        {
            // Blit the render result to the camera destination.
            _cb.name = "Blit";
            _cb.Blit(rtID, BuiltinRenderTextureType.CameraTarget);
            context.ExecuteCommandBuffer(_cb);
            _cb.Clear();
        }

        private void PerformPostProcessing(ScriptableRenderContext context, Camera camera, VolumeStack stack)
        {
            if (CoreUtils.ArePostProcessesEnabled(camera))
            {
                foreach (var renderer in _renderersSorted)
                {
                    var renderObject = renderer.Value;
                    renderObject.SetupStackComponent(stack);

                    if (renderObject.ShouldRender())
                    {
                        CommandBuffer buf = CommandBufferPool.Get();
                        renderObject.Render(buf, camera);
                        context.ExecuteCommandBuffer(buf);
                        CommandBufferPool.Release(buf);
                    }
                }
            }
        }

        private void PerformSceneRender(ScriptableRenderContext context, Camera camera)
        {            
            if (camera.TryGetCullingParameters(out var cullParms))
            {
                //Setup
#if UNITY_EDITOR
                if (camera.cameraType == CameraType.SceneView)
                {
                    ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
                }
#endif
                CullingResults cullResults = context.Cull(ref cullParms);
                var sorting = new SortingSettings(camera);
                sorting.criteria = SortingCriteria.CommonOpaque;
                var drawSetting = new DrawingSettings(new ShaderTagId("Base"), sorting);
                drawSetting.enableDynamicBatching = _settings.m_enableDynamicBatching;
                drawSetting.enableInstancing = true;
                drawSetting.perObjectData = PerObjectData.Lightmaps | PerObjectData.LightProbe | PerObjectData.ReflectionProbes | PerObjectData.LightIndices | PerObjectData.LightData;
                var filterSettings = new FilteringSettings(RenderQueueRange.opaque);

                ResolveLights(cullResults);

                // Draw opaque
                context.DrawRenderers(cullResults, ref drawSetting, ref filterSettings);

                // Draw skybox (saves overdraw)
                context.DrawSkybox(camera);

                // Draw transparency
                filterSettings.renderQueueRange = RenderQueueRange.transparent;
                var ds = drawSetting.sortingSettings;
                ds.criteria = SortingCriteria.CommonTransparent;
                drawSetting.sortingSettings = ds;
                context.DrawRenderers(cullResults, ref drawSetting, ref filterSettings);
            }
        }

        private void ResolveLights(CullingResults cullResults)
        {
            for (int i = 0; i < cullResults.visibleLights.Length; i++)
            {
                if (i == k_maxVisiblePixelLights)
                    break;

                VisibleLight light = cullResults.visibleLights[i];
                visibleLightColors[i] = light.finalColor;

                Vector4 attenuation = Vector4.zero;
                attenuation.w = 1;

                if (light.lightType == LightType.Directional)
                {
                    Vector4 v = light.localToWorldMatrix.GetColumn(2);
                    v.Scale(new Vector4(-1, -1, -1, 1));
                    visibleLightDirectionsOrPositions[i] = v;
                }
                else
                {
                    visibleLightDirectionsOrPositions[i] = light.localToWorldMatrix.GetColumn(3);
                    attenuation.x = 1f / Mathf.Max(light.range * light.range, 0.00001f);

                    if (light.lightType == LightType.Spot)
                    {
                        float outerRad = Mathf.Deg2Rad * 0.5f * light.spotAngle;
                        float outerCos = Mathf.Cos(outerRad);
                        float outerTan = Mathf.Tan(outerRad);
                        float innerCos = Mathf.Cos(Mathf.Atan((46f / 64f) * outerTan));
                        float angleRange = Mathf.Max(innerCos - outerCos, 0.001f);
                        attenuation.z = 1f / angleRange;
                        attenuation.w = -outerCos * attenuation.z;

                        Vector4 v = light.localToWorldMatrix.GetColumn(2);
                        v.Scale(new Vector4(-1, -1, -1, 1));
                        visibleLightSpotDirections[i] = v;
                    }
                }

                visibleLightAttenuations[i] = attenuation;
            }
        }

        private void GetRenderersForEffects()
        {
            if (_renderersSorted == null)
            {
                _renderersSorted = new SortedDictionary<int, PostProcessRenderer>();
            }
            else
            {
                _renderersSorted.Clear();
            }

            var types = from a in AppDomain.CurrentDomain.GetAssemblies()
                        from t in a.GetTypes()
                        let attributes = t.GetCustomAttributes(typeof(PostProcessLinkRendererAttribute), false)
                        where attributes != null && attributes.Length > 0
                        where !t.IsAssignableFrom(typeof(IInternalVolumeEffect))
                        select new { Type = t, Attribute = attributes.Cast<PostProcessLinkRendererAttribute>().First() };            

            foreach (var tDef in types)
            {
                var rendererInstance = Activator.CreateInstance(tDef.Attribute.rendererType) as PostProcessRenderer;
                if (rendererInstance != null)
                {
                    _renderersSorted.Add(rendererInstance.priority, rendererInstance);
#if UNITY_EDITOR
                    Debug.Log($"Found renderer {rendererInstance.GetType().FullName} for {tDef.Type.FullName}");
#endif
                }
                else
                {
                    Debug.LogError($"Renderer type linked in {tDef.Type.FullName} does not inherit from {typeof(PostProcessRenderer).FullName}");
                }
            }
        }
    }
}
