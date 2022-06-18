using UnityEngine;

public struct Point
{
    public Vector3 localPosition;
    public Vector3 worldPosition;
    public float density;
    public Color color;
    public MaterialType materialType;

    public Point(Vector3 localPosition, Vector3 worldPosition, float density, Color color, MaterialType materialType)
    {
        this.localPosition = localPosition;
        this.worldPosition = worldPosition;
        this.density = density;
        this.color = color;
        this.materialType = materialType;
    }
}