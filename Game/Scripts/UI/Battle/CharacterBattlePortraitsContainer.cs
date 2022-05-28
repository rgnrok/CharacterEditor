using System.Collections;
using System.Collections.Generic;
using CharacterEditor;
using UnityEngine;

public class CharacterBattlePortraitsContainer : MonoBehaviour
{
    [SerializeField]
    private GameObject _characterPortraitUIPrefab;
    private Dictionary<string, CharacterBattlePortraitUI> portraits = new Dictionary<string, CharacterBattlePortraitUI>();

    private void Start()
    {
        var battleManager = GameManager.Instance.BattleManager;

        battleManager.OnBattleStart += OnBattleStartHandler;
        battleManager.OnBattleEnd += OnBattleEndHandler;
        battleManager.OnEntityAdded += OnEntityAddedHandler;
        battleManager.OnEntityRemoved += OnEntityRemovedHandler;
        battleManager.OnTurnCompleted += OnTurnCompletedHandler;
        battleManager.OnTurnStarted += OnTurnStartedHandler;
    }



    private void OnBattleStartHandler()
    {
        gameObject.SetActive(true);
    }

    private void OnBattleEndHandler()
    {
        gameObject.SetActive(false);
        foreach (var portrait in portraits.Values)
        {
            Destroy(portrait.gameObject);
        }
        portraits.Clear();
    }

    private void OnEntityAddedHandler(IBattleEntity entity)
    {
        var portrait = Instantiate(_characterPortraitUIPrefab, transform).GetComponent<CharacterBattlePortraitUI>();
        if (portrait == null) return;

        portrait.Init(entity);
        portraits[entity.Guid] = portrait;
    }

    private void OnEntityRemovedHandler(IBattleEntity entity)
    {
        if (!portraits.ContainsKey(entity.Guid)) return;
        portraits[entity.Guid].Clean();
        portraits.Remove(entity.Guid);
    }

    private void OnTurnCompletedHandler(IBattleEntity entity)
    {
        if (!portraits.ContainsKey(entity.Guid)) return;
        portraits[entity.Guid].transform.SetSiblingIndex(transform.childCount);
        portraits[entity.Guid].SetSelected(false);
    }

    private void OnTurnStartedHandler(IBattleEntity entity)
    {
        if (!portraits.ContainsKey(entity.Guid)) return;
        portraits[entity.Guid].SetSelected(true);
    }
}
