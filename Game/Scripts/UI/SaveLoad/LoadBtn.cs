using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CharacterEditor
{
    [Obsolete("Not used any more", true)]
    public class LoadBtn : MonoBehaviour, IPointerClickHandler
    {
        private SaveListView saveList;
        private Popup popup;

        void Awake() {
            popup = GetComponentInParent<Popup>();
            saveList = popup.gameObject.GetComponentInChildren<SaveListView>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            var saveName = saveList.SelectedItem != null ? saveList.SelectedItem.Text.Trim() : "";
            if (!saveName.Equals("")) {
                SaveManager.Instance.OnLoadClick(saveName);
                popup.Close();
            }
        }
    }
}