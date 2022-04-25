using System;
using UnityEngine;

[Serializable]
public class PrefabBoneData : ScriptableObject
{
    [Serializable]
    public class BoneData
    {
        public string bone;
        public string prefabPath;
        public string prefabBundlePath;
    }

    public BoneData[] armorBones;
    public BoneData[] faceBones;
}
