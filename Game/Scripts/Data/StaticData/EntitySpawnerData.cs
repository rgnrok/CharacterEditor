using System;
using UnityEngine;

namespace CharacterEditor.StaticData
{
    [Serializable]
    public class EntitySpawnerData
    {
        public string Id;
        public string ConfigId;
        public Vector3 Position;

        public EntitySpawnerData(string id, string configId, Vector3 position)
        {
            Id = id;
            ConfigId = configId;
            Position = position;
        }
    }

}