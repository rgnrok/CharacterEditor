using System.Collections.Generic;
using CharacterEditor;
using CharacterEditor.Services;
using UnityEngine;

public class CharacterBattlePortraitsContainer : MonoBehaviour
{
    [SerializeField]
    private GameObject _characterPortraitUIPrefab;

    private readonly Dictionary<string, CharacterBattlePortraitUI> _portraits = new Dictionary<string, CharacterBattlePortraitUI>();
    private IBattleManageService _battleManager;

    private void Start()
    {
        _battleManager = AllServices.Container.Single<IBattleManageService>();
        if (_battleManager == null) return;

        _battleManager.OnBattleStart += OnBattleStartHandler;
        _battleManager.OnBattleEnd += OnBattleEndHandler;
        _battleManager.OnEntityAdded += OnEntityAddedHandler;
        _battleManager.OnEntityRemoved += OnEntityRemovedHandler;
        _battleManager.OnTurnCompleted += OnTurnCompletedHandler;
        _battleManager.OnTurnStarted += OnTurnStartedHandler;
    }

    private void OnDestroy()
    {
        if (_battleManager == null) return;

        _battleManager.OnBattleStart -= OnBattleStartHandler;
        _battleManager.OnBattleEnd -= OnBattleEndHandler;
        _battleManager.OnEntityAdded -= OnEntityAddedHandler;
        _battleManager.OnEntityRemoved -= OnEntityRemovedHandler;
        _battleManager.OnTurnCompleted -= OnTurnCompletedHandler;
        _battleManager.OnTurnStarted -= OnTurnStartedHandler;
    }

    private void OnBattleStartHandler()
    {
        gameObject.SetActive(true);
    }

    private void OnBattleEndHandler()
    {
        gameObject.SetActive(false);
        foreach (var portrait in _portraits.Values)
            Destroy(portrait.gameObject);

        _portraits.Clear();
    }

    private void OnEntityAddedHandler(IBattleEntity entity)
    {
        var goInstance = Instantiate(_characterPortraitUIPrefab, transform);
        var portrait = goInstance.GetComponent<CharacterBattlePortraitUI>();
        if (portrait == null)
        {
            Destroy(goInstance);
            return;
        }

        portrait.Init(entity);
        _portraits[entity.Guid] = portrait;
    }

    private void OnEntityRemovedHandler(IBattleEntity entity)
    {
        if (!_portraits.ContainsKey(entity.Guid)) return;
        _portraits[entity.Guid].Clean();
        _portraits.Remove(entity.Guid);
    }

    private void OnTurnCompletedHandler(IBattleEntity entity)
    {
        if (!_portraits.ContainsKey(entity.Guid)) return;
        _portraits[entity.Guid].transform.SetSiblingIndex(transform.childCount);
        _portraits[entity.Guid].SetSelected(false);
    }

    private void OnTurnStartedHandler(IBattleEntity entity)
    {
        if (!_portraits.ContainsKey(entity.Guid)) return;
        _portraits[entity.Guid].SetSelected(true);
    }
}
