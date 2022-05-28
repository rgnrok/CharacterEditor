using System;
using System.Collections.Generic;
using CharacterEditor;
using StatSystem;
using UnityEngine;

namespace EnemySystem
{
    public class Enemy: Entity<EnemyGameObjectData, EnemyConfig>, IBattleEntity, IAttacked, IHover
    {
        public Sprite Portrait { get; private set; }
        private EnemyFSM _enemyFSM;
        public IFSM FSM => _enemyFSM;

        public EnemyAttackComponent AttackComponent { get; private set; }

        public Vital ActionPoints { get { return StatCollection.GetStat<Vital>(StatType.ActionPoint); } }


        public event Action<IBattleEntity> OnDied;

        public static Enemy Create(EnemySaveData data, EnemyGameObjectData gameObjectData, Texture2D texture, Sprite portrait)
        {
            var enemy = new Enemy(data, gameObjectData, texture, portrait);
            enemy.Init();
            return enemy;
        }

        public static Enemy Create(string guid, EnemyGameObjectData gameObjectData, Texture2D texture, Sprite portrait)
        {
            var enemy = new Enemy(guid, gameObjectData, texture, portrait);
            enemy.Init();
            return enemy;
        }

        private Enemy(EnemySaveData data, EnemyGameObjectData gameObjectData, Texture2D texture, Sprite portrait) : base(data, gameObjectData, texture)
        {
            Portrait = portrait;
        }

        private Enemy(string guid, EnemyGameObjectData gameObjectData, Texture2D texture, Sprite portrait) : base(guid, gameObjectData, texture)
        {
            Portrait = portrait;
        }

        protected override void InternalInit()
        {
            base.InternalInit();

            _enemyFSM = new EnemyFSM(this);
            _enemyFSM.Start();
            AttackComponent = new EnemyAttackComponent(this);

            var canvas = GameObjectData.Entity.GetComponentInChildren<EntityCanvas>();
            if (canvas != null) canvas.Init(this);
        }

        protected override void OnDie()
        {
            base.OnDie();
            OnDied?.Invoke(this);
            _enemyFSM.SpawnEvent((int)EnemyFSM.EnemyStateType.Dead);
        }

        public void StartBattle()
        {
            _enemyFSM.SpawnEvent((int)EnemyFSM.EnemyStateType.Battle);
        }

        public void StartTurn(List<IBattleEntity> enemies)
        {
            _enemyFSM.StartTurn(enemies);
        }

        public void ProcessTurn()
        {
            _enemyFSM.ProcessTurn();
        }

        bool IBattleEntity.IsTurnComplete()
        {
            return _enemyFSM.IsTurnComplete();
        }
    }
}