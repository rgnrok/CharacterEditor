using UnityEngine;
using UnityEngine.EventSystems;

namespace CharacterEditor
{
    public class SavePrefabBtn : MonoBehaviour, IPointerClickHandler
    {
        private void Start()
        {
//            if (LoaderManager.Instance.Type == LoaderType.AssetBundle)
//                transform.parent.gameObject.SetActive(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            SaveManager.Instance.OnSavePrefabClick();
        }

    }
}
