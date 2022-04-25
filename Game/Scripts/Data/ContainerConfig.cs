using System;
using System.Collections;
using System.Collections.Generic;
using CharacterEditor.CharacterInventory;
using UnityEngine;

namespace CharacterEditor
{
    [CreateAssetMenu]
    [Serializable]
    public class ContainerConfig : ScriptableObject, IData
    {
        public string guid;
        public ItemData[] initItems;
        public string prefabPath;
        public string bundlePrefabPath;

        public string Guid
        {
            get { return guid; }
        }
    }
}
