using System;
using UnityEngine;

namespace CharacterEditor.Services
{
    public interface ICharacterMoveService : IService //Separate attack and move
    {
        void FireShowMovePoint(string characterGuid, Vector3 point);
        void FireShowAttackPoint(string characterGuid, Vector3 point);
        void FireHideCharacterPointer(string characterGuid);

        void AddObserver(ICharacterMoveObserver observer);
        void RemoveObserver(ICharacterMoveObserver observer);
    }

    public interface ICharacterMoveObserver
    {
        void ShowMovePoint(string characterGuid, Vector3 point);
        void ShowAttackPoint(string characterGuid, Vector3 point);
        void HideCharacterPointer(string characterGuid);
    }
}