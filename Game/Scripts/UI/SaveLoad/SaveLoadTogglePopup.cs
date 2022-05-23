using CharacterEditor.Services;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CharacterEditor
{
    public class SaveLoadTogglePopup : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private SaveLoadPopup.SaveLoadPopupMode mode;

        private void Start()
        {
            var visibleBtn = true;
#if UNITY_EDITOR
            visibleBtn = AllServices.Container.Single<IStaticDataService>().LoaderType != LoaderType.AssetDatabase;
#endif
            transform.gameObject.SetActive(visibleBtn);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            SaveManager.Instance.TogglePopup(mode);
        }
    }
}