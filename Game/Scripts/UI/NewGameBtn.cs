using CharacterEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class NewGameBtn : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        var fsm = AllServices.Container.Single<IFSM>();
        fsm.SpawnEvent((int)GameStateMachine.GameStateType.CreateGame);
    }
}
