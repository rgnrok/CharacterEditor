using System;
using System.Collections;
using System.Collections.Generic;
using StatSystem;
using UnityEngine;

public interface IBattleEntity : IIdentifiable
{
    StatCollection StatCollection { get; }
    Vital Health { get; }
    Vital ActionPoints { get; }

    Sprite Portrait { get; }

    IFSM FSM { get; }

    GameObject EntityGameObject { get; }

    event Action<IBattleEntity> OnDied;

    void StartBattle();
    IEnumerator StartTurn(List<IBattleEntity> enemies);
    void ProcessTurn();

    bool IsTurnComplete();

}
