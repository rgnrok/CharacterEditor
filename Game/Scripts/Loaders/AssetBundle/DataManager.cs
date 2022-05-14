using System.Collections.Generic;
using System.Linq;
using CharacterEditor.JSONMap;
using UnityEngine;

namespace CharacterEditor
{
    namespace AssetBundleLoader
    {
        public class DataManager : IDataManager
        {
            public Dictionary<string, RaceMap> Races { get; private set; }
            public Dictionary<string, Dictionary<MeshType, MeshesMap>> RaceMeshes { get; private set; }
            public Dictionary<string, Dictionary<TextureType, TexturesMap>> RaceTextures { get; private set; }

            public Dictionary<string, GuidPathMap> Items;
            public Dictionary<string, GuidPathMap> PlayerCharacters;
            public Dictionary<string, GuidPathMap> Containers;
            public Dictionary<string, GuidPathMap> Enemies;

            public DataManager(string mapConfigPath)
            {
                var targetFile = Resources.Load<TextAsset>(mapConfigPath);

                var map = JsonUtility.FromJson<BundleMap>(targetFile.text);
                Races = map.races.ToDictionary(x => x.race, x => x);

                Items = map.items.ToDictionary(x => x.guid, x => x);
                PlayerCharacters = map.payableNpc.ToDictionary(x => x.guid, x => x);
                Containers = map.containers.ToDictionary(x => x.guid, x => x);
                Enemies = map.enemies.ToDictionary(x => x.guid, x => x);

                RaceMeshes = new Dictionary<string, Dictionary<MeshType, MeshesMap>>(Races.Count);
                RaceTextures = new Dictionary<string, Dictionary<TextureType, TexturesMap>>(Races.Count);
                foreach (var raceMap in Races)
                {
                    RaceMeshes[raceMap.Key] = raceMap.Value.meshes.ToDictionary(x => x.type, x => x);
                    RaceTextures[raceMap.Key] = raceMap.Value.textures.ToDictionary(x => x.type, x => x);
                }

                Resources.UnloadAsset(targetFile);
            }

            public string[][] ParseCharacterTextures(string characterRace, TextureType textureType)
            {
                if (!RaceTextures.TryGetValue(characterRace, out var raceTexturesMaps)) return null;
                if (!raceTexturesMaps.TryGetValue(textureType, out var texturesMap)) return null;

                var texturePaths = texturesMap.texturePaths;
                var textures = new string[texturePaths.Count][];
                for (var i = 0; i < texturePaths.Count; i++)
                    textures[i] = texturePaths[i].colorPaths;

                return textures;
            }

            public Dictionary<string, string[][]> ParseCharacterMeshes(string characterRace, MeshType meshType)
            {
                if (!RaceMeshes.TryGetValue(characterRace, out var raceMeshesMap)) return null;
                if (!raceMeshesMap.TryGetValue(meshType, out var meshesMap)) return null;

                return meshesMap.meshPaths.ToDictionary(
                    x => x.modelPath,
                    x =>
                    {
                        var texturePaths = new string[x.textures.Count][];
                        for (var i = 0; i < x.textures.Count; i++)
                            texturePaths[i] = x.textures[i].colorPaths.ToArray();

                        return texturePaths;
                    }
                );
            }
        }
    }
}
