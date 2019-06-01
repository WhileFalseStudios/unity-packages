using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace WhileFalse.Retro3D
{
    [VolumeComponentMenu("Retro3D/Fog")]
    public sealed class Fog : VolumeComponent, IInternalVolumeEffect
    {
        [SerializeField] public FloatParameter density = new FloatParameter(0.1f);
        [SerializeField] public ColorParameter color = new ColorParameter(Color.gray);
    }
}