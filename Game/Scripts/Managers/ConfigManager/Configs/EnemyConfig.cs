using System;
using UnityEngine;

namespace CharacterEditor
{
    [Serializable]
    public class EnemyConfig : EntityConfig, IData
    {
        public PrefabBoneData prefabBoneData;

        public PathData texturePath;
        public PathData faceMeshTexturePath;
        public PathData armorTexturePath;
        public PathData materialPath;

        public string portraitIconPath;
        public string portraitIconName;

        public string Guid { get { return guid; }}

        public EnemyConfig()
        {
        }
    }
}