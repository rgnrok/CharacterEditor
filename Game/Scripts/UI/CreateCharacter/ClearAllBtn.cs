using System;
using System.Collections.Generic;
using CharacterEditor.Helpers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CharacterEditor
{
    /*
     * Clears selected textures and meshes.
     */
    public class ClearAllBtn : MonoBehaviour, IPointerClickHandler
    {
        [EnumFlag] public SkinMeshType skinedMeshesMask;

        [EnumFlag] public TextureType textureMask;
        [EnumFlag] public MeshType meshMask;

        private TextureType[] _textures;
        private MeshType[] _meshes;
        private Renderer[] _skinMeshes;
        private IConfigManager _configManager;

        private void Awake()
        {
            _configManager = AllServices.Container.Single<IConfigManager>();
        }

        public void Start()
        {
            _textures = textureMask.FlagToArray<TextureType>();
            _meshes = meshMask.FlagToArray<MeshType>();

            if (_configManager != null)
                _configManager.OnChangeCharacter += PrepareSkinMeshTypes;
        }

        private  void OnDestroy()
        {
            if (_configManager != null)
                _configManager.OnChangeCharacter -= PrepareSkinMeshTypes;
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            if (_textures.Length > 0)
            {
                TextureManager.Instance.OnClear(_textures);
                TextureManager.Instance.OnTexturesChanged += ResetMeshes;
            }
            else
            {
                ResetMeshes();
            }
        }

        private void ResetMeshes()
        {
            TextureManager.Instance.OnTexturesChanged -= ResetMeshes;

            MeshManager.Instance.OnClearMesh(_meshes);
            foreach (var mesh in _skinMeshes)
            {
                mesh.gameObject.SetActive(false);
            }
        }

        private void PrepareSkinMeshTypes()
        {
            var configData = _configManager.ConfigData;

            var list = new List<Renderer>();
            foreach (var enumValue in Enum.GetValues(typeof(SkinMeshType)))
            {
                int checkBit = (int) skinedMeshesMask & (int) enumValue;
                if (checkBit != 0)
                {
                    switch ((SkinMeshType) enumValue)
                    {
                        case SkinMeshType.RobeLong:
                            list.AddRange(configData.LongRobeMeshes);
                            break;
                        case SkinMeshType.RobeShort:
                            list.AddRange(configData.ShortRobeMeshes);
                            break;
                        case SkinMeshType.Cloak:
                            list.AddRange(configData.CloakMeshes);
                            break;
                    }
                }
            }
            _skinMeshes = list.ToArray();
        }
    }
}