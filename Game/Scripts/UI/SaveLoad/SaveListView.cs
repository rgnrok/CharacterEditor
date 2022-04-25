using System;
using System.Collections;
using System.Collections.Generic;
using CharacterEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CharacterEditor
{
    [Obsolete("Not used any more", true)]
    public class SaveListView : MonoBehaviour
    {
        [SerializeField]
        private SaveItem saveItem;

        private ScrollRect scrollView;
        private List<SaveItem> list =  new List<SaveItem>();

        public SaveItem SelectedItem { get; private set; }
        public EventHandler ChangeSelectedItemHandler;


        void Awake()
        {
            scrollView = GetComponent<ScrollRect>();
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
            for (int i = saves.Length; i < currentScrollCount; i++)
            {
                list[i].gameObject.SetActive(false);
                list[i].ClickHandler -= OnItemClick;
            }
            //Create new
            for (int i = currentScrollCount; i < saves.Length; i++)
            {
                SaveItem listItem = GameObject.Instantiate(saveItem, scrollView.content.transform);
                list.Add(listItem);
            }
            //Set text
            for (int i = 0; i < saves.Length; i++) {
                list[i].Text = saves[i];
                list[i].ClickHandler += OnItemClick;
            }
        }

        void OnItemClick(string text)
        {
            SelectedItem.Text = text;
            if (ChangeSelectedItemHandler != null)
            {
                ChangeSelectedItemHandler(this, null);
            }
        }
    }
}