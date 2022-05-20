using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CharacterEditor
{
    public class ShaderSelector : MonoBehaviour
    {
        private Dropdown _shaderDropdown;
        private List<TextureShaderType> _shaderTypes;
        private PrefabShaderManager _prefabShaderManager;

        private void Awake()
        {
            _shaderDropdown = GetComponent<Dropdown>();
            _shaderDropdown.onValueChanged.AddListener(ShaderChanged);

            _prefabShaderManager = PrefabShaderManager.Instance;
        }

        private void Start()
        {
            if (_prefabShaderManager == null) return;
            InitShaderDropdown();
        }

        private void InitShaderDropdown()
        {
            _shaderTypes = new List<TextureShaderType>();
            _shaderDropdown.options.Clear();
            foreach (var materialInfo in _prefabShaderManager.Materials)
            {
                _shaderTypes.Add(materialInfo.shader);
                _shaderDropdown.options.Add(new Dropdown.OptionData(materialInfo.shader.ToString()));
            }

            if (_shaderTypes.Count == 0)
            {
                gameObject.SetActive(false);
                return;
            }

            _shaderDropdown.value = 0;
            _shaderDropdown.captionText.text = _shaderTypes[0].ToString();
        }

        private void ShaderChanged(int selected)
        {
            _prefabShaderManager.UpdateCharacterMaterials(_shaderTypes[selected]);
        }
    }
}