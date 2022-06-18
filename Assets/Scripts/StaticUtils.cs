using UnityEngine;

public static class StaticUtils
{
    public static int Array3DTo1D(int x, int y, int z, int xMax, int yMax) => (z * xMax * yMax) + (y * xMax) + x;
    public static Vector3Int Array1Dto3D(int idx, int xMax, int yMax)
    {
        int z = idx / (xMax * yMax);
        idx -= (z * xMax * yMax);
        int y = idx / xMax;
        int x = idx % xMax;
        return new Vector3Int(x, y, z);
    }
    public static int Array2dTo1d(int x, int y, int width) => y * width + x;
    public static Vector2Int Array1dTo2d(int i, int width) => new Vector2Int { x = i % width, y = i / width };

    public static bool PointInsideSphere(Vector3 Ppoint, Vector3 Ccenter, float Cradius)
    {
        return (Vector3.Distance(Ppoint, Ccenter) <= Cradius);
    }

    public static float Rounder(float x, float g = 16) => Mathf.Floor((x + g / 2) / g) * g;
    public static int RounderInt(float x, float g = 16) => (int)((Mathf.Floor((x + g / 2) / g) * g));
    public static int RounderInt(int x, int g = 16) => (int)Mathf.Floor((x + g / 2) / g) * g;

    public static Vector3 Snap(Vector3 pos, int v)
    {
        float x = pos.x;
        float y = pos.y;
        float z = pos.z;
        x = Mathf.FloorToInt(x / v) * v;
        y = Mathf.FloorToInt(y / v) * v;
        z = Mathf.FloorToInt(z / v) * v;
        return new Vector3(x, y, z);
    }
}
