using System;
using UnityEngine;

namespace CharacterEditor
{
    [Serializable]
    public class CharacterConfig : EntityConfig
    {
        public string folderName = "";

        public PathData previewPrefabPath;
        public PathData createGamePrefabPath;

        public GameObject PreviewPrefab { get; set; }
        public GameObject CreateGamePrefab { get; set; }
        
        public string headBone;

        public string[] shortRobeMeshes;
        public string[] longRobeMeshes;
        public string[] cloakMeshes;

        public TextureType[] availableTextures;
        public MeshTypeBone[] availableMeshes;
    }
}