using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace WhileFalse.Retro3D
{
    [PostProcessLinkRenderer(typeof(BloomRenderer))]
    [VolumeComponentMenu("Retro3D/Post Processing/Bloom")]
    public sealed class Bloom : VolumeComponent
    {
        [SerializeField] public ClampedFloatParameter intensity = new ClampedFloatParameter(1.0f, 0, 1);
        [SerializeField] public ColorParameter tintColor = new ColorParameter(Color.white);
    }
    
    public sealed class BloomRenderer : PostProcessRenderer<Bloom>
    {
        public override int priority => 0;

        private Material _bloomMat;

        public override void Setup()
        {
            //_bloomMat = new Material(Shader.Find("Hidden/Retro3D/PostProcessing/Bloom"));
        }

        public override void Render(CommandBuffer cmd, Camera camera)
        {
            cmd.name = "Bloom";
            //cmd.Blit(BuiltinRenderTextureType.CurrentActive, BuiltinRenderTextureType.CameraTarget, _bloomMat);
        }

        public override bool ShouldRender()
        {
            return _component != null && _component.active && _component.intensity.value > 0;
        }
    }
}
