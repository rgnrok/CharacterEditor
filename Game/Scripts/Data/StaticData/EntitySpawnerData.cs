using System;
using UnityEngine;

namespace CharacterEditor.StaticData
{
    [Serializable]
    public class EntitySpawnerData
    {
        public string Id;
        public Vector3 Position;

        public EntitySpawnerData(string id, Vector3 position)
        {
            Id = id;
            Position = position;
        }
    }

}