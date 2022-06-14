using System;
using EnemySystem;

namespace CharacterEditor.Services
{
    public interface IBattleManageService : IService
    {
        event Action<IBattleEntity> OnTurnCompleted;
        event Action<IBattleEntity> OnTurnStarted;
        event Action OnBattleStart;
        event Action OnBattleEnd;
        event Action OnNewRoundStart;
        event Action<IBattleEntity> OnEntityAdded;
        event Action<IBattleEntity> OnEntityRemoved;
        void AddCharacter(Character character);
        void AddEnemy(Enemy enemy);
        void Update();
    }
}