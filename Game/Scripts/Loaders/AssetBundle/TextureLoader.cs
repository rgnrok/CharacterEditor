using Game;
using UnityEngine;

namespace CharacterEditor
{
    namespace AssetBundleLoader
    {
        public class TextureLoader : CommonLoader<Texture2D>, ITextureLoader
        {
            private readonly LoadedDataManager _dataManager;

            public TextureLoader(LoadedDataManager dataManager, ICoroutineRunner coroutineRunner) : base(coroutineRunner)
            {
                _dataManager = dataManager;
            }
            
            public string[][] ParseCharacterTextures(string characterRace, TextureType textureType)
            {
                if (!_dataManager.RaceTextures.TryGetValue(characterRace, out var raceTexturesMaps)) return null;
                if (!raceTexturesMaps.TryGetValue(textureType, out var texturesMap)) return null;

                var texturePaths = texturesMap.texturePaths;
                var textures = new string[texturePaths.Count][];
                for (var i = 0; i < texturePaths.Count; i++)
                    textures[i] = texturePaths[i].colorPaths.ToArray();

                return textures;
            }
        }
    }
}