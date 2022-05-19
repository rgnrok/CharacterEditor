using UnityEngine;
using UnityEngine.EventSystems;

namespace CharacterEditor
{
    public enum CharacterChangeType
    {
        Forward,
        Back
    }

    public abstract class ChangeCharacterBtn : MonoBehaviour, IPointerClickHandler
    {
        protected abstract CharacterChangeType Type { get; }

        private bool _isProcess;
        private IConfigManager _configManager; //todo ?

        private void Awake()
        {
            _configManager = AllServices.Container.Single<IConfigManager>();
        }

        private void Start()
        {
            _configManager.OnChangeCharacter += CharacterChangedHandler;
        }

        void OnDestroy()
        {
            if (_configManager != null)
                _configManager.OnChangeCharacter -= CharacterChangedHandler;
        }

        public async void OnPointerClick(PointerEventData eventData)
        {
            if (_isProcess || _configManager == null) return;

            _isProcess = true;
            if (Type == CharacterChangeType.Forward)
                await _configManager.OnNextCharacter();
            else
                await _configManager.OnPrevCharacter();
        }

        private void CharacterChangedHandler()
        {
            _isProcess = false;
        }
    }
}
