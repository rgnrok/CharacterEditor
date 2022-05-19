using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CharacterEditor
{
    public abstract class MeshTypeMaskSelector : MonoBehaviour, IPointerClickHandler
    {
        [EnumFlag]
        public MeshType typeMask;
        protected MeshType[] types;

        private Button button;
        private IConfigManager _configManager;

        protected abstract void OnClick();

        private void Awake()
        {
            _configManager = AllServices.Container.Single<IConfigManager>();
        }

        private void Start()
        {
            button = GetComponent<Button>();

            List<MeshType> list = new List<MeshType>();
            foreach (var enumValue in System.Enum.GetValues(typeof(MeshType))) {
                int checkBit = (int)typeMask & (int)enumValue;
                if (checkBit != 0)
                    list.Add((MeshType)enumValue);
            }
            types = list.ToArray();

            if (_configManager != null)
                _configManager.OnChangeCharacter += DisableActionBtns;
        }

        private void OnDestroy()
        {
            if (_configManager != null)
                _configManager.OnChangeCharacter -= DisableActionBtns;
        }

        private void DisableActionBtns()
        {
            var interactable = false;
            foreach (MeshTypeBone bone in _configManager.Config.availableMeshes)
                if (Array.IndexOf(types, bone.mesh) != -1 && Array.IndexOf(MeshManager.Instance.CanChangeTypes, bone.mesh) != -1)
                {
                    interactable = true;
                    break;
                }

            button.interactable = interactable;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!button.interactable)
                return;

            OnClick();
        }
    }
}