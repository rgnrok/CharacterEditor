using UnityEngine.EventSystems;

namespace CharacterEditor
{
    public class ClearMeshColorBtn : MeshAndTextureTypeMaskSelector, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            if (textureTypes.Length > 0) {
                TextureManager.Instance.OnResetColor(textureTypes);
            }
            if (meshTypes.Length > 0) {
                MeshManager.Instance.OnClearMeshColor(meshTypes);
            }
        }
    }
}