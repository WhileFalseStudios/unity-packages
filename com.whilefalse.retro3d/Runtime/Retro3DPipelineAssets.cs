using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WhileFalse.Retro3D
{
#if WHILEFALSE
[CreateAssetMenu(menuName = "Render Pipeline/Retro3D/Pipeline Assets")]
#endif
    public class Retro3DPipelineAssets : ScriptableObject
    {
        [SerializeField] public Shader m_defaultShader;
        [SerializeField] public Material m_defaultMaterial;
    }

}