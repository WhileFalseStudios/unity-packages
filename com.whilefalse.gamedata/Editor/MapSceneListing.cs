using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace WhileFalse.GameData
{
    [CreateAssetMenu(menuName = "Game Data/Map Scene List")]
    public class MapSceneListing : ScriptableObject
    {
        [SerializeField] public SceneAsset persistentScene;
        [SerializeField] public List<SceneAsset> mapScenes = new List<SceneAsset>();
        [SerializeField] public string mapName;
    }

}