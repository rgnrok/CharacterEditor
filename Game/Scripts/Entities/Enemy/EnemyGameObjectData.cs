using System.Collections.Generic;
using CharacterEditor;
using UnityEngine;

namespace EnemySystem
{
    public class EnemyGameObjectData : EntityGameObjectData<EnemyConfig>
    {
        public EnemyGameObjectData(EnemyConfig config, GameObject enemyGo) : base(config, enemyGo)
        {
        }

     
    }
}