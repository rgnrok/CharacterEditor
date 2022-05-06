using System;
using UnityEngine;

namespace CharacterEditor
{

    [Serializable]
    public class CharacterConfig : EntityConfig
    {
        public string previewPrefabPath;
        public string previewBundlePrefabPath;

        public string createGamePrefabPath;
        public string createGameBundlePrefabPath;

        public string enemyPrefabPath;
        public string enemyBundlePrefabPath;

        public GameObject PreviewPrefab { get; set; }
        public GameObject CreateGamePrefab { get; set; }
        public GameObject EnemyPrefab { get; set; }


        public string headBone;

        public string[] shortRobeMeshes;
        public string[] longRobeMeshes;
        public string[] cloakMeshes;

        public TextureType[] availableTextures;
        public MeshTypeBone[] availableMeshes;
    }
}