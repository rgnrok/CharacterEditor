namespace CharacterEditor
{
    public class CharacterAttackComponent : EntityAttackComponent
    {
        private Character _character;
        private CharacterBattleFSM _characteBattleFsm;

        public CharacterAttackComponent(Character character) : base(character.GameObjectData.CharacterObject)
        {
            _character = character;
        }

        public override AttackComponent GetCurrentAttackComponent()
        {
            var weapon = _character.GetWeapon();
            return _meleAttackComponent;
        }
    }
}