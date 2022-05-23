using System;
using UnityEngine;

namespace CharacterEditor
{
    namespace CharacterInventory
    {
        [Serializable]
        public class ItemData : ScriptableObject, IData
        {
            public string guid;
            public string Guid => guid;

            public string itemName;
            public PathData icon;

            // Prefabs for environment visible and etc
            public PathData prefab;
        }
    }
}