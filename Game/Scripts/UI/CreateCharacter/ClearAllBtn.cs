using System;
using System.Collections.Generic;
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

        private TextureType[] textures;
        private MeshType[] meshes;
        private Renderer[] skinMeshes;
        private IConfigManager _configManager;

        private void Awake()
        {
            _configManager = AllServices.Container.Single<IConfigManager>();
        }

        public void Start()
        {
            PrepareTextureTypes();
            PrepareMeshTypes();

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
            if (textures.Length > 0)
            {
                TextureManager.Instance.OnClear(textures);
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

            MeshManager.Instance.OnClearMesh(meshes);
            foreach (var mesh in skinMeshes)
            {
                mesh.gameObject.SetActive(false);
            }
        }

        private void PrepareTextureTypes()
        {
            var list = new List<TextureType>();
            foreach (var enumValue in Enum.GetValues(typeof(TextureType)))
            {
                int checkBit = (int) textureMask & (int) enumValue;
                if (checkBit != 0)
                    list.Add((TextureType) enumValue);
            }
            textures = list.ToArray();
        }


        private void PrepareMeshTypes()
        {
            var list = new List<MeshType>();
            foreach (var enumValue in Enum.GetValues(typeof(MeshType)))
            {
                int checkBit = (int) meshMask & (int) enumValue;
                if (checkBit != 0)
                    list.Add((MeshType) enumValue);
            }
            meshes = list.ToArray();
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
            skinMeshes = list.ToArray();
        }
    }
}