using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[AddComponentMenu("Retro3D/Volume Layer")]
public class Retro3DVolumeLayer : MonoBehaviour
{
    [SerializeField] private LayerMask m_volumeLayers;
    public LayerMask layers => m_volumeLayers;
}
