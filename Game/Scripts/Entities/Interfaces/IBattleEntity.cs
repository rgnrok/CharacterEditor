using System;
using System.Collections.Generic;
using StatSystem;
using UnityEngine;

public interface IBattleEntity
{
    StatCollection StatCollection { get; }
    Vital Health { get; }
    Vital ActionPoints { get; }

    Sprite Portrait { get; }
    string guid { get; }

    FSM BaseFSM { get; }

    GameObject EntityGameObject { get; }

    event Action<IBattleEntity> OnDied;

    void StartBattle();
    void StartTurn(List<IBattleEntity> enemies);
    void ProcessTurn();

    bool IsTurnComplete();

}
