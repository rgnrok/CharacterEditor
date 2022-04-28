using CharacterEditor;

namespace Game
{
    public class LoadProgressState : IState
    {
        private readonly IFSM _fsm;
        private readonly ISaveLoadService _saveLoadService;

        public LoadProgressState(IFSM fsm, ISaveLoadService saveLoadService)
        {
            _fsm = fsm;
            _saveLoadService = saveLoadService;
        }

        public void Enter()
        {
            LoadProgress();
        }

        public void Exit()
        {
            
        }

        private void LoadProgress()
        {
            var lastSave = _saveLoadService.GetLastSave();
            if (string.IsNullOrEmpty(lastSave)) _fsm.SpawnEvent((int) GameStateMachine.GameStateType.CreateGame);
            else _fsm.SpawnEvent((int)GameStateMachine.GameStateType.LoadGame, lastSave);
        }
    }
}