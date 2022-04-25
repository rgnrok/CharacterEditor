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
     * Sets the same color and selected tmesha (hair, beard)
     * Enables and disables random skin mesh
     */
    public class RandomBtn : MonoBehaviour, IPointerClickHandler
    {
        [Header("Skinned Mesh settings")] [EnumFlag]
        public SkinMeshType skinMeshTypeMask;

        private SkinnedMeshRenderer[] longRobeMeshes;
        private SkinnedMeshRenderer[] shortRobeMeshes;
        private SkinnedMeshRenderer[] cloakMeshes;

        [Header("Mesh settings")] [EnumFlag] public MeshType meshTypeMask;
        public TypeMask[] sameMeshes;

        private MeshType[] randomMesheTypes;
        private MeshType[][] sameMesheTypes;

        [Header("Texture settings")] [EnumFlag]
        public TextureType textureTypeMask;

        private TextureType[] randomTextureTypes;
        private TextureType[] ignoreTextureTypes;

        [Header("Color settings")] [EnumFlag] public MeshType colorMeshTypeMask;
        [EnumFlag] public TextureType colorTextureTypeMask;

        private MeshType[] sameMeshColorTypes;
        private TextureType[] sameTextureColorTypes;

        private Action visibleCalback;

        private bool _isWaitingTextures;
        private bool _isWaitingMeshes;
        private bool _isProcess;
        private ConfigManager _configManager;

        void Awake()
        {
            _configManager = AllServices.Container.Single<ConfigManager>();
        }

        public void Start()
        {
            PrepareTextureTypes();
            PrepareMeshTypes();
            PrepareSameColorTypes();

            if (_configManager != null) _configManager.OnChangeCharacter += PrepareSkinMeshTypes;
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isProcess || TextureManager.Instance == null || MeshManager.Instance == null) return;
            _isProcess = true;

            TextureManager.Instance.OnTexturesLoaded -= TexturesChangedHandler;
            MeshManager.Instance.OnMeshesLoaded -= MeshesChangedHandler;
            _isWaitingMeshes = false;
            _isWaitingTextures = false;

            if (randomTextureTypes.Length != 0)
            {
                _isWaitingTextures = true;
                TextureManager.Instance.LockUpdate(true);
                TextureManager.Instance.OnTexturesChanged += TexturesChangedHandler;

                ShuffleSkinendMesh();
                TextureManager.Instance.OnRandom(randomTextureTypes, sameTextureColorTypes, ignoreTextureTypes);
            }

            if (randomMesheTypes.Length != 0)
            {
                _isWaitingMeshes = true;
                MeshManager.Instance.LockUpdate(true);
                MeshManager.Instance.OnMeshesChanged += MeshesChangedHandler;

                var sameColor = sameTextureColorTypes.Length > 0
                    ? TextureManager.Instance.CurrentCharacterTextures[sameTextureColorTypes[0]].SelectedColor
                    : 0;

                MeshManager.Instance.OnRandom(randomMesheTypes, sameMesheTypes, sameMeshColorTypes, sameColor);
            }
        }

        private void PrepareTextureTypes()
        {
            var list = new List<TextureType>();
            foreach (var enumValue in System.Enum.GetValues(typeof(TextureType)))
            {
                var checkBit = (int) textureTypeMask & (int) enumValue;
                if (checkBit != 0)
                    list.Add((TextureType) enumValue);
            }
            randomTextureTypes = list.ToArray();
        }

        private void PrepareSameColorTypes()
        {
            var meshList = new List<MeshType>();
            foreach (var enumValue in Enum.GetValues(typeof(MeshType)))
            {
                var checkBit = (int) colorMeshTypeMask & (int) enumValue;
                if (checkBit != 0)
                    meshList.Add((MeshType) enumValue);
            }
            sameMeshColorTypes = meshList.ToArray();

            var textureList = new List<TextureType>();
            foreach (var enumValue in Enum.GetValues(typeof(TextureType)))
            {
                var checkBit = (int) colorTextureTypeMask & (int) enumValue;
                if (checkBit != 0)
                    textureList.Add((TextureType) enumValue);
            }

            sameTextureColorTypes = textureList.ToArray();
        }

        private void PrepareMeshTypes()
        {
            var list = new List<MeshType>();
            foreach (var enumValue in Enum.GetValues(typeof(MeshType)))
            {
                int checkBit = (int) meshTypeMask & (int) enumValue;
                if (checkBit != 0)
                    list.Add((MeshType) enumValue);
            }

            randomMesheTypes = list.ToArray();

            var sameList = new List<MeshType>();
            sameMesheTypes = new MeshType[sameMeshes.Length][];
            for (int i = 0; i < sameMeshes.Length; i++)
            {
                sameList.Clear();
                foreach (var enumValue in Enum.GetValues(typeof(MeshType)))
                {
                    int checkBit = (int) sameMeshes[i].types & (int) enumValue;
                    if (checkBit != 0)
                        sameList.Add((MeshType) enumValue);
                }

                sameMesheTypes[i] = sameList.ToArray();
            }
        }

        private void PrepareSkinMeshTypes()
        {
            var configData = _configManager.ConfigData;
            foreach (var enumValue in Enum.GetValues(typeof(SkinMeshType)))
            {
                int checkBit = (int) skinMeshTypeMask & (int) enumValue;
                if (checkBit != 0)
                {
                    switch ((SkinMeshType) enumValue)
                    {
                        case SkinMeshType.RobeLong:
                            longRobeMeshes = configData.LongRobeMeshes;
                            break;
                        case SkinMeshType.RobeShort:
                            shortRobeMeshes = configData.ShortRobeMeshes;
                            break;
                        case SkinMeshType.Cloak:
                            cloakMeshes = configData.CloakMeshes;
                            break;
                    }
                }
            }
        }

        private void ShuffleSkinendMesh()
        {
            ignoreTextureTypes = null;

            bool showCloak = randomTextureTypes.Contains(TextureType.Cloak) && (UnityEngine.Random.Range(0, 2) == 1);

            if (randomTextureTypes.Contains(TextureType.Pants))
            {
                int rand = UnityEngine.Random.Range(0, 3);
                switch (rand)
                {
                    case 0:
                        visibleCalback = () =>
                        {
                            SetVisible(cloakMeshes, showCloak);
                            SetVisible(longRobeMeshes, true);
                            SetVisible(shortRobeMeshes, false);
                        };
                        ignoreTextureTypes = new TextureType[1] {TextureType.RobeShort};
                        break;
                    case 1:
                        visibleCalback = () =>
                        {
                            SetVisible(cloakMeshes, showCloak);
                            SetVisible(longRobeMeshes, false);
                            SetVisible(shortRobeMeshes, true);
                        };
                        ignoreTextureTypes = new TextureType[1] {TextureType.RobeLong};

                        break;
                    default:
                        visibleCalback = () =>
                        {
                            SetVisible(cloakMeshes, showCloak);
                            SetVisible(longRobeMeshes, false);
                            SetVisible(shortRobeMeshes, false);
                        };
                        ignoreTextureTypes = new TextureType[2] {TextureType.RobeLong, TextureType.RobeShort};
                        break;
                }
            }
        }

        private void SetVisible(SkinnedMeshRenderer[] meshes, bool visible)
        {
            if (meshes == null)
                return;

            for (int i = 0; i < meshes.Length; i++)
            {
                meshes[i].gameObject.SetActive(visible);
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
            _isProcess = false;

            if (visibleCalback != null) visibleCalback.Invoke();
        }
    }
}