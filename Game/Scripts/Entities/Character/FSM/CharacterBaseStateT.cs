namespace CharacterEditor
{
    public class CharacterBaseStateT<T>: IPayloadedState<T>
    {
        protected Character _character;
        protected CharacterFSM _fsm;

        protected virtual void OnEnemyClick(IAttacked attacked) { }

        public CharacterBaseStateT(CharacterFSM fsm)
        {
            _fsm = fsm;
            _character = fsm.Character;
        }

        public void Enter(T param)
        {
            GameManager.Instance.OnEnemyClick += OnEnemyClickHandler;
        }

        public void Exit()
        {
            GameManager.Instance.OnEnemyClick -= OnEnemyClickHandler;
        }

        private void OnEnemyClickHandler(string characterGuid, IAttacked attacked)
        {
            if (_character == null || _character.guid != characterGuid) return;

            OnEnemyClick(attacked);
        }
    }
}