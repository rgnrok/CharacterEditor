using UnityEngine;
using UnityEngine.EventSystems;

namespace CharacterEditor
{
    public class ClosePopup : MonoBehaviour, IPointerClickHandler
    {

        private Popup popup;

        void Awake()
        {
            popup = GetComponentInParent<Popup>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            popup.gameObject.SetActive(false);
        }
    }
}
