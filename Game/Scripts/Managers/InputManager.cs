using System;
using CharacterEditor.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CharacterEditor
{
    public class InputManager : IService
    {
        public event Action ToggleInventory;
        public event Action ToggleCharacterInfo;
        public event Action EscapePress;
        public event Action SpacePress;

        public event Action<RaycastHit> ContainerGameObjectClick;
        public event Action<RaycastHit> CharacterGameObjectClick;
        public event Action<RaycastHit> EnemyGameObjectClick;
        public event Action<RaycastHit> NpcGameObjectClick;
        public event Action<RaycastHit> PickUpObjectClick;

        public event Action<RaycastHit> GroundUpClick;
        public event Action<RaycastHit> GroundDownClick;

        public event Action<RaycastHit> OnChangeMouseRaycastHit;

        private RaycastHit _currentRaycastHit;
        private Vector3 _prevMousePosition;
        private CursorType _currentCursorType;

        private FollowCamera _camera;

        private int _mouseHitMask;
        private int _cursorHintMask;

        public InputManager()
        {
            _mouseHitMask = 1 << Constants.LAYER_CHARACTER
                            | 1 << Constants.LAYER_CONTAINER
                            | 1 << Constants.LAYER_PICKUP
                            | 1 << Constants.LAYER_GROUND
                            | 1 << Constants.LAYER_ENEMY;

            _cursorHintMask = _mouseHitMask | 1 << Constants.LAYER_NPC;
        }

        public void SetupCamera(FollowCamera camera)
        {
            _camera = camera;
            if (camera != null)
                _camera.OnPositionChanged += CameraPositionChangedHandler;
        }


        public void Update()
        {
            if (Input.GetMouseButtonDown(0)) //Left
            {
                if (MouseLeftDown()) return;
            }

            if (Input.GetMouseButtonDown(1)) //Right
            {
                if (MouseRightDown()) return;
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (MouseLeftUp()) return;
            }

            if (Input.anyKeyDown && !Input.GetKey(KeyCode.Mouse0) && !Input.GetKey(KeyCode.Mouse1))
            {
                if (EventSystem.current.currentSelectedGameObject == null ||
                    EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() == null)
                {
                    KeyDownEvent();
                    return;
                }
            }

            UpdateCursor();
        }

        private void KeyDownEvent()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                ToggleInventory?.Invoke();
                return;
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                ToggleCharacterInfo?.Invoke();
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                SpacePress?.Invoke();
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                EscapePress?.Invoke();
                return;
            }
        }

        private bool MouseLeftDown()
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                var successHit = GetMouseHitByMask(_mouseHitMask);
                if (successHit)
                {
                    switch (_currentRaycastHit.collider.gameObject.layer)
                    {
                        case Constants.LAYER_CHARACTER:
                            CharacterGameObjectClick?.Invoke(_currentRaycastHit);
                            break;
                        case Constants.LAYER_ENEMY:
                            EnemyGameObjectClick?.Invoke(_currentRaycastHit);
                            break;
                        case Constants.LAYER_CONTAINER:
                            ContainerGameObjectClick?.Invoke(_currentRaycastHit);
                            break;
                        case Constants.LAYER_PICKUP:
                            PickUpObjectClick?.Invoke(_currentRaycastHit);
                            break;
                        case Constants.LAYER_GROUND:
                            GroundDownClick?.Invoke(_currentRaycastHit);
                            break;
                    }
                    return true;
                }
            }

            return false;
        }

        private bool MouseLeftUp()
        {
            if (EventSystem.current.IsPointerOverGameObject()) return false;

            var successHit = GetMouseHitByLayer(Constants.LAYER_GROUND);
            if (successHit) GroundUpClick?.Invoke(_currentRaycastHit);

            return successHit;
        }

        private bool MouseRightDown()
        {
            if (EventSystem.current.IsPointerOverGameObject()) return false;

            var successHit = GetMouseHitByLayer(Constants.LAYER_NPC);
            if (successHit) NpcGameObjectClick?.Invoke(_currentRaycastHit);

            return successHit;
        }

        private bool GetMouseHitByLayer(int layer, float distance = 50)
        {
            return GetMouseHitByMask(1 << layer, distance);
        }

        private bool GetMouseHit(params int[] layers)
        {
            if (layers.Length == 0) return false;

            var mask = 0;
            foreach (var layer in layers)
                mask |= 1 << layer;

            return GetMouseHitByMask(mask);
        }

        private bool GetMouseHitByMask(int mask, float distance = 50)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            return Physics.Raycast(ray, out _currentRaycastHit, distance, mask);
        }


        private void UpdateCurrentRaycast(RaycastHit[] hits)
        {
            if (hits.Length == 0) return;
            if (hits.Length == 1)
            {
                _currentRaycastHit = hits[0];
                return;
            }

            var currentCharacterGoId = GameManager.Instance.CurrentCharacter.EntityGameObject.GetInstanceID();
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider.gameObject.layer == Constants.LAYER_CHARACTER && hits[i].collider.gameObject.GetInstanceID() == currentCharacterGoId)
                    continue;

                _currentRaycastHit = hits[i];
                break;
            }
        }

        private void UpdateCursor(bool force = false)
        {
            if (!force && _prevMousePosition == Input.mousePosition) return;
            _prevMousePosition = Input.mousePosition;

            var successHit = GetMouseHitByMask(_cursorHintMask);
            if (!successHit)
            {
                UpdateCursor(CursorType.Default);
                return;
            }

            OnChangeMouseRaycastHit?.Invoke(_currentRaycastHit);
        }

        public async void UpdateCursor(CursorType type)
        {
            if (_currentCursorType == type) return;

            _currentCursorType = type;

            var loaderService = AllServices.Container.Single<ILoaderService>();
            var cursorTexture = await loaderService.CursorLoader.LoadCursor(_currentCursorType);
            if (cursorTexture != null) Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
        }

 

        private void CameraPositionChangedHandler()
        {
            UpdateCursor(true);
        }
    }
}
