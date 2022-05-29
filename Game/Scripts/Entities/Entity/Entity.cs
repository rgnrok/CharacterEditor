using StatSystem;
using UnityEngine;

namespace CharacterEditor
{
    public abstract class Entity<TGoData, TConfig> : IIdentifiable where TGoData : EntityGameObjectData<TConfig> where TConfig: EntityConfig
    {
        public Texture2D Texture { get; }

        public string Guid { get; }
        public string ConfigGuid { get; }
        public TGoData GameObjectData { get; }

        public StatCollection StatCollection { get; private set; }

        public Vital Health => 
            StatCollection.GetStat<Vital>(StatType.Health);

        public bool IsAlive() => 
            Health.StatCurrentValue > 0;

        public GameObject EntityGameObject => 
            GameObjectData.Entity;

        private bool _initialized;


        protected Entity(string guid, TGoData gameObjectData, Texture2D texture, StatCollection stats)
        {
            Guid = guid;
            ConfigGuid = gameObjectData.Config.guid;

            GameObjectData = gameObjectData;
            Texture = texture;
            StatCollection = stats;
        }

        public void Init()
        {
            if (_initialized) return;

            InternalInit();
            _initialized = true;
        }

        protected virtual void InternalInit()
        {
            Health.OnCurrentValueChange += HealthChanged;
        }

        private void HealthChanged()
        {
            if (Health.StatCurrentValue <= 0)
                Die();
        }

        private void Die()
        {
            Health.OnCurrentValueChange -= HealthChanged;
            OnDie();
        }

        protected virtual void OnDie()
        {

        }
    }
}
