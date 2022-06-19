using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public Point[,,] Points = new Point[MapGen.chunkSize.x, MapGen.chunkSize.y, MapGen.chunkSize.z];
    //public Dictionary<Vector3Int, Point> worldPoints = new Dictionary<Vector3Int, Point>();

    public Vector3Int chunkWorldPos;
    public bool saved = false;

    private Transform myTransform;
    private MeshFilter myMeshFilter;
    private Mesh myMesh;
    private MeshCollider myMeshCollider;
    private float distanceToPlayer;

    public void InitMesh()
    {
        myTransform = transform;
        myMeshFilter = GetComponent<MeshFilter>();
        myMesh = myMeshFilter.sharedMesh;
        myMeshCollider = GetComponent<MeshCollider>();

        /*
        for (int z = 0; z < MapGen.chunkSize.z; z++)
        {
            for (int y = 0; y < MapGen.chunkSize.y; y++)
            {
                for (int x = 0; x < MapGen.chunkSize.x; x++)
                {
                    worldPoints.Add(chunkWorldPos + new Vector3Int(x, y, z), Points[x, y, z]);
                }
            }
        }
        */
    }

    public void UpdateChunk()
    {
        MarchingCubes marchc = new MarchingCubes(Points, 0.5f);
        Mesh mesh = marchc.CreateMeshData(Points);

        myMeshFilter.sharedMesh = mesh;
        myMeshCollider.sharedMesh = mesh;
    }

    private void Update()
    {
        distanceToPlayer = Vector3.Distance(MapGen.instance.playerTransform.position, myTransform.position);

        if (distanceToPlayer > MapGen.instance.renderDistance.x * 2.0f)
        {
            gameObject.SetActive(false);
        }
    }

    public void SaveChunkToDisk()
    {
        Debug.Log("Chunk not saved");
    }

    public void LoadChunkFromDisk()
    {
        Debug.Log("Chunk not loaded");
    }

    public void DestroyChunk()
    {
        GetComponentInParent<MapGen>().ChunkCells.Remove(chunkWorldPos);
        Destroy(myMesh);
        Destroy(gameObject);
    }
}
