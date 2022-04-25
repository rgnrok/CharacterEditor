using System;
using System.Collections.Generic;
using CharacterEditor.CharacterInventory;
using StatSystem;
using UnityEditor;
using UnityEngine;

namespace CharacterEditor
{
    public abstract class Entity<TGoData, TConfig> : IHover where TGoData : EntityGameObjectData<TConfig> where TConfig: EntityConfig
    {
        public Texture2D Texture { get; private set; }
//        public EntityConfig Config { get; private set; }
//        public TData Data { get; protected set; }

        public string guid { get; protected set; }
        public string configGuid { get; protected set; }

        public TGoData GameObjectData { get; protected set; }

        public StatCollection StatCollection { get; private set; }

        public Vital Health
        {
            get { return StatCollection.GetStat<Vital>(StatType.Health); }
        }

        public bool IsAlive()
        {
            return Health.StatCurrentValue > 0;
        }

        public GameObject EntityGameObject { get { return GameObjectData.Entity;} }


        protected Entity()
        {
        }

        protected Entity(EntitySaveData data, TGoData gameObjectData, Texture2D texture) : this()
        {
            guid = data.guid;
            configGuid = data.configGuid;

            GameObjectData = gameObjectData;
            Texture = texture;
            StatCollection = new DefaultStatCollection(data.currentHealthValue, data.currentManaValue, data.stats);

            gameObjectData.Entity.transform.position = data.position;
            gameObjectData.Entity.transform.rotation = data.rotation;
        }

        protected Entity(string guid, TGoData gameObjectData, Texture2D texture) : this()
        {
            this.guid = guid;
            configGuid = gameObjectData.Config.guid;

            GameObjectData = gameObjectData;
            Texture = texture;
            StatCollection = new DefaultStatCollection();
        }

        protected virtual void Init()
        {
            Health.OnCurrentValueChange += HealthChanged;
        }


        private void HealthChanged()
        {
            if (Health.StatCurrentValue <= 0)
            {
                Die();
            }
        }

        protected virtual void Die()
        {

        }
    }
}

