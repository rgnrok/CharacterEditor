namespace CharacterEditor
{
    public class Popup : GameUI
    {
        private bool _isShow;
        private InputManager _inputManager;

        void Start()
        {
            _inputManager = AllServices.Container.Single<InputManager>();
            _inputManager.EscapePress += EscapePressHandler;
        }

        private void OnDestroy()
        {
            if (_inputManager != null)
                _inputManager.EscapePress -= EscapePressHandler;
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