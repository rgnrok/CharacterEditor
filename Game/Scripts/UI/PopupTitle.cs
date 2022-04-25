
using UnityEngine;
using UnityEngine.EventSystems;

public class PopupTitle : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    private Transform popup;

    private Vector3 dragOffset;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (popup == null) popup = transform.parent;
        dragOffset = popup.transform.position - Input.mousePosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        popup.transform.position = Input.mousePosition + dragOffset;
    }
}
