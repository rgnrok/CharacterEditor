using System;
using CharacterEditor;
using UnityEngine;

[Serializable]
public class PrefabBoneData : ScriptableObject
{
    [Serializable]
    public class BoneData
    {
        public string bone;
        public PathData prefabPath;
    }

    public BoneData[] armorBones;
    public BoneData[] faceBones;
}
