using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterEditor
{
    public class GameUI : MonoBehaviour
    {
        protected SizeController _sizeController;
        protected Transform _content;

        protected virtual void OnEnable()
        {
            InnerUpdate();
        }

        protected void InnerUpdate()
        {
            if (_sizeController == null) _sizeController = GetComponent<SizeController>();
            if (_sizeController != null) _sizeController.Recalculate();

        }

        protected GameObject AddChild(GameObject prefab)
        {
            if (_content == null)
            {
                _content = transform.Find("Content");
                if (_content == null) _content = transform;
            }

            return Instantiate(prefab, _content);
        }
    }
}