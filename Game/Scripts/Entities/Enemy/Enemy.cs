using System;
using System.Collections.Generic;
using CharacterEditor;
using StatSystem;
using UnityEngine;

namespace EnemySystem
{
    public class Enemy: Entity<EnemyGameObjectData, EnemyConfig>, IBattleEntity, IAttacked, IHover, ICleanable
    {
        public Sprite Portrait { get; private set; }
        private EnemyFSM _enemyFSM;
        public IFSM FSM => _enemyFSM;

        public EnemyAttackComponent AttackComponent { get; private set; }

        public Vital ActionPoints { get { return StatCollection.GetStat<Vital>(StatType.ActionPoint); } }


        public event Action<IBattleEntity> OnDied;

  
        public Enemy(EnemySaveData data, EnemyGameObjectData gameObjectData, Texture2D texture, Sprite portrait) : base(data.guid, gameObjectData, texture, data.GetStats())
        {
            Portrait = portrait;
        }

        public Enemy(string guid, StatCollection stats, EnemyGameObjectData gameObjectData, Texture2D texture, Sprite portrait) : base(guid, gameObjectData, texture, stats)
        {
            Portrait = portrait;
        }

        public void CleanUp()
        {
            FSM.CurrentState.Exit();
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