using System.Collections.Generic;
using UnityEngine;

namespace CharacterEditor.Services
{
    public class MergeTextureService : IMergeTextureService
    {
        private Texture2D _emptyAlphaTexture;
        private readonly Dictionary<string, List<string>> _shaderTextures = new Dictionary<string, List<string>>();

        public MergeTextureService()
        {
            CreateEmptyTexture();
        }

        /*
         * Combining the texture of the character with shader
         */
        public void MergeTextures(Material skinRenderShaderMaterial, RenderTexture renderSkinTexture, Dictionary<string, Texture2D> textures)
        {
            var textureNames = GetTexturesName(skinRenderShaderMaterial.shader);
            foreach (var textureName in textureNames)
            {
                if (textures.ContainsKey(textureName)) continue;
                skinRenderShaderMaterial.SetTexture(textureName, _emptyAlphaTexture);
            }

            foreach (var texturePair in textures)
                skinRenderShaderMaterial.SetTexture(texturePair.Key, texturePair.Value);

            Graphics.Blit(Texture2D.whiteTexture, renderSkinTexture, skinRenderShaderMaterial);
        }


        public Texture2D BuildTextureAtlas(int partTextureSize, List<Texture2D> textures)
        {
            if (textures.Count == 0) return null;

            var texturesPerRow = CalculateTexturesPerRow(textures.Count);
            var resultTextureSize = partTextureSize * texturesPerRow;

            var resultTexture = new Texture2D(resultTextureSize, resultTextureSize, TextureFormat.RGB24, false);

            var j = 0;
            foreach (var texture in textures)
            {
                var position = j++;
                var x = partTextureSize * (position % (resultTextureSize / partTextureSize));
                var y = (resultTextureSize - partTextureSize) - (partTextureSize * position / resultTextureSize) * partTextureSize;

                resultTexture.SetPixels32(x, y, partTextureSize, partTextureSize, texture.GetPixels32());
            }

            return resultTexture;
        }

        private static int CalculateTexturesPerRow(int count)
        {
            return count > 4 ? 4 :
                count > 1 ? 2 : 1;
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