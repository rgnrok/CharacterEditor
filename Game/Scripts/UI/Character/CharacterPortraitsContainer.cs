using System.Collections;
using System.Collections.Generic;
using CharacterEditor;
using UnityEngine;

public class CharacterPortraitsContainer : MonoBehaviour
{
    [SerializeField]
    private GameObject _characterPortraitUIPrefab;
    private readonly Dictionary<string, CharacterPortraitUI> portraits = new Dictionary<string, CharacterPortraitUI>();

    private void Start()
    {
        GameManager.Instance.OnAddCharacter += OnAddCharacterHandler;
        GameManager.Instance.OnRemoveCharacter += OnRemoveCharacterHandler;
    }
    private void OnDestroy()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.OnAddCharacter -= OnAddCharacterHandler;
        GameManager.Instance.OnRemoveCharacter -= OnRemoveCharacterHandler;
    }

    private void OnAddCharacterHandler(Character character)
    {
        var portrait = Instantiate(_characterPortraitUIPrefab, transform).GetComponent<CharacterPortraitUI>();
        if (portrait == null) return;

        portrait.Init(character);
        portraits[character.guid] = portrait;

    }

    private void OnRemoveCharacterHandler(Character character)
    {
        var characterGuid = character.guid;
        if (!portraits.ContainsKey(characterGuid)) return;
        portraits[characterGuid].Clean();
        portraits.Remove(characterGuid);
    }
}
