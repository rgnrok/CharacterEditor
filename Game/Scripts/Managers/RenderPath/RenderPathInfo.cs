using System.Collections.Generic;
using UnityEngine;

public struct RenderPathInfo
{
    public Vector3 targetPoint;
    public Vector3 endPoint;
    public bool isComplete;
    public float totalDistance;
    public IAttacked attacked;

    public Vector3[] points;

    public RenderPathInfo(Vector3 point) : this()
    {
        targetPoint = point;
        endPoint = point;
        points = new Vector3[0];
    }
}
