using CharacterEditor.Services;

namespace CharacterEditor
{
    public class Popup : GameUI
    {
        private bool _isShow;
        private IInputService _inputService;

        void Start()
        {
            _inputService = AllServices.Container.Single<IInputService>();
            _inputService.EscapePress += EscapePressHandler;
        }

        private void OnDestroy()
        {
            if (_inputService != null)
                _inputService.EscapePress -= EscapePressHandler;
        }


        public void Toggle()
        {
            gameObject.SetActive(!gameObject.activeSelf);
            _isShow = !_isShow;
        }

        public void Open()
        {
            gameObject.SetActive(true);
            _isShow = true;
        }

        public void Close()
        {
            gameObject.SetActive(false);
            _isShow = false;
        }

        private void EscapePressHandler()
        {
            if (_isShow) Close();
        }
    }
}