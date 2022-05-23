using System;
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
        public PathData prefab;

        public string Guid
        {
            get { return guid; }
        }
    }
}
