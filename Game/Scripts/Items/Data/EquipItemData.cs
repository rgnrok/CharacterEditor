using System;
using StatSystem;

namespace CharacterEditor
{
    namespace CharacterInventory
    {
        #region SubClasses
        [Serializable]
        public class ConfigEquipItemData
        {
            public string configGuid;
            public string configPath;
            public EquipTextureData[] textures;
            public EquipModelData[] models;
        }

        [Serializable]
        public class EquipTextureData
        {
            public TextureType textureType;
            public string texturePath;
            public string textureBundlePath;
        }

        [Serializable]
        public class EquipModelData
        {
            public MeshType[] availableMeshes;
            public string prefabPath;
            public string prefabBundlePath;
            public string additionalPrefabPath;
            public string additionalPrefabBundlePath;
            public string texturePath;
            public string textureBundlePath;
            public string additionalTexturePath;
            public string additionalTextureBundlePath;
        }
        #endregion

        [Serializable]
        public class StatData
        {
            public StatType type;
            public float value;
            public ModifierType modType;
            public float modTime;
        }

        [Serializable]
        public class EquipItemData : ItemData
        {
            public ConfigEquipItemData[] configsItems;
            public MeshType[] hidedMeshTypes;
            public EquipItemType itemType;
            public EquipItemSubType itemSubType;

            #region Stats

            public bool randomStats;
            public int randomStatsCount;
            public StatData[] stats;


            #endregion

        }
    }
}