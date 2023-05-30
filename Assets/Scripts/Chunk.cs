using System;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public Point[,,] Points = new Point[MapGen.chunkSize.x, MapGen.chunkSize.y, MapGen.chunkSize.z];

    public Vector3Int chunkWorldPos;
    public bool saved = false;
    private float distanceToPlayer;

    public Transform m_Transform { get; private set; }
    public MeshFilter m_MeshFilter { get; private set; }
    public Mesh m_Mesh { get; private set; }
    public MeshCollider m_MeshCollider { get; private set; }

    public void GetRefs()
    {
        m_Transform = transform;
        m_MeshFilter = GetComponent<MeshFilter>();
        m_Mesh = m_MeshFilter.sharedMesh = new Mesh();
        m_MeshCollider = GetComponent<MeshCollider>();
    }

    public void UpdateChunk()
    {
        MapGen.instance.marchCubes.Marching(m_MeshFilter.sharedMesh, Points, 0.5f);

        if (m_MeshFilter.sharedMesh.vertexCount > 2) m_MeshCollider.sharedMesh = m_MeshFilter.sharedMesh;

        saved = false;
    }

    private void Update()
    {
        distanceToPlayer = Vector3.Distance(MapGen.instance.playerTransform.position, m_Transform.position);

        if (distanceToPlayer > MapGen.instance.renderDistance.x * 2)
        {
            gameObject.SetActive(false);
        }
    }

    public void SaveChunkToDisk()
    {
        throw new NotImplementedException("Chunk not saved");

        //save

        MapGen.instance.savedChunks.Add(m_Transform.position);
        DestroyChunk();
    }

    public void LoadChunkFromDisk()
    {
        throw new NotImplementedException("Chunk not loaded");

        if (!MapGen.instance.savedChunks.Contains(m_Transform.position)) throw new Exception("Chunk not saved!");

        //load

        GetRefs();
        UpdateChunk();
    }

    public void DestroyChunk()
    {
        MapGen.instance.ChunkCells.Remove(chunkWorldPos);
        Destroy(m_Mesh);
        Destroy(gameObject);
    }

    public Vector3 GetWorldPoint(Vector3 localPoint) => m_Transform.position + localPoint;

    public Vector3 GetLocalPoint(Vector3 worldPoint) => worldPoint - m_Transform.position;
}
