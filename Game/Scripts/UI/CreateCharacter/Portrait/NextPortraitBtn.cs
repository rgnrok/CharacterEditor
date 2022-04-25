using CharacterEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class NextPortraitBtn : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        TextureManager.Instance.OnNextPortrait();
    }
}
