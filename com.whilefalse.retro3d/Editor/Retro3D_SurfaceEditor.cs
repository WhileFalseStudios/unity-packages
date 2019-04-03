using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Retro3D_SurfaceEditor : ShaderGUI
{
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        base.OnGUI(materialEditor, properties);
        //foreach (var prop in properties)
        //{
        //    materialEditor.ShaderProperty(prop, prop.displayName);
        //}
    }
}