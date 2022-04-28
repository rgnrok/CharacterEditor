using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CharacterEditor
{
    public class SaveBtn : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private InputField _input;

        private Popup _popup;

        void Awake()
        {
            _popup = GetComponentInParent<Popup>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            var saveName = _input.text.Trim();
            if (string.IsNullOrEmpty(saveName)) return;

            SaveManager.Instance.Save(saveName);
            _popup.Close();
        }

    }
}