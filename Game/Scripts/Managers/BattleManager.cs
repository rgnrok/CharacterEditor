using System;
using System.Collections;
using System.Collections.Generic;
using EnemySystem;
using UnityEngine;

namespace CharacterEditor
{
    public class BattleManager
    {
        private List<IBattleEntity> _battleEntities = new List<IBattleEntity>();
        private List<IBattleEntity> _enemies = new List<IBattleEntity>();
        private List<IBattleEntity> _characters = new List<IBattleEntity>();

        private List<IBattleEntity> _currentRoundEntities = new List<IBattleEntity>();
        private IBattleEntity _currentEntity;

        private int _round;
        private bool _isBattleStart;

        public event Action<IBattleEntity> OnTurnCompleted;
        public event Action<IBattleEntity> OnTurnStarted;
        public event Action OnBattleStart;
        public event Action OnBattleEnd;
        public event Action OnNewRoundStart;
        public event Action<IBattleEntity> OnEntityAdded;
        public event Action<IBattleEntity> OnEntityRemoved;

        public void Update()
        {
            ProcessRound();
        }

        private void AddEntity(IBattleEntity entity)
        {
            if (_battleEntities.Contains(entity)) return;
            entity.OnDied += EntityOnDiedHandler;


            entity.StartBattle();
            _currentRoundEntities.Add(entity);
            _battleEntities.Add(entity);

            SortEntities(); //??

            if (!_isBattleStart && _characters.Count > 0 && _enemies.Count > 0) StartBattle();
            if (OnEntityAdded != null) OnEntityAdded(entity);
        }

        private void EntityOnDiedHandler(IBattleEntity entity)
        {
            entity.OnDied -= EntityOnDiedHandler;
            RemoveEntity(entity);
        }

        private void RemoveEntity(IBattleEntity entity)
        {
            if (!_battleEntities.Contains(entity)) return;
            entity.OnDied -= EntityOnDiedHandler;

            _currentRoundEntities.Remove(entity);
            _battleEntities.Remove(entity);

            if (entity is Character) _characters.Remove(entity);
            else _enemies.Remove(entity);

            SortEntities(); //??

            if (OnEntityRemoved != null) OnEntityRemoved(entity);
            if (_isBattleStart && (_characters.Count == 0 || _enemies.Count == 0)) EndBattle();

        }

        public void AddCharacter(Character character)
        {
            if (_characters.Contains(character)) return;

            _characters.Add(character);
            AddEntity(character);
        }

        public void AddEnemy(Enemy enemy)
        {
            if (_enemies.Contains(enemy)) return;

            _enemies.Add(enemy);
            AddEntity(enemy);
        }

        private void SortEntities()
        {
            _battleEntities.Sort((a, b) => a.guid.CompareTo(b.guid));
        }

        private void StartBattle()
        {
            _round = 1;
            _isBattleStart = true;

            if (OnBattleStart != null) OnBattleStart();
            Helper.StartCoroutine(StartNewRound());
        }

        private void EndBattle()
        {
            _isBattleStart = false;
            _characters.Clear();
            _enemies.Clear();
            _battleEntities.Clear();

            if (OnBattleEnd != null) OnBattleEnd();
        }

        private void ProcessRound()
        {
            if (!_isBattleStart) return;
            if (_currentEntity == null) return;

            if (_currentEntity.IsTurnComplete())
            {
                EntityEndTurn();
                return;
            }

             _currentEntity.ProcessTurn();

//            if (_fakeEn != null) return;
//            _fakeEn = GameManager.Instance.StartCoroutine(FakeCor());
        }

   

        private void EntityEndTurn()
        {
            if (OnTurnCompleted != null) OnTurnCompleted(_currentEntity);

            _currentRoundEntities.Remove(_currentEntity);
            if (_currentRoundEntities.Count == 0)
            {
                Helper.StartCoroutine(StartNewRound());
                return;
            }

            StartTurn();
        }

        private IEnumerator StartNewRound()
        {
            _currentEntity = null;
            yield return new WaitForSecondsRealtime(1f);
            _round++;
            _currentRoundEntities.Clear();
            _currentRoundEntities.AddRange(_battleEntities);

            SortEntities();

            StartTurn();
            if (OnNewRoundStart != null) OnNewRoundStart();
        }

        private void StartTurn()
        {
            _currentEntity = _currentRoundEntities[0];

            var character = _currentEntity as Character;
            if (character != null)
            {
                _currentEntity.StartTurn(_enemies);
                GameManager.Instance.SetCharacter(character, true);
            }
            else
                _currentEntity.StartTurn(_characters);

            if (OnTurnStarted != null) OnTurnStarted(_currentEntity);
        }
    }
}