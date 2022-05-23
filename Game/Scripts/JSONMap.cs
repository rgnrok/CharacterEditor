using System;
using System.Collections.Generic;

namespace CharacterEditor
{
    namespace JSONMap
    {
        [Serializable]
        public class DataMap
        {
            public List<RaceMap> races = new List<RaceMap>();
            public List<GuidPathMap> items = new List<GuidPathMap>();
            public List<GuidPathMap> playableNpc = new List<GuidPathMap>();
            public List<GuidPathMap> enemies = new List<GuidPathMap>();
            public List<GuidPathMap> containers = new List<GuidPathMap>();
        }

        [Serializable]
        public class GuidPathMap
        {
            public string path;
            public string guid;
        }
        
        [Serializable]
        public class RaceMap
        {
            public List<TexturesMap> textures = new List<TexturesMap>();
            public List<MeshesMap> meshes = new List<MeshesMap>();
            public string race;
            public string configGuid;
            public string configPath;
            public string prefabPath;
            public string previewPrefabPath;
            public string createGamePrefabPath;
        }

        #region Mesh

        [Serializable]
        public class MeshesMap
        {
            public List<MapMesh> meshPaths = new List<MapMesh>();
            public MeshType type;
        }

      
        [Serializable]
        public class MapMesh
        {
            public List<MapTexture> textures = new List<MapTexture>();
            public string modelPath;
        }

        #endregion

        #region Texture

        [Serializable]
        public class TexturesMap
        {
            public List<MapTexture> texturePaths = new List<MapTexture>();
            public TextureType type;
        }

        [Serializable]
        public class MapTexture
        {
            public string[] colorPaths;
        }
      
        #endregion
    }
}