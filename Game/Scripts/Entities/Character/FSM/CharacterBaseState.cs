namespace CharacterEditor
{
    public class CharacterBaseState: IState
    {
        protected CharacterFSM _fsm;
        protected Character _character;
        protected virtual void OnEnemyClick(IAttacked attacked) { }

        public CharacterBaseState(CharacterFSM fsm)
        {
            _fsm = fsm;
            _character = fsm.Character;
        }

        public void Enter()
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