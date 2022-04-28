using System;
using System.Runtime.InteropServices.WindowsRuntime;
using CharacterEditor.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CharacterEditor
{
    public class InputManager
    {
        public enum CursorState
        {
            Undefined,
            Default,
            PickUp,
            Hand,
            Attack,
            Speak,
        }

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

        public event Action<RaycastHit> OnChangeMouseRaycasHit;

        private RaycastHit _currentRaycastHit;
        private Vector3 _prevMousePosition;
        private CursorState _currentCursorState;

        private FollowCamera _camera;
        public InputManager()
        {
            _camera = Camera.main.GetComponent<FollowCamera>();
            _camera.OnPositionChanged += CameraPositionChangedHandler;
        }


        public void Update()
        {
            if (Input.anyKeyDown && !Input.GetKey(KeyCode.Mouse0) && !Input.GetKey(KeyCode.Mouse1))
            {
                if (EventSystem.current.currentSelectedGameObject == null ||
                    EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() == null)
                {
                    KeyDownEvent();
                    return;
                }
            }

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

            if (Input.GetMouseButton(0))
            {
                return;
            }
            UpdateCursor();
        }

        private void KeyDownEvent()
        {
            if (Input.GetKeyDown(KeyCode.I))
                ToggleInventory?.Invoke();

            if (Input.GetKeyDown(KeyCode.C))
                ToggleCharacterInfo?.Invoke();

            if (Input.GetKeyDown(KeyCode.Space))
                SpacePress?.Invoke();

            if (Input.GetKeyDown(KeyCode.Escape))
                EscapePress?.Invoke();
        }

        private bool MouseLeftDown()
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                var successHit = GetMouseHit(Constants.LAYER_CHARACTER, Constants.LAYER_CONTAINER, Constants.LAYER_PICKUP, Constants.LAYER_GROUND, Constants.LAYER_ENEMY);
                if (successHit)
                {
                    switch (_currentRaycastHit.collider.gameObject.layer)
                    {
                        case Constants.LAYER_CHARACTER:
                            if (CharacterGameObjectClick != null) CharacterGameObjectClick(_currentRaycastHit);
                            break;
                        case Constants.LAYER_ENEMY:
                            if (EnemyGameObjectClick != null) EnemyGameObjectClick(_currentRaycastHit);
                            break;
                        case Constants.LAYER_CONTAINER:
                            if (ContainerGameObjectClick != null) ContainerGameObjectClick(_currentRaycastHit);
                            break;
                        case Constants.LAYER_PICKUP:
                            if (PickUpObjectClick != null) PickUpObjectClick(_currentRaycastHit);
                            break;
                        case Constants.LAYER_GROUND:
                            if (GroundDownClick != null) GroundDownClick(_currentRaycastHit);
                            break;
                    }
                    return true;
                }
            }

            return false;
        }

        private bool MouseLeftUp()
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                var successHit = GetMouseHit(Constants.LAYER_GROUND);
                if (successHit)
                {
                    if (GroundUpClick != null) GroundUpClick(_currentRaycastHit);
                    return true;
                }
            }

            return false;
        }

        private bool MouseRightDown()
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                var successHit = GetMouseHit(Constants.LAYER_NPC);
                if (successHit)
                {
                    if (NpcGameObjectClick != null) NpcGameObjectClick(_currentRaycastHit);
                    return true;
                }
            }

            return false;
        }

        private bool GetMouseHit(params int[] layers)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (layers.Length == 0) return false;

            int mask = 0;
            foreach (var layer in layers)
            {
                mask |= 1 << layer;
            }

            return Physics.Raycast(ray, out _currentRaycastHit, float.MaxValue, mask);
//            if (rayCasts.Length == 0) return false;

//            UpdateCurrentRaycast(rayCasts);
            return true;
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

            var successHit = GetMouseHit(Constants.LAYER_PICKUP, Constants.LAYER_CONTAINER, Constants.LAYER_NPC, Constants.LAYER_ENEMY, Constants.LAYER_CHARACTER, Constants.LAYER_GROUND);
            if (!successHit)
            {
                UpdateCursor(CursorState.Default);
                return;
            }

            if (OnChangeMouseRaycasHit != null) OnChangeMouseRaycasHit(_currentRaycastHit);
        }

        public void UpdateCursor(CursorState state)
        {
            if (_currentCursorState == state) return;

            _currentCursorState = state;
            var cursorPath = GetCursorIconByState(state);
            if (cursorPath == null)
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                return;
            }

            // @todo
            var loaderService = AllServices.Container.Single<ILoaderService>();
            loaderService.TextureLoader.LoadByPath(cursorPath,
                (path, texture) =>
                {
                    if (texture != null)
                        Cursor.SetCursor(texture, Vector2.zero, CursorMode.Auto);
                    
                });
        }

        private string GetCursorIconByState(CursorState state)
        {
            switch (state)
            {
                case CursorState.Default:
                    return "cursors/Cursor_Arrow_1";
                case CursorState.Hand:
                    return "cursors/Cursor_ItemMove_1";
                case CursorState.PickUp:
                    return "cursors/Cursor_ItemMove_2";
                case CursorState.Attack:
                    return "cursors/Cursor_Melee_1";
            }

            return null;
        }

        private void CameraPositionChangedHandler()
        {
            UpdateCursor(true);
        }
    }
}
//Invalid texture used for cursor - check importer settings or texture creation.Texture must be RGBA32, readable, have alphaIsTransparency enabled and have no mip chain.