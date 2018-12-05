using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace WhileFalse.GameData
{
    public class MapBuildPipelineWindow : EditorWindow
    {
        [MenuItem("Tools/Game Data Management/Map Build Pipeline")]
        static void CreateWindow()
        {
            MapBuildPipelineWindow wnd = GetWindow<MapBuildPipelineWindow>(false, "Map Build Pipeline");
            wnd.minSize = new Vector2(400, 600);
        }

        private MapBuildList mapList;
        private int selectedTab;
        private List<string> tabs = new List<string>
    {
        "Editor Utilities",
        "Map List",
        "Build"
    };

        ReorderableList mapBuildListField;

        BuildTarget selectedBuildTarget;

        private void OnEnable()
        {

        }

        private void OnGUI()
        {
            selectedTab = GUILayout.Toolbar(selectedTab, tabs.ToArray(), GUILayout.Height(30));
            EditorGUILayout.Separator();
            switch (selectedTab)
            {
                case 0:
                    DrawEditorUtilsUI();
                    break;
                case 1:
                    DrawMapListUI();
                    break;
                case 2:
                    DrawBuildUI();
                    break;
            }
        }

        private void DrawEditorUtilsUI()
        {
            EditorGUILayout.HelpBox("A set of utilities to make working with the map system easier.", MessageType.Info);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Scene Settings", EditorStyles.boldLabel);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            if (GUILayout.Button("Normalise Scene Lighting Settings"))
            {

            }
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Changes the lighting settings of all scenes to match the settings of the active scene (should be the persistent scene)", MessageType.Info);
            GUILayout.EndVertical();
            EditorGUILayout.Separator();
        }

        private void DrawMapListUI()
        {
            mapList = (MapBuildList)EditorGUILayout.ObjectField("Map List", mapList, typeof(MapBuildList), false);

            if (mapBuildListField == null && mapList != null)
            {
                CreateBuildList();
            }

            if (mapList != null)
            {
                mapBuildListField.DoLayoutList();
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Scene here do not have to be added to the build list as they are compiled as asset bundles.", MessageType.Info);

                //Check if they have the correct asset bundle (we can't assign them manually it seems...)
            }
            else
            {
                EditorGUILayout.HelpBox("You must specify a map list to build.", MessageType.Error);
            }
        }

        private void CreateBuildList()
        {
            mapBuildListField = new ReorderableList(mapList.mapBuildList, typeof(MapSceneListing), true, false, true, true);
            mapBuildListField.elementHeightCallback += (int index) =>
            {
                int itemCount = (3 + mapList.mapBuildList[index].mapScenes.Count);
                return EditorGUIUtility.singleLineHeight * itemCount + 2 * itemCount;
            };
            mapBuildListField.drawElementCallback += (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = mapList.mapBuildList[index];
                rect.y += 2;

                EditorGUI.LabelField(rect, $"{element.mapName} ({element.mapScenes.Count + 1} scenes)", EditorStyles.boldLabel);
                rect.y += EditorGUIUtility.singleLineHeight + 2;

                Rect r = rect;
                r.height = EditorGUIUtility.singleLineHeight;
                element = (MapSceneListing)EditorGUI.ObjectField(r, "Map List", element, typeof(MapSceneListing), false);
                mapList.mapBuildList[index] = element;

                rect.y += EditorGUIUtility.singleLineHeight + 2;

                EditorGUI.LabelField(rect, $"Persistent Scene: {AssetDatabase.GetAssetPath(element.persistentScene)}");
                rect.y += EditorGUIUtility.singleLineHeight + 2;
                rect.x += 20;
                foreach (var m in element.mapScenes)
                {
                    EditorGUI.LabelField(rect, AssetDatabase.GetAssetPath(m));
                    rect.y += EditorGUIUtility.singleLineHeight + 2;
                }
            };
        }

        private void DrawBuildUI()
        {
            selectedBuildTarget = (BuildTarget)EditorGUILayout.EnumFlagsField("Build Platform", selectedBuildTarget);
            if (BuildPipeline.IsBuildTargetSupported(BuildPipeline.GetBuildTargetGroup(selectedBuildTarget), selectedBuildTarget))
            {
                EditorGUILayout.HelpBox("Build target installed!", MessageType.Info);
                if (GUILayout.Button("Build Maps"))
                {
                    var path = System.IO.Path.Combine(Application.dataPath, "..", "AssetBundles", selectedBuildTarget.ToString());
                    BuildMapBundles(path);
                }
                EditorGUILayout.Space();
                if (GUILayout.Button("Build Player"))
                {

                }
            }
            else
            {
                EditorGUILayout.HelpBox("The selected build target is not installed.", MessageType.Error);
            }
        }

        private void BuildMapBundles(string outputPath)
        {
            foreach (var map in mapList.mapBuildList)
            {

            }
            var manifest = BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.UncompressedAssetBundle | BuildAssetBundleOptions.DeterministicAssetBundle, selectedBuildTarget);
        }

        private void BuildPlayer(string outputPath)
        {

        }
    }
}