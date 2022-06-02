using System.Collections.Generic;
using UnityEngine;

namespace CharacterEditor.Services
{
    class CharacterMoveService : ICharacterMoveService
    {
        private readonly List<ICharacterMoveObserver> _observers = new List<ICharacterMoveObserver>();

        public void FireShowMovePoint(string characterGuid, Vector3 point)
        {
            foreach (var observer in _observers)
                observer.ShowMovePoint(characterGuid, point);
        }

        public void FireShowAttackPoint(string characterGuid, Vector3 point)
        {
            foreach (var observer in _observers)
                observer.ShowAttackPoint(characterGuid, point);
        }

        public void FireHideCharacterPointer(string characterGuid)
        {
            foreach (var observer in _observers)
                observer.HideCharacterPointer(characterGuid);
        }

        public void AddObserver(ICharacterMoveObserver observer)
        {
            _observers.Add(observer);
        }

        public void RemoveObserver(ICharacterMoveObserver observer)
        {
            _observers.Remove(observer);
        }
    }
}