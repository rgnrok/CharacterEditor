using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CharacterEditor
{
    public class SaveCharacterBtn : MonoBehaviour, IPointerClickHandler
    {
        public Action OnClick;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (OnClick != null) OnClick();
        }
    }
}