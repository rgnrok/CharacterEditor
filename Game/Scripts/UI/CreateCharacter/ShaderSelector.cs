using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CharacterEditor
{
    public class ShaderSelector : MonoBehaviour
    {
        private Dropdown shaderDropdown;
        private List<TextureShaderType> shaders;

        void Awake()
        {
            shaderDropdown = GetComponent<Dropdown>();
            shaderDropdown.onValueChanged.AddListener(delegate
            {
                StartCoroutine(ShaderChanged());
            });
        }

        void Start()
        {
            if (TextureManager.Instance == null) return;

            shaders = new List<TextureShaderType>();
            shaderDropdown.options.Clear();
            foreach (var materialInfo in TextureManager.Instance.Materials)
            {
                shaders.Add(materialInfo.shader);
                shaderDropdown.options.Add(new Dropdown.OptionData(materialInfo.shader.ToString()));
            }
            if (shaders.Count == 0)
            {
                gameObject.SetActive(false);
                return;
            }

            shaderDropdown.value = 0;
            shaderDropdown.captionText.text = shaders[0].ToString();
        }

        private IEnumerator ShaderChanged()
        {
            while (!TextureManager.Instance.IsReady || !MeshManager.Instance.IsReady)
                yield return null;

            var selectedShader = shaders[shaderDropdown.value];
            TextureManager.Instance.SetShader(selectedShader);
            MeshManager.Instance.SetShader(selectedShader);
        }
    }
}