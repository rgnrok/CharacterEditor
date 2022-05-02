using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace CharacterEditor.Services
{
    public class MergeTextureService
    {
        private Texture2D _emptyAlphaTexture;

        public MergeTextureService()
        {
            CreateEmptyTexture();
        }

        private void CreateEmptyTexture()
        {
            _emptyAlphaTexture = new Texture2D(1, 1);
            _emptyAlphaTexture.SetPixel(0, 0, Color.clear);
            _emptyAlphaTexture.Apply();
        }

        /*
         * Combining the texture of the character
         */
        public async Task<RenderTexture> MergeTextures(Material skinRenderShaderMaterial, RenderTexture renderSkinTexture, Dictionary<TextureType, CharacterTexture> textures, TextureType[] ignoreTypes )
        {
            // Clear uniquie textures for each character
            skinRenderShaderMaterial.SetTexture("_BeardTex", _emptyAlphaTexture);
            skinRenderShaderMaterial.SetTexture("_FaceFeatureTex", _emptyAlphaTexture);

            foreach (var texture in textures.Values)
            {
                if (texture.Type == TextureType.Cloak) continue;
                while (!texture.IsReady) await Task.Yield();

                var textureName = texture.GetShaderTextureName();
                if (textureName == null) continue;

                var isIgnoredType = ignoreTypes != null && Array.IndexOf(ignoreTypes, texture.Type) != -1;
                if (isIgnoredType)
                {
                    skinRenderShaderMaterial.SetTexture(textureName, _emptyAlphaTexture);
                    continue;
                }

                skinRenderShaderMaterial.SetTexture(textureName, texture.Current);
            }
            Graphics.Blit(Texture2D.whiteTexture, renderSkinTexture);
            Graphics.Blit(textures[TextureType.Skin].Current, renderSkinTexture, skinRenderShaderMaterial);

            return renderSkinTexture;
        }
    }
}