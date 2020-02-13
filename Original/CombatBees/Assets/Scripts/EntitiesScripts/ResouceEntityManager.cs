using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Rendering;
using Unity.Transforms;
using System;
using System.Collections.Generic;

public class ResouceEntityManager : MonoBehaviour
{
    [Header("Meshes and Mats")]
    public Mesh resourceMesh;
    public Material resourceMat;

    [Header("Stats and Counters")]
    public float resourceSize;
    public float snapStiffness;
    public float carryStiffness;
    public float spawnRate = .1f;
    public int beesPerResource;
    [Space(10)]
    public int startResourceCount;

    List<Matrix4x4> matrices;
    Vector2Int gridCounts;
    Vector2 gridSize;
    Vector2 minGridPos;

    public int[,] stackHeights;

    float spawnTimer = 0f;

    public static ResouceEntityManager rinstance;

    private void Awake()
    {

        if (rinstance != null && rinstance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            rinstance = this;
        }
    }

    void Start()
    {
        matrices = new List<Matrix4x4>();
        gridCounts = Vector2Int.RoundToInt(new Vector2(Field.size.x, Field.size.z) / resourceSize);
        gridSize = new Vector2(Field.size.x / gridCounts.x, Field.size.z / gridCounts.y);
        minGridPos = new Vector2((gridCounts.x - 1f) * -.5f * gridSize.x, (gridCounts.y - 1f) * -.5f * gridSize.y);
        stackHeights = new int[gridCounts.x, gridCounts.y];

        EntityManager EM = World.DefaultGameObjectInjectionWorld.EntityManager;

        EntityArchetype resourceArchetype = EM.CreateArchetype(
            typeof(ResourceComponent),
            typeof(RenderMesh),
            typeof(LocalToWorld),
            typeof(Translation),
            typeof(Scale)
            );

        NativeArray<Entity> resources = new NativeArray<Entity>(startResourceCount,Allocator.Temp);

        EM.CreateEntity(resourceArchetype, resources);

        for (int i = 0; i < resources.Length; i++)
        {
            Entity singleRes = resources[i];
            Vector3 pos = new Vector3(minGridPos.x * .25f + UnityEngine.Random.value * Field.size.x * .25f, UnityEngine.Random.value * 10f, minGridPos.y + UnityEngine.Random.value * Field.size.z);
            matrices.Add(Matrix4x4.identity);

            EM.SetComponentData(singleRes, new ResourceComponent
            {
                 
            });

            EM.SetComponentData(singleRes, new Translation
            {
                Value = pos
            });

            EM.SetComponentData(singleRes, new Scale
            {
                Value = resourceSize
            });

            EM.SetSharedComponentData(singleRes, new RenderMesh
            {
                mesh = resourceMesh,
                material = resourceMat
            });
        }

        resources.Dispose();
    }


    public Vector3 GetStackPos(int x, int y, int height)
    {
        return new Vector3(minGridPos.x + x * gridSize.x, -Field.size.y * .5f + (height + .5f) * resourceSize, minGridPos.y + y * gridSize.y);
    }

    public void GetGridIndex(Vector3 pos, out int gridX, out int gridY)
    {
        gridX = Mathf.FloorToInt((pos.x - minGridPos.x + gridSize.x * .5f) / gridSize.x);
        gridY = Mathf.FloorToInt((pos.z - minGridPos.y + gridSize.y * .5f) / gridSize.y);

        gridX = Mathf.Clamp(gridX, 0, gridCounts.x - 1);
        gridY = Mathf.Clamp(gridY, 0, gridCounts.y - 1);
    }

    public Vector3 NearestSnappedPos(Vector3 pos)
    {
        int x, y;
        GetGridIndex(pos, out x, out y);
        return new Vector3(minGridPos.x + x * gridSize.x, pos.y, minGridPos.y + y * gridSize.y);
    }
}
