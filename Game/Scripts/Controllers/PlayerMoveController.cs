using System;
using System.Collections.Generic;
using CharacterEditor;
using UnityEngine;

public class PlayerMoveController : MonoBehaviour
{
    [SerializeField] private GameObject movePointer;
    [SerializeField] private GameObject attackMovePointer;

    private bool _isGroundClick;

    public Action CurrentCharacterPositionChanged;
    private Dictionary<string, GameObject> _movePointers = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> _attackMovePointers = new Dictionary<string, GameObject>();
    private InputManager _inputManager;

    public event Action<string, Vector3> OnGroundClick;


    private void Start()
    {
        _inputManager = AllServices.Container.Single<InputManager>();
        _inputManager.GroundUpClick += GroundUpClickHandler;
        _inputManager.GroundDownClick += GroundDownClickHandler;
    }

    private void OnDestroy()
    {
        if (_inputManager != null)
        {
            _inputManager.GroundUpClick += GroundUpClickHandler;
            _inputManager.GroundDownClick += GroundDownClickHandler;
        }
    }

    private void GroundDownClickHandler(RaycastHit hit)
    {
        _isGroundClick = true;
    }

    private void GroundUpClickHandler(RaycastHit hit)
    {
        if (!_isGroundClick) return;

        if (OnGroundClick != null)
        {
            var ch = GameManager.Instance.CurrentCharacter;
            if (ch != null) OnGroundClick(ch.guid, hit.point);
        }
        _isGroundClick = false;
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


 

    public void ShowCharacterPointer(Character character, Vector3 point, bool isAttack = false)
    {
        if (character == null) return;

        HideCharacterPointer(character);

        var collection = isAttack ? _attackMovePointers : _movePointers;
        var colPointer = isAttack ? attackMovePointer : movePointer;

        GameObject pointer;
        if (!collection.TryGetValue(character.guid, out pointer))
        {
            pointer = Instantiate(colPointer, point + colPointer.transform.position, colPointer.transform.rotation);
            collection[character.guid] = pointer;
        }

        pointer.transform.position = point + colPointer.transform.position;
       

        pointer.SetActive(true);
    }

    public void HideCharacterPointer(Character character)
    {
        if (character == null) return;
        HideCharacterPointer(character.guid);
    }

    public void HideCharacterPointer(string characterGuid)
    {
        GameObject pointer;
        if (_movePointers.TryGetValue(characterGuid, out pointer)) pointer.SetActive(false);
        if (_attackMovePointers.TryGetValue(characterGuid, out pointer)) pointer.SetActive(false);

    }
}
