//#define DEBUG
using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using static StaticUtils;

public class MapGen : Singleton<MapGen>
{
    [Serializable]
    public class TexturesForAtlas
    {
        public Texture2D albedo;
        public Texture2D normalMap;
        public Texture2D metallicMap;

        public TexturesForAtlas() { }
    }

    //public static Vector3Int mapSizeInChunks = new Vector3Int(10, 10, 10);
    public static Vector3Int chunkSize = new Vector3Int(17, 17, 17);
    //public BoundsInt worldBounds;

    public Vector3Int renderDistance = new Vector3Int(200, 200, 200);
    [HideInInspector] public Vector2 offsetV;
    public Transform playerTransform;
    public Dictionary<Vector3Int, Chunk> ChunkCells = new Dictionary<Vector3Int, Chunk>();
    public HashSet<Vector3> savedChunks = new HashSet<Vector3>();
    public Material TerrainMat;

    private MarchingCubes marchCubes;

    int mineToolSize = 2;
    float mineToolSpeed = 0.1f;

    Chunk cScript;
    List<Chunk> chunksToUpdate = new List<Chunk>();
    HashSet<Chunk> chunksToUpdateHS = new HashSet<Chunk>();

    [SerializeField] private int currentChunkCount = 0;

    [HideInInspector] public Vector3Int chunkSnapVector;

    NativeArray<Point> Points;

    public List<TexturesForAtlas> textures;

    void Start()
    {
        Points = new NativeArray<Point>(chunkSize.x * chunkSize.y * chunkSize.z, Allocator.Persistent);
        //chunkMaterials = new NativeArray<MaterialType>(materials.Count, Allocator.Persistent);

        offsetV = new Vector2(UnityEngine.Random.Range(0, 9999999), UnityEngine.Random.Range(0, 9999999));
        chunkSnapVector = new Vector3Int(chunkSize.x - 1, chunkSize.y - 1, chunkSize.z - 1);

        Application.targetFrameRate = 60;

        /*
        worldBounds = new BoundsInt(0, 0, 0, 
                                    mapSizeInChunks.x * chunkSize.x - mapSizeInChunks.x * chunkSize.x / chunkSize.x, 
                                    mapSizeInChunks.y * chunkSize.y - mapSizeInChunks.y * chunkSize.y / chunkSize.y, 
                                    mapSizeInChunks.z * chunkSize.z - mapSizeInChunks.z * chunkSize.z / chunkSize.z);
        */
    }

    public Chunk CreateChunk(Vector3Int worldPos)
    {
        DateTime exectime = DateTime.Now;

        GameObject chunkObj = new GameObject();
        MeshFilter meshFilter = chunkObj.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = chunkObj.AddComponent<MeshRenderer>();
        MeshCollider meshCollider = chunkObj.AddComponent<MeshCollider>();

        chunkObj.transform.parent = transform;
        chunkObj.transform.position = worldPos;

        chunkObj.name = "Chunk " + chunkObj.transform.position;

        Chunk currentChunk = chunkObj.AddComponent<Chunk>();
        currentChunk.chunkWorldPos = worldPos;

        GenJob genJob = new GenJob()
        {
            chunkSize = chunkSize,
            chunkPos = currentChunk.chunkWorldPos,
            offsetV = offsetV,
            Points = Points
        };

        int genJobSize = chunkSize.x * chunkSize.y * chunkSize.z;
        JobHandle genJobHandle = genJob.Schedule(genJobSize, SystemInfo.processorCount);

        genJobHandle.Complete();

        //copy
        for (int i = 0; i < Points.Length; i++)
        {
            Vector3Int conv = Array1Dto3D(i, chunkSize.x, chunkSize.y);
            currentChunk.Points[conv.x, conv.y, conv.z] = Points[i];
        }

        marchCubes = new MarchingCubes(currentChunk.Points, 0.5f);
        Mesh mesh = marchCubes.CreateMeshData(currentChunk.Points);

        meshRenderer.material = TerrainMat;

        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;

        currentChunk.GetRefs();

        Debug.Log("Chunk generated in: " + (DateTime.Now - exectime).Milliseconds + " ms");

        return currentChunk;
    }

