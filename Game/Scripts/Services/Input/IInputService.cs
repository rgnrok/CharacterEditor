using System;
using UnityEngine;

namespace CharacterEditor.Services
{
    public interface IInputService : IService
    {
        event Action ToggleInventory;
        event Action ToggleCharacterInfo;
        event Action EscapePress;
        event Action SpacePress;
        event Action<RaycastHit> ContainerGameObjectClick;
        event Action<RaycastHit> CharacterGameObjectClick;
        event Action<RaycastHit> EnemyGameObjectClick;
        event Action<RaycastHit> NpcGameObjectClick;
        event Action<RaycastHit> PickUpObjectClick;
        event Action<Vector3> GroundClick;
        event Action<RaycastHit> OnChangeMouseRaycastHit;
        void SetupCamera(FollowCamera camera);

        void Update();
        void UpdateCursor(CursorType type);
    }
}