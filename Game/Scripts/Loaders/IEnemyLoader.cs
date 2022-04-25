using System;
using System.Collections.Generic;

namespace CharacterEditor
{
    public interface IEnemyLoader : IService
    {
        void LoadEnemies(Action<Dictionary<string, EnemyConfig>> callback);

        void LoadEnemy(string guid, Action<EnemyConfig> callback);

        void LoadEnemies(List<string> guids, Action<Dictionary<string, EnemyConfig>> callback);
    }
}
