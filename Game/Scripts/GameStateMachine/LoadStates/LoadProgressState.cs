using CharacterEditor.Services;

namespace Game
{
    public class LoadProgressState : IState
    {
        private readonly IFSM _fsm;
        private readonly ISaveLoadService _saveService;

        public LoadProgressState(IFSM fsm, ISaveLoadService saveService)
        {
            _fsm = fsm;
            _saveService = saveService;
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
            var lastSave = _saveService.GetLastSave();
            if (string.IsNullOrEmpty(lastSave)) _fsm.SpawnEvent((int) GameStateMachine.GameStateType.CreateGame);
            else _fsm.SpawnEvent((int)GameStateMachine.GameStateType.LoadGame, lastSave);
        }
    }
}