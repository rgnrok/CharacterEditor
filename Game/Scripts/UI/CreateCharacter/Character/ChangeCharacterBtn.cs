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
        protected abstract CharacterChangeType _type { get; }

        private bool _isProcess;
        private IConfigManager _configManager; //todo ?

        void Awake()
        {
            _configManager = AllServices.Container.Single<IConfigManager>();
        }

        void Start()
        {
            _configManager.OnChangeCharacter += CharacterChangedHandler;
        }

        public async void OnPointerClick(PointerEventData eventData)
        {
            if (_isProcess || _configManager == null) return;

            _isProcess = true;
            if (_type == CharacterChangeType.Forward)
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
