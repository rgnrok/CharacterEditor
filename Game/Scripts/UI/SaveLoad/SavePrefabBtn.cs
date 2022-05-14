using CharacterEditor.Services;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CharacterEditor
{
    public class SavePrefabBtn : MonoBehaviour, IPointerClickHandler
    {
        private void Start()
        {
            var staticDataService = AllServices.Container.Single<IStaticDataService>();

            if (staticDataService.LoaderType == LoaderType.AssetBundle)
                transform.gameObject.SetActive(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            SaveManager.Instance.OnSavePrefabClick();
        }

    }
}
