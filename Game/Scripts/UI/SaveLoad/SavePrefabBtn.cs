using CharacterEditor.Services;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CharacterEditor
{
    public class SavePrefabBtn : MonoBehaviour, IPointerClickHandler
    {
        private void Start()
        {
            var visibleBtn = false;
#if UNITY_EDITOR
            visibleBtn = AllServices.Container.Single<IStaticDataService>().LoaderType == LoaderType.AssetDatabase;
#endif
            transform.gameObject.SetActive(visibleBtn);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            SaveManager.Instance.OnSavePrefabClick();
        }

    }
}
