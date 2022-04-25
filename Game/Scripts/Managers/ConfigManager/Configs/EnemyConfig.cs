using System;
using UnityEngine;

namespace CharacterEditor
{
    [Serializable]
    public class EnemyConfig : EntityConfig, IData
    {
        public CharacterConfig entityConfig; //change to entity config and move enemyprefab
        public PrefabBoneData prefabBoneData;

        public string texturePath;
        public string faceMeshTexturePath;
        public string armorTexturePath;
        public string materialPath;

        public string textureBundlePath;
        public string faceMeshTextureBundlePath;
        public string armorTextureBundlePath;
        public string materialBundlePath;
        public string portraitIconPath;
        public string portraitIconName;

        public string Guid { get { return guid; }}

        public EnemyConfig()
        {
        }
    }
}