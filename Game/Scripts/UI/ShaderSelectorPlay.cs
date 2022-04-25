using System.Collections;
using System.Collections.Generic;
using CharacterEditor.CharacterInventory;
using UnityEngine;
using UnityEngine.UI;

namespace CharacterEditor
{
    public class ShaderSelectorPlay : MonoBehaviour
    {
        private Dropdown shaderDropdown;
        private List<TextureShaderType> shaders;

        void Awake()
        {
//            shaderDropdown = GetComponent<Dropdown>();
//            shaderDropdown.onValueChanged.AddListener(delegate
//            {
//                StartCoroutine(ShaderChanged());
//            });
        }

        void Start()
        {
//            shaders = new List<TextureShaderType>();
//            shaderDropdown.options.Clear();
//            foreach (var materialInfo in ItemManager.Instance.Materials)
//            {
//                shaders.Add(materialInfo.shader);
//                shaderDropdown.options.Add(new Dropdown.OptionData(materialInfo.shader.ToString()));
//            }
//            if (shaders.Count == 0)
//            {
//                gameObject.SetActive(false);
//                return;
//            }
//
//            shaderDropdown.value = 0;
//            shaderDropdown.captionText.text = shaders[0].ToString();
//            StartCoroutine(ShaderChanged());
        }

//        private IEnumerator ShaderChanged()
//        {
////            while (!ItemManager.Instance.IsReady)
////                yield return null;
////
////            var selectedShader = shaders[shaderDropdown.value];
////            ItemManager.Instance.SetShader(selectedShader);
//        }
    }
}