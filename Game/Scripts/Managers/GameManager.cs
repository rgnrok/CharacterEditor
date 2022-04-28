using System;
using System.Collections.Generic;
using CharacterEditor;
using CharacterEditor.CharacterInventory;
using EnemySystem;
using Game;
using UnityEngine;
using UnityEngine.Profiling;

public class GameManager : MonoBehaviour, ICoroutineRunner
{
    public static GameManager Instance { get; private set; }
    public InputManager InputManager { get; private set; }
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
    public Action<string> OnRemoveCharacter;

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

    #region SpawnPoints

    [SerializeField] private PlayerCharacterSpawnPoint[] _playerCharacterSpawnPoints;
    public PlayerCharacterSpawnPoint[] PlayerCharacterSpawnPoints { get { return _playerCharacterSpawnPoints ?? new PlayerCharacterSpawnPoint[0]; } }

    [SerializeField] private EnemySpawnPoint[] _enemySpawnPoints;
    public EnemySpawnPoint[] EnemySpawnPoints { get { return _enemySpawnPoints ?? new EnemySpawnPoint[0]; } }

    [SerializeField] private ContainerSpawnPoint[] _containerSpawnPoints;
    public ContainerSpawnPoint[] ContainerSpawnPoints { get { return _containerSpawnPoints ?? new ContainerSpawnPoint[0]; } }

    #endregion

    // private GameStateMachine _gameStateMachine;


    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        Instance = this;
        BattleManager = new BattleManager();

        InputManager = new InputManager();
        InputManager.CharacterGameObjectClick += CharacterGameObjectClickHandler;
        InputManager.EnemyGameObjectClick += EnemyGameObjectClickHandler;
        InputManager.NpcGameObjectClick += NpcGameObjectClickHandler;
        InputManager.ToggleInventory += ToggleInventoryHandler;
        InputManager.ToggleCharacterInfo += ToggleCharacterInfoHandler;
        InputManager.ContainerGameObjectClick += ContainerGameObjectClickHandler;
        InputManager.PickUpObjectClick += PickUpObjectClickHandler;
        InputManager.OnChangeMouseRaycasHit += OnChangeMouseRaycastHitHandler;

        PlayerMoveController = GetComponent<PlayerMoveController>();
        RenderPathController = GetComponent<RenderPathController>();

        Characters = new Dictionary<string, Character>();
        Enemies = new Dictionary<string, Enemy>();

        HoverManager = GetComponent<HoverManager>();
        _followCamera = Camera.main.GetComponent<FollowCamera>();
        // _gameStateMachine = new GameStateMachine(new SceneLoader(this), AllServices.Container);

    }

    private void Start()
    {
        Canvas = GameObject.Find("Canvas").transform;
        PlayerMoveController.CurrentCharacterPositionChanged += CurrentCharacterPositionChangedHandler;
        // _gameStateMachine.Start();
    }


    public void SetNpcPlayerCharacter(Character ch)
    {
        _npcPlayerCharacters[ch.guid] = ch;
    }

    public void SetCharacter(string guid)
    {
        if (Characters.ContainsKey(guid)) 
            SetCharacter(Characters[guid]);
    }


    public void AddCharacter(Character ch)
    {
        SetCharacter(ch);
        if (OnAddCharacter != null) OnAddCharacter(ch);
    }

    public void AddEnemy(Enemy enemy)
    {
        Enemies[enemy.guid] = enemy;
    }

    public void SetCharacter(Character ch, bool focus = false)
    {
//        if (CurrentCharacter != null && CurrentCharacter.guid == ch.guid) return;

        CurrentCharacter = ch;
        Characters[ch.guid] = ch;

        ItemManager.Instance.SetCharacter(CurrentCharacter);
        CurrentCharacter.GameObjectData.CharacterObject.SetActive(true);


        Inventory.Init(CurrentCharacter);

        if (focus) _followCamera.SetFocus(CurrentCharacter.GameObjectData.CharacterObject.transform, true);

        OnChangeCharacter?.Invoke(CurrentCharacter);
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

        if (OnEnemyClick != null) OnEnemyClick(CurrentCharacter.guid, enemy);
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

    private void ContainerGameObjectClickHandler(RaycastHit containerHit)
    {
        PlayerMoveController.CurrentCharacterStop();
        PlayerMoveController.LookCurrentCharacterToPoint(containerHit.point);

        if (Helper.IsNear(CurrentCharacter.GameObjectData.CharacterObject.transform.position, containerHit.collider))
        {
            var container = containerHit.transform.GetComponent<Container>();
            if (container == null) return;

            OpenedContainers[container.Guid] = container;
            ContainerPopup.Init(container);
            ContainerPopup.Open();
        }
        else
        {
//            PlayerMoveController.MoveCurrentCharacterToPoint(containerHit.point, false,
//                endMovePoint => { ContainerGameObjectClickHandler(containerHit); });
//            ContainerPopup.Close();
        }
    }

    private void PickUpObjectClickHandler(RaycastHit gameObjectHit)
    {
        InputManager.UpdateCursor(InputManager.CursorState.PickUp);

        PlayerMoveController.CurrentCharacterStop();
        PlayerMoveController.LookCurrentCharacterToPoint(gameObjectHit.point);

        if (Helper.IsNear(CurrentCharacter.GameObjectData.CharacterObject.transform.position, gameObjectHit.collider))
        {
            var pickUpItem = gameObjectHit.transform.GetComponent<PickUpItem>();
            if (pickUpItem == null) return;

            Inventory.AddToInvetory(pickUpItem);
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
        
        
        Profiler.BeginSample("GM Update");
        InputManager.Update();
        BattleManager.Update();
        // _gameStateMachine.Update();
        Profiler.EndSample();
    }

    private void ConvertToCharacter(Character character)
    {
        _npcPlayerCharacters.Remove(character.guid);

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
        var state = InputManager.CursorState.Default;
        switch (hit.transform.gameObject.layer)
        {
            case Constants.LAYER_PICKUP:
                state = InputManager.CursorState.Hand;
                break;

            case Constants.LAYER_CHARACTER:
                var currentCharacterGoId = CurrentCharacter.EntityGameObject.GetInstanceID();
                var hitId = hit.transform.gameObject.GetInstanceID();

                if (currentCharacterGoId == hitId) break;

                var character = GetCharacterByGoId(hitId);
                if (character != null)
                {
                    HoverManager.HoverFirends(character);
                    state = InputManager.CursorState.Speak;
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
                    state = InputManager.CursorState.Attack;
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

        InputManager.UpdateCursor(state);
    }
}
