using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CharacterEditor
{
    public class SaveBtn : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private InputField input;

        private Popup popup;

        void Awake() {
            popup = GetComponentInParent<Popup>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            var saveName = input.text.Trim();
            if (!saveName.Equals(""))
            {
                SaveManager.Instance.OnSaveClick(saveName);
                popup.Close();
            }
        }

    }
}