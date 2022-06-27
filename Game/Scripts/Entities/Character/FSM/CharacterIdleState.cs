using CharacterEditor;
using CharacterEditor.FmsPayload;
using CharacterEditor.Services;
using UnityEngine;

public class CharacterIdleState : IState
{
    private readonly CharacterFSM _fsm;
    private readonly IInputService _inputService;
    private readonly ICharacterManageService _characterManageService;
    private readonly Character _character;
    private readonly PlayerMoveComponent _moveComponent;
    private GameManager _gameManager;

    public CharacterIdleState(CharacterFSM fsm, IInputService inputService, ICharacterManageService characterManageService)
    {
        _fsm = fsm;
        _inputService = inputService;
        _characterManageService = characterManageService;

        _character = fsm.Character;
        _moveComponent = _character.MoveComponent;
    }

    public void Enter()
    {
        _moveComponent.Stop(true);

        _inputService.GroundClick += OnGroundClickHandler;
        _inputService.ContainerGameObjectClick += ContainerGameObjectClickHandler;

        _gameManager = GameManager.Instance; //todo
        _gameManager.OnEnemyClick += OnEnemyClickHandler;

    }

    public void Exit()
    {
        _inputService.GroundClick -= OnGroundClickHandler;
        _inputService.ContainerGameObjectClick -= ContainerGameObjectClickHandler;

        if (_gameManager != null)
            _gameManager.OnEnemyClick -= OnEnemyClickHandler;


    }

    private void ContainerGameObjectClickHandler(RaycastHit containerHit)
    {
        if (!IsCurrentCharacter()) return;

        if (Helper.IsNear(_character.GameObjectData.CharacterObject.transform.position, containerHit.point))
        {
            _character.MoveComponent.RotateTo(containerHit.point);

            var container = containerHit.transform.GetComponent<Container>();
            if (container == null) return;

            _gameManager.OpenContainer(container);
        }
        else
        {
            _fsm.SpawnEvent((int)CharacterFSM.CharacterStateType.Move, 
                new MovePayload(containerHit.point,
                    () =>
                    {
                        ContainerGameObjectClickHandler(containerHit);
                    })
                );
        }
    }


    private void OnEnemyClickHandler(string characterGuid, IAttacked attacked)
    {
        if (!IsCurrentCharacter()) return;

        _fsm.SpawnEvent((int)CharacterFSM.CharacterStateType.Attack, attacked);
    }

    private void OnGroundClickHandler(Vector3 point)
    {
        if (!IsCurrentCharacter()) return;

        _fsm.SpawnEvent((int)CharacterFSM.CharacterStateType.Move, new MovePayload(point));
    }

    private bool IsCurrentCharacter() => 
        _character == _characterManageService?.CurrentCharacter;
}
