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
            public string Guid { get { return guid; } }

            public string itemName;
            public string iconPath;
            public string iconBundleName;
            public string prefabPath;
            public string prefabBundlePath;
        }
    }
}