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
            public CharacterConfig characterConfig;
            public EquipTextureData[] textures;
            public EquipModelData[] models;
        }

        [Serializable]
        public class EquipTextureData
        {
            public TextureType textureType;
            public PathData texture;
        }

        [Serializable]
        public class EquipModelData
        {
            public MeshType[] availableMeshes;
            public PathData prefab;
            public PathData additionalPrefab;
            public PathData texture;
            public PathData additionalTexture;
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