using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CharacterEditor
{
    public class SaveItem : MonoBehaviour, IPointerClickHandler
    {
        private Text _text;

        public string Text
        {
            get { return _text.text; }
            set { _text.text = value; }
        }

        void Awake()
        {
            _text = GetComponent<Text>();
        }

        public Action<string> ClickHandler;
        public Action DoubleClickHandler;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount > 1)
            {
                if (DoubleClickHandler != null) DoubleClickHandler();
            }
            else
            {
                if (ClickHandler != null) ClickHandler(Text);
            }
        }
    }
}