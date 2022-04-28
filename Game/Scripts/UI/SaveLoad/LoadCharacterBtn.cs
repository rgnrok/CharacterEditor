using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class LoadCharacterBtn : MonoBehaviour, IPointerClickHandler
{
    public Action OnClick;

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick?.Invoke();
    }
}
