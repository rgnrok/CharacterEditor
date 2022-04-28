using System.Collections.Generic;
using CharacterEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveLoadPopup : Popup
{
    public enum SaveLoadPopupMode
    {
        Save,
        Load
    }

    [SerializeField] private SaveCharacterBtn _saveControll;
    [SerializeField] private LoadCharacterBtn _loadControll;
    [SerializeField] private InputField _input;
    [SerializeField] private GameObject _saveItem;
    [SerializeField] private ScrollRect _scrollView;

    private readonly List<SaveItem> _list = new List<SaveItem>();
    private SaveLoadPopupMode _mode;
    private SaveManager _saveManager;

    private void Awake()
    {
        _saveManager = SaveManager.Instance;
    }

    private void Start()
    {
        _saveControll.OnClick = OnSaveHandler;
        _loadControll.OnClick = OnLoadHandler;
    }

    public void SetMode(SaveLoadPopupMode mode)
    {
        _saveControll.gameObject.SetActive(mode == SaveLoadPopupMode.Save);
        _loadControll.gameObject.SetActive(mode == SaveLoadPopupMode.Load);
        _input.readOnly = mode == SaveLoadPopupMode.Load;
        _mode = mode;
    }

    private void OnSaveHandler()
    {
        var saveName = _input.text.Trim();
        if (string.IsNullOrEmpty(saveName)) return;

        _saveManager.Save(saveName);

        var isExistSave = false;
        foreach (var saveItem in _list)
        {
            if (!saveName.Equals(saveItem.Text)) continue;

            isExistSave = true;
            break;
        }

        if (!isExistSave)
            AddSaveItem(saveName);

    }

    private void OnLoadHandler()
    {
        var saveName = _input.text.Trim();
        if (string.IsNullOrEmpty(saveName)) return;

        _saveManager.Load(saveName);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        UpdateSaveList();
    }

    void UpdateSaveList()
    {
        var currentScrollCount = _scrollView.content.childCount;
        var saves = _saveManager.GetSaves();
        
        for (var i = saves.Length; i < currentScrollCount; i++)
            _list[i].gameObject.SetActive(false);
        
        for (var i = currentScrollCount; i < saves.Length; i++)
            AddSaveItem(saves[i]);

        for (var i = 0; i < saves.Length; i++)
        {
            _list[i].Text = saves[i];
            _list[i].gameObject.SetActive(true);
        }
    }

    private void AddSaveItem(string saveName)
    {
        var listItem = Instantiate(_saveItem, _scrollView.content.transform).GetComponent<SaveItem>();
        if (listItem == null) return;

        listItem.Text = saveName;
        listItem.ClickHandler += OnItemClick;
        listItem.DoubleClickHandler += OnDoubleItemClick;
        _list.Add(listItem);
    }

    private void OnItemClick(string text)
    {
        _input.text = text;
    }

    private void OnDoubleItemClick()
    {
        Toggle();

        if (_mode == SaveLoadPopupMode.Save)
            OnSaveHandler();
        else
            OnLoadHandler();
    }
}
