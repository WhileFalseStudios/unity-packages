using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Retro3D_SurfaceEditor : ShaderGUI
{
    enum SurfaceMode
    {
        Opaque = 0,
        Masked = 1,
        Transparent = 2,
        Additive = 3
    }

    enum RenderFaces
    {
        Front = 1,
        Back = 2,
        Both = 0
    }

    private const string k_EditorPrefsPrefix = "WhileFalse:Retro3D:Material:UI_State:";

    protected class Styles
    {
        public static readonly GUIContent RenderStyleContent = new GUIContent("Render Style", "Render style to use");
        public static readonly GUIContent RenderFacesContent = new GUIContent("Culling Mode", "Culling mode to use");
        public static readonly GUIContent AlbdeoTexContent = new GUIContent("Albedo", "Albedo (color) texture + tint color");
        public static readonly GUIContent EmissiveTexContent = new GUIContent("Emission", "Emission texture + tint color");
        public static readonly GUIContent TexOffsetContent = new GUIContent("Offsets");
        public static readonly GUIContent ReflectionsContent = new GUIContent("Reflections", "Enable cubemap reflections");
        public static readonly GUIContent ReflectionSpecContent = new GUIContent("Reflection Specular Color");

        public static readonly GUIContent ViewmodelModeContent = new GUIContent("Viewmodel Projection", "Renders this material so that it will not clip through the world.");

        public static readonly GUIContent FlagsHeaderContent = new GUIContent("Render Properties", "Main style properties");
        public static readonly GUIContent PropsHeaderContent = new GUIContent("Material Properties");
        public static readonly GUIContent MainPropsHeaderContent = new GUIContent("Appearance");
        public static readonly GUIContent ReflPropsHeaderContent = new GUIContent("Reflections");
        public static readonly GUIContent OptionsHeaderContent = new GUIContent("Advanced");
        public static readonly GUIContent EmissiveNoticeContent = new GUIContent("GI properties disabled because no Emission texture is assigned.");
    }

    private MaterialProperty albedoTex;
    private MaterialProperty albedoColor;
    private MaterialProperty emissiveTex;
    private MaterialProperty emissiveColor;
    private MaterialProperty texOffset;
    private MaterialProperty enableReflections;
    private MaterialProperty reflTex;
    private MaterialProperty reflColor;
    private MaterialProperty cullMode;

    private bool GetToggleState(string key)
    {
        return EditorPrefs.GetBool($"{k_EditorPrefsPrefix}{key}");
    }

    private void SetToggleState(string key, bool state)
    {
        EditorPrefs.SetBool($"{k_EditorPrefsPrefix}{key}", state);
    }

    private void FindProperties(MaterialProperty[] props)
    {
        albedoTex = FindProperty("_MainTex", props);
        albedoColor = FindProperty("_Color", props);
        emissiveTex = FindProperty("_Emissive", props);
        emissiveColor = FindProperty("_EmissiveColor", props);
        texOffset = FindProperty("_MainTex_ST", props);
        enableReflections = FindProperty("_Reflections", props);
        reflTex = FindProperty("_ReflTex", props);
        reflColor = FindProperty("_ReflColor", props);
        cullMode = FindProperty("_CullMode", props);
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        FindProperties(properties);

        Material mat = materialEditor.target as Material;

        DrawPropertiesGUI(materialEditor, mat);
    }

    private void DrawPropertiesGUI(MaterialEditor editor, Material mat)
    {
        if (mat == null)
            return;

        EditorGUI.BeginChangeCheck();

        DrawFlags(editor, mat);

        EditorGUILayout.Separator();

        EditorGUILayout.LabelField(Styles.PropsHeaderContent, EditorStyles.boldLabel);
        DrawMaterialProps(editor, mat);
        DrawMaterialOptions(editor, mat);

        EditorGUI.EndChangeCheck();
    }

    private void DrawFlags(MaterialEditor editor, Material mat)
    {
        EditorGUILayout.LabelField(Styles.FlagsHeaderContent, EditorStyles.boldLabel);
        var v = (RenderFaces)EditorGUILayout.EnumPopup(Styles.RenderFacesContent, (RenderFaces)Mathf.Round(cullMode.floatValue));
        cullMode.floatValue = (int)v;
    }

    private void DrawMaterialProps(MaterialEditor editor, Material mat)
    {
        {
            var texStates = EditorGUILayout.BeginFoldoutHeaderGroup(GetToggleState("TexGroups"), Styles.MainPropsHeaderContent);
            if (texStates)
            {
                editor.TexturePropertyWithHDRColor(Styles.AlbdeoTexContent, albedoTex, albedoColor, true);
                editor.TexturePropertyWithHDRColor(Styles.EmissiveTexContent, emissiveTex, emissiveColor, false);
                editor.TextureScaleOffsetProperty(texOffset);

                if (emissiveTex.textureValue == null)
                {
                    mat.DisableKeyword("EMISSION_ON");
                }
                else
                {
                    mat.EnableKeyword("EMISSION_ON");
                }
            }
            SetToggleState("TexGroups", texStates);
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        {
            var reflState = EditorGUILayout.BeginFoldoutHeaderGroup(GetToggleState("Refl"), Styles.ReflPropsHeaderContent);
            if (reflState)
            {
                var s = mat.IsKeywordEnabled("REFLECTIONS_ON");
                s = EditorGUILayout.Toggle(Styles.ReflectionsContent, s);
                if (s)
                {
                    mat.EnableKeyword("REFLECTIONS_ON");
                    DrawReflectionProps(editor, mat);
                }
                else
                {
                    mat.DisableKeyword("REFLECTIONS_ON");
                }

            }
            SetToggleState("Refl", reflState);
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }

    private void DrawReflectionProps(MaterialEditor editor, Material mat)
    {
        editor.TexturePropertyWithHDRColor(Styles.ReflectionSpecContent, reflTex, reflColor, false);
    }

    private void DrawMaterialOptions(MaterialEditor editor, Material mat)
    {
        var advState = EditorGUILayout.BeginFoldoutHeaderGroup(GetToggleState("Advanced"), Styles.OptionsHeaderContent);
        if (advState)
        {
            {
                int vmMode = mat.GetInt("_ViewModel");
                var t = EditorGUILayout.Toggle(Styles.ViewmodelModeContent, vmMode > 0);
                mat.SetInt("_ViewModel", t ? 1 : 0);
            }

            editor.EnableInstancingField();

            if (emissiveTex.textureValue == null)
            {
                EditorGUILayout.HelpBox(Styles.EmissiveNoticeContent.text, MessageType.Warning);
            }
            EditorGUI.BeginDisabledGroup(emissiveTex.textureValue == null);
            {
                editor.LightmapEmissionProperty();
                editor.DoubleSidedGIField();
            }
            EditorGUI.EndDisabledGroup();
        }

        SetToggleState("Advanced", advState);
        EditorGUILayout.EndFoldoutHeaderGroup();
    }
}