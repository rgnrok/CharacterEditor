using System;
using UnityEngine.EventSystems;

namespace CharacterEditor
{
    public class NextMeshColorBtn : MeshAndTextureTypeMaskSelector, IPointerClickHandler
    {
        private Action updateMeshCalback;

        private bool _isWaitingTextures;
        private bool _isWaitingMeshes;


        public void OnPointerClick(PointerEventData eventData)
        {
            TextureManager.Instance.OnTexturesChanged -= TexturesChangedHandler;
            MeshManager.Instance.OnMeshesChanged -= MeshesChangedHandler;

            if (textureTypes.Length > 0)
            {
                _isWaitingTextures = true;
                TextureManager.Instance.LockUpdate(true);
                TextureManager.Instance.OnTexturesChanged += TexturesChangedHandler;

                TextureManager.Instance.OnNextColor(textureTypes);
            }

            if (meshTypes.Length > 0)
            {
                _isWaitingMeshes = true;
                MeshManager.Instance.LockUpdate(true);
                MeshManager.Instance.OnMeshesChanged += MeshesChangedHandler;
                
                if (textureTypes.Length > 0)
                {
                    MeshManager.Instance.SetMeshColor(meshTypes, TextureManager.Instance.CurrentCharacterTextures[textureTypes[0]].SelectedColor);
                }
                else
                {
                    MeshManager.Instance.OnNextColor(meshTypes);
                }
            }
        }

        private void TexturesChangedHandler()
        {
            if (TextureManager.Instance == null) return;

            TextureManager.Instance.OnTexturesChanged -= TexturesChangedHandler;
            _isWaitingTextures = false;
            if (!_isWaitingMeshes) UpdateMeshesAndTextures();
        }

        private void MeshesChangedHandler()
        {
            if (MeshManager.Instance == null) return;

            MeshManager.Instance.OnMeshesChanged -= MeshesChangedHandler;
            _isWaitingMeshes = false;
            if (!_isWaitingTextures) UpdateMeshesAndTextures();
        }

        private void UpdateMeshesAndTextures()
        {
            TextureManager.Instance.LockUpdate(false);
            MeshManager.Instance.LockUpdate(false);
        }
    }
}