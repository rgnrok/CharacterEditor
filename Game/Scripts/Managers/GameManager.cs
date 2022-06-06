using System;
using System.Collections.Generic;
using CharacterEditor;
using CharacterEditor.CharacterInventory;
using CharacterEditor.Services;
using EnemySystem;
using Game;
using UnityEngine;
using UnityEngine.Profiling;

public class GameManager : MonoBehaviour, ICoroutineRunner
{
    public static GameManager Instance { get; private set; }
    public BattleManager BattleManager { get; private set; }

    public PlayerMoveController PlayerMoveController { get; private set; }
    public RenderPathController RenderPathController { get; private set; }

    public HoverManager HoverManager { get; private set; }
    public Transform Canvas { get; private set; }

    #region Characters
    public string MainCharacterGuid { get; set; }
    public Character CurrentCharacter { get; private set; }

    public Action<Character> OnChangeCharacter;
    public Action<Character> OnAddCharacter;
    public Action<Character> OnRemoveCharacter;

    public Action<string, IAttacked> OnEnemyClick;

    public Dictionary<string, Character> Characters { get; private set; }
    public Dictionary<string, Enemy> Enemies { get; private set; }

    // Игровые персонажи, которые в будущем могут стать персонажами игрока
    private Dictionary<string, Character> _npcPlayerCharacters = new Dictionary<string, Character>();
    public Dictionary<string, Character> NpcPlayerCharacters { get { return _npcPlayerCharacters; } }

    private FollowCamera _followCamera;


    #endregion

    #region Popups

    [SerializeField]
    private ItemData[] inventoryItems;

    [SerializeField] private Inventory inventoryPopup;
    [SerializeField] private CharacterPopup characterPopup;
    [SerializeField] private ContainerPopup containerPopup;

    public Dictionary<string, Container> OpenedContainers = new Dictionary<string, Container>();
    public Inventory Inventory { get { return inventoryPopup; } }
    public ContainerPopup ContainerPopup { get { return containerPopup; } }
    #endregion

    private ISaveLoadService _saveLoadService;
    private IInputService _inputService;
    private ICharacterEquipItemService _equipItemService;
    private ICharacterManageService _characterManageService;

    // private GameStateMachine _gameStateMachine;


    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        Instance = this;
        BattleManager = new BattleManager();

        _followCamera = Camera.main.GetComponent<FollowCamera>();

        PlayerMoveController = GetComponent<PlayerMoveController>();
        RenderPathController = GetComponent<RenderPathController>();

        Characters = new Dictionary<string, Character>();
        Enemies = new Dictionary<string, Enemy>();

        HoverManager = GetComponent<HoverManager>();
        // _gameStateMachine = new GameStateMachine(new SceneLoader(this), AllServices.Container);

