using CharacterEditor.Helpers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CharacterEditor
{
    public abstract class MeshAndTextureChangedBtn : MonoBehaviour, IPointerClickHandler
    {
        [EnumFlag]
        public MeshType meshTypeMask;
        protected MeshType[] meshTypes;

        [EnumFlag]
        public TextureType textureTypeMask;
        protected TextureType[] textureTypes;

        private bool _isWaitingTextures;
        private bool _isWaitingMeshes;

        protected TextureManager _textureManager;
        protected MeshManager _meshManager;

        protected abstract void ChangeTexture();
        protected abstract void ChangeMesh();

        private void Start()
        {
            meshTypes = meshTypeMask.FlagToArray<MeshType>();
            textureTypes = textureTypeMask.FlagToArray<TextureType>();

            _textureManager = TextureManager.Instance;
            _meshManager = MeshManager.Instance;
        }

        private void OnDestroy()
        {
            if (_textureManager != null)
                _textureManager.OnTexturesChanged -= TexturesChangedHandler;
            if (_meshManager != null)
                _meshManager.OnMeshesChanged -= MeshesChangedHandler;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnChangeTexture();
            OnChangeMesh();
        }

        private void OnChangeTexture()
        {
            if (textureTypes.Length <= 0) return;

            _textureManager.OnTexturesChanged -= TexturesChangedHandler;

            _isWaitingTextures = true;
            _textureManager.LockUpdate(true);
            _textureManager.OnTexturesChanged += TexturesChangedHandler;

            ChangeTexture();
        }

        private void OnChangeMesh()
        {
            if (meshTypes.Length <= 0) return;

            _meshManager.OnMeshesChanged -= MeshesChangedHandler;

            _isWaitingMeshes = true;
            _meshManager.LockUpdate(true);
            _meshManager.OnMeshesChanged += MeshesChangedHandler;

            ChangeMesh();
        }

        private void TexturesChangedHandler()
        {
            if (_textureManager == null) return;

            _textureManager.OnTexturesChanged -= TexturesChangedHandler;
            _isWaitingTextures = false;

            UpdateMeshesAndTextures();
        }

        private void MeshesChangedHandler()
        {
            if (_meshManager == null) return;

            _meshManager.OnMeshesChanged -= MeshesChangedHandler;
            _isWaitingMeshes = false;

            UpdateMeshesAndTextures();
        }

        private void UpdateMeshesAndTextures()
        {
            if (_isWaitingMeshes || _isWaitingTextures) return;

            _textureManager.LockUpdate(false);
            _meshManager.LockUpdate(false);
        }
    }
}