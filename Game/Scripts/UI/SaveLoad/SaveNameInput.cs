using System;
using UnityEngine;
using UnityEngine.UI;

namespace CharacterEditor
{
    [Obsolete("Not used any more", true)]
    public class SaveNameInput : MonoBehaviour
    {

        private InputField input;

        void Awake()
        {
            input = GetComponent<InputField>();
            var popup = GetComponentInParent<Popup>();
            SaveListView saveList = popup.gameObject.GetComponentInChildren<SaveListView>();
            saveList.ChangeSelectedItemHandler += OnChangeSaveName;
        }

        void OnChangeSaveName(object sender, EventArgs e)
        {
            input.text = (sender as SaveListView).SelectedItem.Text;
        }
    }
}
