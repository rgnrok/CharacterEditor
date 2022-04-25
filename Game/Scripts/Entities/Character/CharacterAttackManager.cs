using System;
using CharacterEditor.CharacterInventory;
using UnityEngine;

namespace CharacterEditor
{
    public class CharacterAttackManager : EntityAttackManager
    {
        private Character _character;
        private CharacterBattleFSM _characteBattleFsm;

        public CharacterAttackManager(Character character) : base(character.GameObjectData.CharacterObject)
        {
            _character = character;
        }

        protected override AttackComponent GetCurrentAttackComponent()
        {
            var weapon = _character.GetWeapon();
            return _meleAttackComponent;
        }

       
    }
}