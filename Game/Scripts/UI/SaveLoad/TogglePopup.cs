using CharacterEditor.Services;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CharacterEditor
{
    public class TogglePopup : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private SaveLoadPopup.SaveLoadPopupMode mode;

        private void Start()
        {
            if (AllServices.Container.Single<IStaticDataService>().LoaderType != LoaderType.AssetBundle) //todo
                transform.gameObject.SetActive(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            SaveManager.Instance.TogglePopup(mode);
        }
    }
}