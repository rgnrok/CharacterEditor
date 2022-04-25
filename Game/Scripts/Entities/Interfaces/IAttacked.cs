using System;
using StatSystem;
using UnityEngine;

public interface IAttacked
{
    Vital Health { get; }
    GameObject EntityGameObject { get; }
    StatCollection StatCollection { get; }
    event Action<IBattleEntity> OnDied;
}