    private void OnDestroy()
    {
        Points.Dispose();
        //chunkMaterials.Dispose();
    }

    [BurstCompile]
    private struct GenJob : IJobParallelFor
    {
        [ReadOnly] public Vector3Int chunkSize;
        [ReadOnly] public Vector3 chunkPos;
        [ReadOnly] public Vector2 offsetV;
        public NativeArray<Point> Points;
        public Vector3Int localCoords;
        public Vector3 worldCoords;

        public void Execute(int i)
        {
            localCoords = Array1Dto3D(i, chunkSize.x, chunkSize.y);
            worldCoords = chunkPos + localCoords;

            Color color = Color.red;

            if (worldCoords.y > 0.0f) //air
                Points[i] = new Point(localCoords, 0, color);
            else if (worldCoords.y == 0.0f) //surface
                Points[i] = new Point(localCoords, 0.9f, color);
            else //undergorund
                Points[i] = new Point(localCoords, 1f, color);
        }
    }

    void Update()
    {
        currentChunkCount = ChunkCells.Count;

        //chunk gen and activate
        for (int z = (int)playerTransform.position.z - renderDistance.z; z < playerTransform.position.z + renderDistance.z; z += chunkSnapVector.z)
        {
            for (int y = (int)playerTransform.position.y - renderDistance.y; y < playerTransform.position.y + renderDistance.y; y += chunkSnapVector.y)
            {
                for (int x = (int)playerTransform.position.x - renderDistance.x; x < playerTransform.position.x + renderDistance.x; x += chunkSnapVector.x)
                {
                    Vector3Int pos = Vector3Int.RoundToInt(Snapping.Snap(new Vector3Int(x, y, z), chunkSnapVector, SnapAxis.All));
                    //if (!worldBounds.Contains(pos)) continue;

                    if (ChunkCells.ContainsKey(pos))
                    {
                        ChunkCells[pos].gameObject.SetActive(true);
                    }
                    else
                    {
                        ChunkCells.Add(pos, CreateChunk(pos));
                        goto next;
                    }
                }
            }
        }

    next:

        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit(); //escape pressed

        //mouse click(mining the terrain)
        if (Input.GetMouseButton(0))
        {
            Ray raym = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitm;
            if (Physics.Raycast(raym, out hitm))
            {
#if DEBUG
                Debug.DrawLine(raym.origin, hitm.point, Color.red);
#endif
                if (hitm.transform.TryGetComponent(out cScript))
                {
                    //checking points
                    Vector3Int hitPoint = new Vector3Int((int)hitm.point.x, (int)hitm.point.y, (int)hitm.point.z);

                    for (int xx = -mineToolSize; xx <= mineToolSize; xx++)
                    {
                        for (int zz = -mineToolSize; zz <= mineToolSize; zz++)
                        {
                            for (int yy = -mineToolSize; yy <= mineToolSize; yy++)
                            {
                                Vector3Int point = new Vector3Int(hitPoint.x + xx, hitPoint.y + yy, hitPoint.z + zz);
#if DEBUG
                                Debug.DrawLine(hitm.point, point, Color.cyan);
#endif
                                if (!PointInsideSphere(point, hitPoint, mineToolSize)) continue;

                                Vector3Int pointLocal = point - cScript.chunkWorldPos;

                                float density = 0.0f;
                                if (pointLocal.x >= 0 && pointLocal.y >= 0 && pointLocal.z >= 0 && pointLocal.x < chunkSize.x && pointLocal.y < chunkSize.y && pointLocal.z < chunkSize.z)
                                {
                                    if (Input.GetKey(KeyCode.LeftShift))
                                    {
                                        cScript.Points[pointLocal.x, pointLocal.y, pointLocal.z].density += mineToolSpeed;
                                    }
                                    else
                                    {
                                        cScript.Points[pointLocal.x, pointLocal.y, pointLocal.z].density -= mineToolSpeed;
                                    }

                                    density = cScript.Points[pointLocal.x, pointLocal.y, pointLocal.z].density;
                                }

                                //handle neighbours:
                                List<Vector3Int> neighbourCoords = GetNeighbourCoords(cScript.chunkWorldPos);

                                foreach (Vector3Int neighbourPosition in neighbourCoords)
                                {
#if DEBUG
                                    Debug.DrawLine(hitm.point, neighbourPosition, Color.magenta);
#endif
                                    Vector3Int pointLocalNeighb = point - neighbourPosition;
                                    Chunk nChunk = ChunkCells[neighbourPosition];

                                    if (pointLocalNeighb.x >= 0 && pointLocalNeighb.y >= 0 && pointLocalNeighb.z >= 0 && pointLocalNeighb.x < chunkSize.x && pointLocalNeighb.y < chunkSize.y && pointLocalNeighb.z < chunkSize.z)
                                    {
                                        nChunk.Points[pointLocalNeighb.x, pointLocalNeighb.y, pointLocalNeighb.z].density = density;
                                        //nChunk.Points[pointLocalNeighb.x, pointLocalNeighb.y, pointLocalNeighb.z].color = cScript.Points[pointLocal.x, pointLocal.y, pointLocal.z].color;
                                    }

                                    if (!chunksToUpdateHS.Contains(nChunk))
                                    {
                                        chunksToUpdate.Add(nChunk);
                                        chunksToUpdateHS.Add(nChunk);
                                    }
                                }
                            }
                        }
                    }
                    cScript.UpdateChunk();
                    chunksToUpdate.ForEach(chnk => { chnk.UpdateChunk(); });
                    chunksToUpdate.Clear();
                    chunksToUpdateHS.Clear();
                }
            }
        }
    }

