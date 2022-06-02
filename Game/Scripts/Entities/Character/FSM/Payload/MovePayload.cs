using System;
using UnityEngine;

namespace CharacterEditor.FmsPayload
{
    public class MovePayload
    {
        public Vector3 Point { get; }
        public Action Callback { get; }

        public MovePayload(Vector3 point, Action callback = null)
        {
            Point = point;
            Callback = callback;
        }
    }
}