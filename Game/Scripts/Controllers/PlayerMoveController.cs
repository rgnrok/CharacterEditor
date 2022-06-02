using System;
using System.Collections.Generic;
using CharacterEditor;
using CharacterEditor.Services;
using UnityEngine;

public class PlayerMoveController : MonoBehaviour, ICharacterMoveObserver
{
    [SerializeField] private GameObject movePointer;
    [SerializeField] private GameObject attackMovePointer;

    private Dictionary<string, GameObject> _movePointers = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> _attackMovePointers = new Dictionary<string, GameObject>();

    private InputService _inputService;
    private ICharacterMoveService _characterMoveService;


    private void Start()
    {
        _characterMoveService = AllServices.Container.Single<ICharacterMoveService>();
        if (_characterMoveService != null)
            _characterMoveService.AddObserver(this);
    }

    private void OnDestroy()
    {
        if (_characterMoveService != null)
            _characterMoveService.RemoveObserver(this);
    }

    public void CurrentCharacterStop()
    {
//        todo !! Move to FSM. How? Dont know
//        if (_currentPlayer != null)
//            _currentPlayer.Stop();
    }

    public void LookCurrentCharacterToPoint(Vector3 point)
    {
//        todo !! Move to FSM. How? Dont know
//        if (_currentPlayer != null)
//            _currentPlayer.RotateTo(point);
    }

    public void ShowMovePoint(string characterGuid, Vector3 point)
    {
        ShowPointer(characterGuid, point, _movePointers, movePointer);
    }

    public void ShowAttackPoint(string characterGuid, Vector3 point)
    {
        ShowPointer(characterGuid, point, _attackMovePointers, attackMovePointer);
    }

    private void ShowPointer(string characterGuid, Vector3 point, Dictionary<string, GameObject> pointerCollection, GameObject pointerPrefab)
    {
        if (string.IsNullOrEmpty(characterGuid)) return;
        HideCharacterPointer(characterGuid);

        if (!pointerCollection.TryGetValue(characterGuid, out var pointer))
        {
            pointer = Instantiate(pointerPrefab, point, pointerPrefab.transform.rotation);
            pointerCollection[characterGuid] = pointer;
        }

        pointer.transform.position = point + pointerPrefab.transform.position;
        pointer.SetActive(true);
    }


    public void HideCharacterPointer(string characterGuid)
    {
        if (_movePointers.TryGetValue(characterGuid, out var pointer)) pointer.SetActive(false);
        if (_attackMovePointers.TryGetValue(characterGuid, out pointer)) pointer.SetActive(false);
    }
}
