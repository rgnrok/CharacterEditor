using System;
using UnityEngine;

public class AnimatorEventReceiver : MonoBehaviour
{
    public Action OnAttack;

    public void AnimationOnAttackEvent()
    {
        OnAttack?.Invoke();
    }
}