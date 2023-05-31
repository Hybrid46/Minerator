using System;
using UnityEngine;

[Serializable]
public struct Point
{
    public Vector3 localPosition;
    public float density;
    public float textureIndex;

    public Point(Vector3 localPosition, float density, float textureIndex)
    {
        this.localPosition = localPosition;
        this.density = density;
        this.textureIndex = textureIndex;
    }
}