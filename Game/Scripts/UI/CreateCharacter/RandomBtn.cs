using System;
using System.Collections.Generic;
using System.Linq;
using CharacterEditor.Helpers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CharacterEditor
{
    [Serializable]
    public class TypeMask
    {
        [EnumFlag] public MeshType types;
    }

    /*
     * Mixes the selected textures and meshes.
     * Sets the same color and selected mesh (hair, beard)
     * Enables and disables random skin mesh
     */
    public class RandomBtn : MonoBehaviour, IPointerClickHandler
    {
        [Header("Skinned Mesh settings")]
        [EnumFlag]
        public SkinMeshType skinMeshTypeMask;
        
        [Header("Mesh settings")]
        [EnumFlag]
        public MeshType meshTypeMask;
        public TypeMask[] sameMeshes;

        private MeshType[] _randomMeshTypes;
        private MeshType[][] _sameMesheTypes;

        [Header("Texture settings")]
        [EnumFlag]
        public TextureType textureTypeMask;

        private TextureType[] _randomTextureTypes;
        private TextureType[] _ignoreTextureTypes;

        [Header("Color settings")]
        [EnumFlag]
        public MeshType colorMeshTypeMask;

        [EnumFlag]
        public TextureType colorTextureTypeMask;

        private MeshType[] _sameMeshColorTypes;
        private TextureType[] _sameTextureColorTypes;

        private Renderer[] _longRobeMeshes;
        private Renderer[] _shortRobeMeshes;
        private Renderer[] _cloakMeshes;
        
        private bool _isWaitingTextures;
        private bool _isWaitingMeshes;
        private bool IsProcess => _isWaitingTextures || _isWaitingMeshes;

        private IConfigManager _configManager;
        private MeshManager _meshManager;
        private TextureManager _textureManager;

        private Action _robeCloakVisibleCallback;


        public void Awake()
        {
            _configManager = AllServices.Container.Single<IConfigManager>();
            _textureManager = TextureManager.Instance;
            _meshManager = MeshManager.Instance;
        }

        public void Start()
        {
            PrepareMaskTypes();
            if (_configManager != null) _configManager.OnChangeCharacter += PrepareSkinMeshTypes;
        }

        private void OnDestroy()
        {
            if (_configManager != null)
                _configManager.OnChangeCharacter -= PrepareSkinMeshTypes;

            if (_textureManager != null)
                _textureManager.OnTexturesChanged -= TexturesChangedHandler;

            if (_meshManager != null)
                _meshManager.OnMeshesChanged -= MeshesChangedHandler;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (IsProcess) return;

            RandomizeTextures();
            RandomizeMeshes();
        }

        private void RandomizeTextures()
        {
            if (_randomTextureTypes.Length == 0 || _textureManager == null) return;

            _textureManager.OnTexturesChanged -= TexturesChangedHandler;

            _isWaitingTextures = true;
            _textureManager.LockUpdate(true);
            _textureManager.OnTexturesChanged += TexturesChangedHandler;

            ShuffleSkinMeshes();
            _textureManager.OnRandom(_randomTextureTypes, _sameTextureColorTypes, _ignoreTextureTypes);
        }

        private void RandomizeMeshes()
        {
            if (_randomMeshTypes.Length == 0 || _meshManager == null) return;

            _meshManager.OnMeshesChanged -= MeshesChangedHandler;

            _isWaitingMeshes = true;
            _meshManager.LockUpdate(true);
            _meshManager.OnMeshesChanged += MeshesChangedHandler;

            var sameColor = _sameTextureColorTypes.Length > 0
                ? _textureManager.GetSelectedTextureColor(_sameTextureColorTypes[0])
                : 0;

            _meshManager.OnRandom(_randomMeshTypes, _sameMesheTypes, _sameMeshColorTypes, sameColor);
        }


        private void PrepareMaskTypes()
        {
            // Textures
            _randomTextureTypes = textureTypeMask.FlagToArray<TextureType>();

            //Meshes
            _randomMeshTypes = meshTypeMask.FlagToArray<MeshType>();
            _sameMesheTypes = new MeshType[sameMeshes.Length][];
            for (var i = 0; i < sameMeshes.Length; i++)
                _sameMesheTypes[i] = sameMeshes[i].types.FlagToArray<MeshType>();

            //Colors
            _sameMeshColorTypes = colorMeshTypeMask.FlagToArray<MeshType>();
            _sameTextureColorTypes = colorTextureTypeMask.FlagToArray<TextureType>();
        }

        private void PrepareSkinMeshTypes()
        {
            var configData = _configManager.ConfigData;

            if (((int)skinMeshTypeMask & (int)SkinMeshType.RobeLong) != 0)
                _longRobeMeshes = configData.LongRobeMeshes;

            if (((int)skinMeshTypeMask & (int)SkinMeshType.RobeShort) != 0)
                _shortRobeMeshes = configData.ShortRobeMeshes;

            if (((int)skinMeshTypeMask & (int)SkinMeshType.Cloak) != 0)
                _cloakMeshes = configData.CloakMeshes;
        }

        private void ShuffleSkinMeshes()
        {
            _ignoreTextureTypes = null;
            var showLongRobe = false;
            var showShortRobe = false;

            var showCloak = _randomTextureTypes.Contains(TextureType.Cloak) && (UnityEngine.Random.Range(0, 2) == 1);

            if (_randomTextureTypes.Contains(TextureType.RobeShort) && _randomTextureTypes.Contains(TextureType.RobeShort))
            {
                var rand = UnityEngine.Random.Range(0, 3);
                switch (rand)
                {
                    case 0:
                        showLongRobe = true;
                        _ignoreTextureTypes = new[] { TextureType.RobeShort, TextureType.Pants};
                        break;
                    case 1:
                        showShortRobe = true;
                        _ignoreTextureTypes = new[] { TextureType.RobeLong, TextureType.Pants };
                        break;
                    default:
                        _ignoreTextureTypes = new[] { TextureType.RobeLong, TextureType.RobeShort };
                        break;
                }
            }

            _robeCloakVisibleCallback = () =>
            {
                SetVisible(_cloakMeshes, showCloak);
                SetVisible(_longRobeMeshes, showLongRobe);
                SetVisible(_shortRobeMeshes, showShortRobe);
            };
        }


        private void SetVisible(Renderer[] meshes, bool visible)
        {
            if (meshes == null) return;
            foreach (var mesh in meshes) mesh.gameObject.SetActive(visible);
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
            if (IsProcess) return;
            
            _textureManager.LockUpdate(false);
            _meshManager.LockUpdate(false);

            _robeCloakVisibleCallback?.Invoke();
        }
    }
}