using System;
using System.Collections.Generic;
using System.Linq;
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

        private SkinnedMeshRenderer[] _longRobeMeshes;
        private SkinnedMeshRenderer[] _shortRobeMeshes;
        private SkinnedMeshRenderer[] _cloakMeshes;

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

        private MeshType[] sameMeshColorTypes;
        private TextureType[] sameTextureColorTypes;

        private Action _robeCloakVisibleCallback;

        private bool _isWaitingTextures;
        private bool _isWaitingMeshes;
        private bool _isProcess;

        private IConfigManager _configManager;
        private MeshManager _meshManager;
        private TextureManager _textureManager;

        public RandomBtn(MeshType[] randomMeshTypes)
        {
            _randomMeshTypes = randomMeshTypes;
        }

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

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isProcess || _textureManager == null || _meshManager == null) return;
            _isProcess = true;

            _textureManager.OnTexturesLoaded -= TexturesChangedHandler;
            _meshManager.OnMeshesLoaded -= MeshesChangedHandler;

            RandomizeTextures();
            RandomizeMeshes();
        }

        private void RandomizeMeshes()
        {
            _isWaitingMeshes = false;
            if (_randomMeshTypes.Length == 0) return;

            _isWaitingMeshes = true;
            _meshManager.LockUpdate(true);
            _meshManager.OnMeshesChanged += MeshesChangedHandler;

            var sameColor = sameTextureColorTypes.Length > 0
                ? _textureManager.CurrentCharacterTextures[sameTextureColorTypes[0]].SelectedColor
                : 0;

            _meshManager.OnRandom(_randomMeshTypes, _sameMesheTypes, sameMeshColorTypes, sameColor);
        }

        private void RandomizeTextures()
        {
            _isWaitingTextures = false;
            if (_randomTextureTypes.Length == 0) return;

            _isWaitingTextures = true;
            _textureManager.LockUpdate(true);
            _textureManager.OnTexturesChanged += TexturesChangedHandler;

            ShuffleSkinMeshes();
            _textureManager.OnRandom(_randomTextureTypes, sameTextureColorTypes, _ignoreTextureTypes);
        }

        private void PrepareTypeMask<T>(int mask, out T[] randomType)
        {
            var list = new List<T>();
            foreach (var enumValue in Enum.GetValues(typeof(T)))
            {
                var checkBit = mask & (int)enumValue;
                if (checkBit != 0)
                    list.Add((T)enumValue);
            }
            randomType = list.ToArray();
        }

        private void PrepareMaskTypes()
        {
            // Textures
            PrepareTypeMask((int)textureTypeMask, out _randomTextureTypes);

            //Meshes
            PrepareTypeMask((int)meshTypeMask, out _randomMeshTypes);
            _sameMesheTypes = new MeshType[sameMeshes.Length][];
            for (var i = 0; i < sameMeshes.Length; i++)
                PrepareTypeMask((int) sameMeshes[i].types, out _sameMesheTypes[i]);

            //Colors
            PrepareTypeMask((int)colorMeshTypeMask, out sameMeshColorTypes);
            PrepareTypeMask((int)colorTextureTypeMask, out sameTextureColorTypes);
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


        private void SetVisible(SkinnedMeshRenderer[] meshes, bool visible)
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
            if (_isWaitingMeshes || _isWaitingTextures) return;
            
            _textureManager.LockUpdate(false);
            _meshManager.LockUpdate(false);
            _isProcess = false;

            if (_robeCloakVisibleCallback != null) _robeCloakVisibleCallback.Invoke();
        }
    }
}