using System;
using System.Collections;
using System.Collections.Generic;
using CharacterEditor.Services;
using EnemySystem;
using Game;
using UnityEngine;

namespace CharacterEditor
{
    public class BattleManager : IBattleManageService
    {
        private readonly List<IBattleEntity> _battleEntities = new List<IBattleEntity>();
        private readonly List<IBattleEntity> _enemies = new List<IBattleEntity>();
        private readonly List<IBattleEntity> _characters = new List<IBattleEntity>();

        private readonly List<IBattleEntity> _currentRoundEntities = new List<IBattleEntity>();
        private IBattleEntity _currentEntity;

        private int _round;
        private bool _isBattleStart;
        private readonly ICoroutineRunner _coroutineRunner;

        public event Action<IBattleEntity> OnTurnCompleted;
        public event Action<IBattleEntity> OnTurnStarted;
        public event Action OnBattleStart;
        public event Action OnBattleEnd;
        public event Action OnNewRoundStart;
        public event Action<IBattleEntity> OnEntityAdded;
        public event Action<IBattleEntity> OnEntityRemoved;

        public BattleManager(ICoroutineRunner coroutineRunner)
        {
            _coroutineRunner = coroutineRunner;
        }

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

            SortEntities();

            if (!_isBattleStart && _characters.Count > 0 && _enemies.Count > 0) StartBattle();
            OnEntityAdded?.Invoke(entity);
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

            OnEntityRemoved?.Invoke(entity);
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
            _battleEntities.Sort((a, b) => a.Guid.CompareTo(b.Guid));
        }

        private void StartBattle()
        {
            _round = 1;
            _isBattleStart = true;

            OnBattleStart?.Invoke();
            _coroutineRunner.StartCoroutine(StartNewRound());
        }

        private void EndBattle()
        {
            _isBattleStart = false;
            _characters.Clear();
            _enemies.Clear();
            _battleEntities.Clear();

            OnBattleEnd?.Invoke();
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
        }
        
        private void EntityEndTurn()
        {
            OnTurnCompleted?.Invoke(_currentEntity);

            _currentRoundEntities.Remove(_currentEntity);
            if (_currentRoundEntities.Count == 0)
            {
                _coroutineRunner.StartCoroutine(StartNewRound());
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
            OnNewRoundStart?.Invoke();
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
            {
                _currentEntity.StartTurn(_characters);
            }

            OnTurnStarted?.Invoke(_currentEntity);
        }
    }
}