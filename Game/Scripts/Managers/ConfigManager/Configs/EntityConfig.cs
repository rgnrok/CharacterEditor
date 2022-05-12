using System;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterEditor
{
    [Serializable]
    public class EntityConfig : ScriptableObject
    {
        public string guid;
        public PathData prefabPath;

        public GameObject Prefab { get; set; }

        public string[] skinnedMeshes;
    }
}