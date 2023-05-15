using System;
using UnityEngine;

[Serializable]
public struct Point
{
    public Vector3 localPosition;
    public float density;
    public Color color;

    public Point(Vector3 localPosition, float density, Color color)
    {
        this.localPosition = localPosition;
        this.density = density;
        this.color = color;
    }
}