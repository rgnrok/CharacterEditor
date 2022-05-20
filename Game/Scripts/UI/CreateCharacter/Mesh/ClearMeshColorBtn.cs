using UnityEngine.EventSystems;

namespace CharacterEditor
{
    public class ClearMeshColorBtn : MeshAndTextureChangedBtn
    {
        protected override void ChangeTexture()
        {
            _textureManager.OnResetColor(textureTypes);
        }

        protected override void ChangeMesh()
        {
            _meshManager.OnClearMeshColor(meshTypes);
        }
    }
}