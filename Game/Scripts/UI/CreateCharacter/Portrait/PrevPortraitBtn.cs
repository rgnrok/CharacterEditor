using CharacterEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class PrevPortraitBtn : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        TextureManager.Instance.OnPrevPortrait();
    }
}
