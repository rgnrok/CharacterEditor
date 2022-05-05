using System.Collections.Generic;
using UnityEngine;

namespace CharacterEditor.Services
{
    public class MergeTextureService : IMergeTextureService
    {
        private Texture2D _emptyAlphaTexture;
        private Dictionary<string, List<string>> _shaderTextures = new Dictionary<string, List<string>>();

        public MergeTextureService()
        {
            CreateEmptyTexture();
        }

        /*
         * Combining the texture of the character
         */
        public void MergeTextures(Material skinRenderShaderMaterial, RenderTexture renderSkinTexture, Texture2D baseTexture, Dictionary<string, Texture2D> textures)
        {
            var textureNames = GetTexturesName(skinRenderShaderMaterial.shader);
            foreach (var textureName in textureNames)
            {
                if (textures.ContainsKey(textureName)) continue;
                skinRenderShaderMaterial.SetTexture(textureName, _emptyAlphaTexture);
            }

            foreach (var texturePair in textures)
            {
                skinRenderShaderMaterial.SetTexture(texturePair.Key, texturePair.Value);
            }

            Graphics.Blit(Texture2D.whiteTexture, renderSkinTexture);
            Graphics.Blit(baseTexture, renderSkinTexture, skinRenderShaderMaterial);
        }

        private void CreateEmptyTexture()
        {
            _emptyAlphaTexture = new Texture2D(1, 1);
            _emptyAlphaTexture.SetPixel(0, 0, Color.clear);
            _emptyAlphaTexture.Apply();
        }

        private IEnumerable<string> GetTexturesName(Shader shader)
        {
            if (_shaderTextures.TryGetValue(shader.name, out var textures))
                return textures;

            var names = new List<string>();
            for (var i = 0; i < shader.GetPropertyCount(); i++)
            {
                if (shader.GetPropertyType(i) != UnityEngine.Rendering.ShaderPropertyType.Texture) continue;
                names.Add(shader.GetPropertyName(i));
            }

            _shaderTextures[shader.name] = names;
            return names;
        }
    }
}