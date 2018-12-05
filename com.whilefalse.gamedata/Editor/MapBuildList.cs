using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace WhileFalse.GameData
{
    [CreateAssetMenu(menuName = "Game Data/Map Listing")]
    public class MapBuildList : ScriptableObject
    {
        [SerializeField] public List<MapSceneListing> mapBuildList = new List<MapSceneListing>();
    }
}
