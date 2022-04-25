using System;
using System.Collections.Generic;
using CharacterEditor;
using StatSystem;
using UnityEngine;

namespace EnemySystem
{
    public class Enemy: Entity<EnemyGameObjectData, EnemyConfig>, IBattleEntity, IAttacked
    {
        public Sprite Portrait { get; private set; }
        public EnemyFSM FSM { get; private set; }
        public FSM BaseFSM => FSM;

        public GameObject EntityGameObject { get {return GameObjectData.Entity; } }

        public EnemyAttackManager AttackManager { get; private set; }

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

        protected override void Init()
        {
            base.Init();
            FSM = new EnemyFSM(this);
            FSM.Start();
            AttackManager = new EnemyAttackManager(this);

            var canvas = GameObjectData.Entity.GetComponentInChildren<EntityCanvas>();
            if (canvas != null) canvas.Init(this);
        }

        protected override void Die()
        {
            base.Die();
            if (OnDied != null) OnDied(this);
            FSM.SpawnEvent((int)EnemyFSM.EnemyStateType.Dead);
        }

        public void StartBattle()
        {
            FSM.SpawnEvent((int)EnemyFSM.EnemyStateType.Battle);
        }

        public void StartTurn(List<IBattleEntity> enemies)
        {
            FSM.StartTurn(enemies);
        }

        public void ProcessTurn()
        {
            FSM.ProcessTurn();
        }

        bool IBattleEntity.IsTurnComplete()
        {
            return FSM.IsTurnComplete();
        }
    }
}