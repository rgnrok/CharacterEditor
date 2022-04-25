using System.Collections.Generic;
using System.Linq;
using CharacterEditor.JSONMap;
using UnityEngine;

namespace CharacterEditor
{
    public class LoadedDataManager
    {
        public Dictionary<string, RaceMap> Races { get; private set; }
        public Dictionary<string, Dictionary<MeshType, MeshesMap>> RaceMeshes { get; private set; }
        public Dictionary<string, Dictionary<TextureType, TexturesMap>> RaceTextures { get; private set; }

        public Dictionary<string, GuidPathMap> Items;
        public Dictionary<string, GuidPathMap> PlayerCharacters;
        public Dictionary<string, GuidPathMap> Containers;
        public Dictionary<string, GuidPathMap> Enemies;

        public LoadedDataManager(string mapConfigPath)
        {
            var targetFile = Resources.Load<TextAsset>(mapConfigPath);

            var map = JsonUtility.FromJson<BundleMap>(targetFile.text);
            Races = map.races.ToDictionary(x => x.race, x => x);

            Items = map.items.ToDictionary(x => x.guid, x => x);
            PlayerCharacters = map.playerCharacters.ToDictionary(x => x.guid, x => x);
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
    }
}
