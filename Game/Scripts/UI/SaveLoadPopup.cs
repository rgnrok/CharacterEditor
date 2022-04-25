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

    [SerializeField] private SaveCharacterBtn saveControll;
    [SerializeField] private LoadCharacterBtn loadControll;
    [SerializeField] private InputField input;
    [SerializeField] private GameObject saveItem;
    [SerializeField] private ScrollRect scrollView;
    private List<SaveItem> _list = new List<SaveItem>();
    private SaveLoadPopupMode _mode;


    public void SetMode(SaveLoadPopupMode mode)
    {
        saveControll.gameObject.SetActive(mode == SaveLoadPopupMode.Save);
        loadControll.gameObject.SetActive(mode == SaveLoadPopupMode.Load);
        input.readOnly = mode == SaveLoadPopupMode.Load;
        _mode = mode;
    }


    void Start()
    {
        saveControll.OnClick = SaveSelected;
        loadControll.OnClick = LoadSelected;
    }

    private void SaveSelected()
    {
        var saveName = input.text.Trim();
        if (saveName == "") return;

        SaveManager.Instance.OnSaveClick(saveName);
        var isExistSave = false;
        foreach (var saveItem in _list)
        {
            if (!saveName.Equals(saveItem.Text)) continue;

            isExistSave = true;
            break;
        }

        if (!isExistSave)
            AddSaveItem(saveName);

        PlayerPrefs.SetString(SaveManager.LOADED_SAVE_KEY, saveName);
    }

    private void LoadSelected()
    {
        PlayerPrefs.SetString(SaveManager.LOADED_SAVE_KEY, input.text.Trim());
        SceneManager.LoadScene("Play_Character_Scene");
    }

    void OnEnable()
    {
        UpdateSaveList();
    }

    void UpdateSaveList()
    {
        var currentScrollCount = scrollView.content.childCount;
        var saves = SaveManager.Instance.GetSaves();
        //Hide old
        for (var i = saves.Length; i < currentScrollCount; i++)
        {
            _list[i].gameObject.SetActive(false);
        }
        //Create new
        for (var i = currentScrollCount; i < saves.Length; i++)
        {
            AddSaveItem(saves[i]);
        }

        //Set texts
        for (var i = 0; i < saves.Length; i++)
        {
            _list[i].Text = saves[i];
        }
    }

    private void AddSaveItem(string saveName)
    {
        var listItem = Instantiate(saveItem, scrollView.content.transform).GetComponent<SaveItem>();
        if (listItem != null)
        {
            listItem.Text = saveName;
            listItem.ClickHandler += OnItemClick;
            listItem.DoubleClickHandler += OnDoubleItemClick;
            _list.Add(listItem);
        }
    }

    void OnItemClick(string text)
    {
        input.text = text;
    }
    void OnDoubleItemClick()
    {
        if (_mode == SaveLoadPopupMode.Save)
            SaveSelected();
        else
            LoadSelected();
        
        Toggle();
    }
}