        InitServices();
    }

    private void InitServices()
    {
        _inputService = AllServices.Container.Single<IInputService>();
        if (_inputService != null)
        {
            _inputService.SetupCamera(_followCamera);
            _inputService.CharacterGameObjectClick += CharacterGameObjectClickHandler;
            _inputService.EnemyGameObjectClick += EnemyGameObjectClickHandler;
            _inputService.NpcGameObjectClick += NpcGameObjectClickHandler;
            _inputService.ToggleInventory += ToggleInventoryHandler;
            _inputService.ToggleCharacterInfo += ToggleCharacterInfoHandler;
            _inputService.PickUpObjectClick += PickUpObjectClickHandler;
            _inputService.OnChangeMouseRaycastHit += OnChangeMouseRaycastHitHandler;
        }

        _saveLoadService = AllServices.Container.Single<ISaveLoadService>();
        if (_saveLoadService != null)
        {
            _saveLoadService.OnCharactersLoaded += OnCharactersLoadedHandler;
            _saveLoadService.OnPlayableNpcLoaded += OnPlayableNpcLoadedHandler;
            _saveLoadService.OnEnemiesLoaded += OnEnemiesLoadedHandler;
            _saveLoadService.OnLoadData += OnLoadDataHandler;
        }

        _equipItemService = AllServices.Container.Single<ICharacterEquipItemService>();
        _characterManageService = AllServices.Container.Single<ICharacterManageService>();
    }

    private void Start()
    {
        Canvas = GameObject.Find("Canvas").transform;
        // PlayerMoveController.CurrentCharacterPositionChanged += CurrentCharacterPositionChangedHandler;
        // _gameStateMachine.Start();
    }

    private void OnDestroy()
    {
        if (_saveLoadService != null)
        {
            _saveLoadService.OnCharactersLoaded -= OnCharactersLoadedHandler;
            _saveLoadService.OnPlayableNpcLoaded -= OnPlayableNpcLoadedHandler;
            _saveLoadService.OnEnemiesLoaded -= OnEnemiesLoadedHandler;
            _saveLoadService.OnLoadData -= OnLoadDataHandler;
        }

        if (_inputService != null)
        {
            _inputService.CharacterGameObjectClick -= CharacterGameObjectClickHandler;
            _inputService.EnemyGameObjectClick -= EnemyGameObjectClickHandler;
            _inputService.NpcGameObjectClick -= NpcGameObjectClickHandler;
            _inputService.ToggleInventory -= ToggleInventoryHandler;
            _inputService.ToggleCharacterInfo -= ToggleCharacterInfoHandler;
            _inputService.PickUpObjectClick -= PickUpObjectClickHandler;
            _inputService.OnChangeMouseRaycastHit -= OnChangeMouseRaycastHitHandler;
        }
    }


    public void SetCharacter(string guid)
    {
        if (Characters.ContainsKey(guid)) 
            SetCharacter(Characters[guid]);
    }


    private void RemoveCharacter(Character ch)
    {
        if (!Characters.Remove(ch.Guid)) return;
        OnRemoveCharacter?.Invoke(ch);
    }

    private void AddCharacter(Character ch)
    {
        SetCharacter(ch);
        OnAddCharacter?.Invoke(ch);
    }



    public void SetCharacter(Character ch, bool focus = false)
    {
//        if (CurrentCharacter != null && CurrentCharacter.guid == ch.guid) return;

        CurrentCharacter = ch;
        Characters[ch.Guid] = ch;

        _equipItemService.SetupCharacter(CurrentCharacter);
        CurrentCharacter.GameObjectData.CharacterObject.SetActive(true);


        Inventory.Init(CurrentCharacter);

        if (focus) _followCamera.SetFocus(CurrentCharacter.GameObjectData.CharacterObject.transform, true);

        OnChangeCharacter?.Invoke(CurrentCharacter);
        _characterManageService.SelectCharacter(ch);
    }


    private void CharacterGameObjectClickHandler(RaycastHit hit)
    {
        foreach (var ch in Characters)
        {
            if (ch.Value.GameObjectData.CharacterObject.GetInstanceID() == hit.transform.gameObject.GetInstanceID())
            {
                SetCharacter(ch.Value);
                break;
            }
        }
    }

    private void EnemyGameObjectClickHandler(RaycastHit hit)
    {
        var enemy = GetEnemyByGoId(hit.collider.gameObject.GetInstanceID());
        if (enemy == null) return;

        if (OnEnemyClick != null) OnEnemyClick(CurrentCharacter.Guid, enemy);
    }


    private void NpcGameObjectClickHandler(RaycastHit hit)
    {
        foreach (var npc in _npcPlayerCharacters)
        {
            if (npc.Value.GameObjectData.CharacterObject.GetInstanceID() == hit.transform.gameObject.GetInstanceID())
            {
                ConvertToCharacter(npc.Value);
                break;
            }
        }
    }

    private void ToggleInventoryHandler()
    {
        inventoryPopup.Toggle();
    }

    private void ToggleCharacterInfoHandler()
    {
        characterPopup.Toggle();
    }

    public void OpenContainer(Container container)
    {
        if (container == null) return;

        OpenedContainers[container.Guid] = container;
        ContainerPopup.Init(container);
        ContainerPopup.Open();
    }

  

    private void PickUpObjectClickHandler(RaycastHit gameObjectHit)
    {
        _inputService.UpdateCursor(CursorType.PickUp);

        PlayerMoveController.CurrentCharacterStop();
        PlayerMoveController.LookCurrentCharacterToPoint(gameObjectHit.point);

        if (Helper.IsNear(CurrentCharacter.GameObjectData.CharacterObject.transform.position, gameObjectHit.collider))
        {
            var pickUpItem = gameObjectHit.transform.GetComponent<PickUpItem>();
            if (pickUpItem == null) return;

            Inventory.AddToInventory(pickUpItem);
        }
        else
        {
//            PlayerMoveController.MoveCurrentCharacterToPoint(gameObjectHit.point, false,
//                endMovePoint => { PickUpObjectClickHandler(gameObjectHit); });
//            ContainerPopup.Close();
        }
    }

    private void CurrentCharacterPositionChangedHandler()
    {
        ContainerPopup.Close();
    }


    private void Update()
    {
        // _inputService.Update();
        BattleManager.Update();
        // _gameStateMachine.Update();
    }

    private void ConvertToCharacter(Character character)
    {
        _npcPlayerCharacters.Remove(character.Guid);

        var previewPrefab = Instantiate(character.GameObjectData.Config.PreviewPrefab);
        previewPrefab.transform.position = Vector3.zero;
        previewPrefab.SetActive(false);
        character.GameObjectData.InitPreviewPrefab(previewPrefab);

        AddCharacter(character);
        character.GameObjectData.CharacterObject.layer = Constants.LAYER_CHARACTER;
    }

    private Character GetCharacterByGoId(int goId)
    {
        foreach (var ch in Characters)
        {
            if (ch.Value.GameObjectData.CharacterObject.GetInstanceID() == goId)
                return ch.Value;
        }

        return null;
    }

    public Enemy GetEnemyByGoId(int goId)
    {
        foreach (var ch in Enemies)
        {
            if (ch.Value.GameObjectData.Entity.GetInstanceID() == goId)
                return ch.Value;
        }

        return null;
    }

    public void EnemyVisibleCharacter(Enemy enemy, GameObject charaGameObject)
    {
        var character = GetCharacterByGoId(charaGameObject.GetInstanceID());
        if (character == null) return;

        BattleManager.AddCharacter(character);
        BattleManager.AddEnemy(enemy);
    }

    private void OnChangeMouseRaycastHitHandler(RaycastHit hit)
    {
        var state = CursorType.Default;
        switch (hit.transform.gameObject.layer)
        {
            case Constants.LAYER_PICKUP:
                state = CursorType.Hand;
                break;

            case Constants.LAYER_CHARACTER:
                var currentCharacterGoId = CurrentCharacter.EntityGameObject.GetInstanceID();
                var hitId = hit.transform.gameObject.GetInstanceID();

                if (currentCharacterGoId == hitId) break;

                var character = GetCharacterByGoId(hitId);
                if (character != null)
                {
                    HoverManager.HoverFirends(character);
                }
                else
                {
                    HoverManager.UnHover();
                }
                break;
            case Constants.LAYER_ENEMY:
                var enemy = GetEnemyByGoId(hit.transform.gameObject.GetInstanceID());
                if (enemy != null)
                {
                    HoverManager.HoverEnemies(enemy);
                    state = CursorType.Attack;
                }
                else
                {
                    HoverManager.UnHover();
                }
                break;
            default:
                HoverManager.UnHover();
                break;
        }

        _inputService.UpdateCursor(state);
    }

    private void OnCharactersLoadedHandler(IList<Character> characters)
    {
        foreach (var character in Characters.Values)
            OnRemoveCharacter?.Invoke(character);
        Characters.Clear();

        foreach (var character in characters)
            AddCharacter(character);
    }

    private void OnPlayableNpcLoadedHandler(IList<Character> npcs)
    {
        foreach (var npc in npcs)
            _npcPlayerCharacters[npc.Guid] = npc;
    }

    private void OnEnemiesLoadedHandler(IList<Enemy> enemies)
    {
        foreach (var enemy in enemies)
            Enemies[enemy.Guid] = enemy;
    }

    private void OnLoadDataHandler(SaveData saveData)
    {
        MainCharacterGuid = saveData.mainCharacterGuid;
        SetCharacter(saveData.selectedCharacterGuid);
    }

}