    private List<Vector3Int> GetNeighbourCoords(Vector3Int worldCoord)
    {
        List<Vector3Int> neighbours = new List<Vector3Int>(26);
        Vector3Int actualCell = new Vector3Int();

        for (int z = worldCoord.z - chunkSnapVector.z; z <= worldCoord.z + chunkSnapVector.z; z += chunkSnapVector.z)
        {
            for (int y = worldCoord.y - chunkSnapVector.y; y <= worldCoord.y + chunkSnapVector.y; y += chunkSnapVector.y)
            {
                for (int x = worldCoord.x - chunkSnapVector.x; x <= worldCoord.x + chunkSnapVector.x; x += chunkSnapVector.x)
                {
                    actualCell.Set(x, y, z);

                    if (actualCell == worldCoord) continue; //skip self

                    if (ChunkCells.ContainsKey(actualCell))
                    {
                        neighbours.Add(actualCell);
                    }
                }
            }
        }

        return neighbours;
    }

    private void OnDrawGizmos()
    {
        foreach (KeyValuePair<Vector3Int, Chunk> chunk in ChunkCells)
        {
            if (chunk.Value.isActiveAndEnabled)
            {
                Gizmos.color = new Color(0.0f, 1.0f, 1.0f, 0.125f);
                Gizmos.DrawCube(chunk.Value.chunkWorldPos, chunkSize);
                Gizmos.color = new Color(0.0f, 1.0f, 1.0f, 0.2f);
                Gizmos.DrawWireCube(chunk.Value.chunkWorldPos, chunkSize);
            }
            else
            {
                Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.125f);
                Gizmos.DrawCube(chunk.Value.chunkWorldPos, chunkSize);
                Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.2f);
                Gizmos.DrawWireCube(chunk.Value.chunkWorldPos, chunkSize);
            }
        }
    }
}