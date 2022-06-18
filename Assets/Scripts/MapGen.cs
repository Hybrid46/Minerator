//#define DEBUG
using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class MapGen : Singleton<MapGen>
{
    //public static Vector3Int mapSizeInChunks = new Vector3Int(10, 10, 10);
    public static Vector3Int chunkSize = new Vector3Int(16, 16, 16);
    //public BoundsInt worldBounds;

    public Vector3Int renderDistance = new Vector3Int(200, 200, 200);
    public Vector2 offsetV;
    public Transform playerTransform;
    public Dictionary<Vector3Int, Chunk> ChunkCells = new Dictionary<Vector3Int, Chunk>();
    public Material TerrainMat;

    [SerializeField]
    public List<MaterialType> materials = new List<MaterialType>();

    MarchingCubes marchCubes;

    //for mining
    int mineToolSize = 2;
    float mineSpeed = 0.1f;

    Chunk cScript;
    List<Chunk> chunksToUpdate = new List<Chunk>();
    HashSet<Chunk> chunksToUpdateHS = new HashSet<Chunk>();

    private int currentChunkCount = 0;

    private Vector3Int chunkSnapVector;

    //generate
    NativeArray<Point> Points;
    //NativeArray<MaterialType> chunkMaterials;

    void Start()
    {
        Points = new NativeArray<Point>(chunkSize.x * chunkSize.y * chunkSize.z, Allocator.Persistent);
        //chunkMaterials = new NativeArray<MaterialType>(materials.Count, Allocator.Persistent);

      offsetV = new Vector2(UnityEngine.Random.Range(0, 9999999), UnityEngine.Random.Range(0, 9999999));
        chunkSnapVector = new Vector3Int(chunkSize.x - 2, chunkSize.y - 2, chunkSize.z - 2);

        //QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        /*
        worldBounds = new BoundsInt(0, 0, 0, 
                                    mapSizeInChunks.x * chunkSize.x - mapSizeInChunks.x * chunkSize.x / chunkSize.x, 
                                    mapSizeInChunks.y * chunkSize.y - mapSizeInChunks.y * chunkSize.y / chunkSize.y, 
                                    mapSizeInChunks.z * chunkSize.z - mapSizeInChunks.z * chunkSize.z / chunkSize.z);
        */

        //Debug.Log(worldBounds.max);
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
            Points = Points,
            chunkPos = currentChunk.chunkWorldPos,
            //materials = chunkMaterials,
            offsetV = offsetV
        };

        int genJobSize = chunkSize.x * chunkSize.y * chunkSize.z;
        JobHandle genJobHandle = genJob.Schedule(genJobSize, SystemInfo.processorCount);

        genJobHandle.Complete();

        //copy
        for (int i = 0; i < Points.Length; i++)
        {
            Vector3Int conv = StaticUtils.Array1Dto3D(i, chunkSize.x, chunkSize.y);
            currentChunk.Points[conv.x, conv.y, conv.z] = Points[i];
        }

        marchCubes = new MarchingCubes(currentChunk.Points, 0.5f);
        Mesh mesh = marchCubes.CreateMeshData(currentChunk.Points);

        meshRenderer.material = TerrainMat;

        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;

        currentChunk.InitMesh();

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
        [ReadOnly] public Vector2 offsetV;
        //[ReadOnly] public NativeArray<MaterialType> materials;
        public NativeArray<Point> Points;
        public Vector3 chunkPos;
        public Vector3Int localCoords;
        public Vector3 worldCoords;
        private MaterialType matType;

        public void Execute(int i)
        {
            localCoords = StaticUtils.Array1Dto3D(i, chunkSize.x, chunkSize.y);
            worldCoords = new Vector3(chunkPos.x + localCoords.x, chunkPos.y + localCoords.y, chunkPos.z + localCoords.z);

            //matType = StaticUtils.ChooseMaterialByHeight(height[heightIndexFromLocalCoord], materials);

            if (worldCoords.y > 0.0f) //air
                Points[i] = new Point(localCoords, worldCoords, 0, matType.color, matType);
            else if (worldCoords.y == 0.0f) //surface
                Points[i] = new Point(localCoords, worldCoords, 0.9f, matType.color, matType);
            else //undergorund
                Points[i] = new Point(localCoords, worldCoords, 1f, matType.color, matType);
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
                    //}
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
                if (hitm.transform.TryGetComponent(typeof(Chunk), out Component comp))
                {
                    cScript = (Chunk)comp;

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
                                if (!StaticUtils.PointInsideSphere(point, hitPoint, mineToolSize)) continue;

                                Vector3Int pointLocal = point - cScript.chunkWorldPos;

                                float density = 0.0f;
                                if (pointLocal.x >= 0 && pointLocal.y >= 0 && pointLocal.z >= 0 && pointLocal.x < chunkSize.x && pointLocal.y < chunkSize.y && pointLocal.z < chunkSize.z)
                                {
                                    if (Input.GetKey(KeyCode.LeftShift))
                                    {
                                        cScript.Points[pointLocal.x, pointLocal.y, pointLocal.z].density += mineSpeed;
                                    }
                                    else
                                    {
                                        cScript.Points[pointLocal.x, pointLocal.y, pointLocal.z].density -= mineSpeed;
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

    public static Vector3 GetPointChunkCoord(int x, int y, int z) => new Vector3 { x = x % chunkSize.x, y = y % chunkSize.y, z = z % chunkSize.z };

    private void OnDrawGizmos()
    {
        foreach (KeyValuePair<Vector3Int, Chunk> chunk in ChunkCells)
        {
            if (chunk.Value.isActiveAndEnabled)
            {
                GizmoExtension.GizmosExtend.DrawBox(chunk.Value.chunkWorldPos, chunkSize, Quaternion.identity, Color.green);
            }
            else
            {
                GizmoExtension.GizmosExtend.DrawBox(chunk.Value.chunkWorldPos, chunkSize, Quaternion.identity, Color.red);
            }
        }

        /*
        //Camera matrix
        for (int z = (int)playerTransform.position.z - renderDistance.z; z < playerTransform.position.z + renderDistance.z; z += chunkSize.z - 1)
        {
            for (int y = (int)playerTransform.position.y - renderDistance.y; y < playerTransform.position.y + renderDistance.y; y += chunkSize.y - 1)
            {
                for (int x = (int)playerTransform.position.x - renderDistance.x; x < playerTransform.position.x + renderDistance.x; x += chunkSize.x - 1)
                {
                    Vector3Int pos = Vector3Int.RoundToInt(Snapping.Snap(new Vector3Int(x, y, z), new Vector3Int(chunkSize.x - 2, chunkSize.y - 2, chunkSize.z - 2), SnapAxis.All));

                    GizmoExtension.GizmosExtend.DrawBox(pos, chunkSize, Quaternion.identity, Color.green);
                }
            }
        }
        */
    }
}

[System.Serializable]
public struct MaterialType
{
    public int id;
    public float minHeight;
    public float maxHeight;
    public Color color;
    [Range(0, 1)]
    public float metallic;
    [Range(0, 1)]
    public float smoothness;
    public Color emission;
}