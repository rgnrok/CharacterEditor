using System;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterEditor
{
    public abstract class ChangeSkinMeshBtn : TextureTypeMaskSelector
    {
        [SerializeField]
        private SkinMeshType[] activeMeshes;
        [SerializeField]
        private SkinMeshType[] disableMeshes;
      

        protected override void OnClick()
        {
            ChangeMesh();
        }

        protected virtual void ChangeMesh()
        {
            TextureManager.Instance.OnTexturesChanged += TextureChangeHandler;
            ChangeSkinTexture();
        }

        protected abstract void ChangeSkinTexture();

        protected void TextureChangeHandler()
        {
            TextureManager.Instance.OnTexturesChanged -= TextureChangeHandler;

            foreach (var mesh in disableMeshes)
                ActiveMeshes(mesh, false);

            foreach (var mesh in activeMeshes)
                ActiveMeshes(mesh, true);
        }

        private void ActiveMeshes(SkinMeshType type, bool active)
        {
            var configData = _configManager.ConfigData;
            var meshes = new List<SkinnedMeshRenderer>();
            switch (type)
            {
                case SkinMeshType.RobeLong:
                    meshes.AddRange(configData.LongRobeMeshes);
                    break;
                case SkinMeshType.RobeShort:
                    meshes.AddRange(configData.ShortRobeMeshes);
                    break;
                case SkinMeshType.Cloak:
                    meshes.AddRange(configData.CloakMeshes);
                    break;
            }
            
            foreach (SkinnedMeshRenderer mesh in meshes)
                mesh.gameObject.SetActive(active);
        }
    }
}
