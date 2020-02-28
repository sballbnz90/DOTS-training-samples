using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;     //mi serve per il component Translation
using Unity.Rendering;      //mi serve per il component RenderMesh
using Unity.Collections;    //mi serve per i NativeArray
using Unity.Mathematics;
using UnityEngine.Jobs;

public class BeeManagerDOTS : MonoBehaviour
{
    public static BeeManagerDOTS Instance { get; private set; }

    [Header("GameStats")]
    [SerializeField] private int startBeeCount = 50;
    public int actualYellow;
    public int actualBlue;

    [Header("Graphics")]
    [SerializeField] private Mesh beeMesh;
    [SerializeField] private Material blueMaterial, yellowMaterial;

    [Header("BeeStats")]
    //public NativeArray<float3> positionArray;
    public int teamRepulsion = 10;
    public int teamAttraction = 10;
    public float beeMoveSpeed = 5;
    public float beeChaseSpeed = 10;


    public EntityManager entityManager;
    EntityArchetype blueArchetype, yellowArchetype;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        //classe che gestisce le entities
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        //creo l'archetipo
        blueArchetype = entityManager.CreateArchetype(typeof(Translation), typeof(RenderMesh), typeof(LocalToWorld), typeof(BeeComponent), typeof(TeamBlue),
            typeof(ChunkWorldRenderBounds), typeof(PerInstanceCullingTag), typeof(RenderBounds), typeof(Rotation), typeof(WorldRenderBounds));
        yellowArchetype = entityManager.CreateArchetype(typeof(Translation), typeof(RenderMesh), typeof(LocalToWorld), typeof(BeeComponent), typeof(TeamYellow),
            typeof(ChunkWorldRenderBounds), typeof(PerInstanceCullingTag), typeof(RenderBounds), typeof(Rotation), typeof(WorldRenderBounds));

        for (int i = 0; i < startBeeCount / 2; i++)
        {
            Entity entity = entityManager.CreateEntity(blueArchetype);

            entityManager.SetComponentData(entity, new Translation { Value = new float3(UnityEngine.Random.Range(-Field.size.x / 2, 0), UnityEngine.Random.Range(-Field.size.y / 2, Field.size.y / 2), UnityEngine.Random.Range(-Field.size.z / 2, Field.size.z / 2)) });
            entityManager.SetSharedComponentData(entity, new RenderMesh
            {
                mesh = beeMesh,
                material = blueMaterial,
            });

            //positionArray[i] = entityManager.GetComponentData<Translation>(entity).Value;

            entityManager.SetComponentData(entity, new BeeComponent
            {
                team = 1,
                home = new float3(-Field.size.x, 0, 0),
                randomGenerator = new Unity.Mathematics.Random((uint)i + 1)
            });

            //entityManager.SetSharedComponentData(entity, new MoveSpeedComponent { moveSpeed = beeMoveSpeed });

            entityManager.SetSharedComponentData(entity, new TeamBlue());

            actualBlue++;

            // Yellow
            entity = entityManager.CreateEntity(yellowArchetype);

            entityManager.SetComponentData(entity, new Translation { Value = new float3(UnityEngine.Random.Range(0, Field.size.x / 2), UnityEngine.Random.Range(-Field.size.y / 2, Field.size.y / 2), UnityEngine.Random.Range(-Field.size.z / 2, Field.size.z / 2)) });

            entityManager.SetSharedComponentData(entity, new RenderMesh
            {
                mesh = beeMesh,
                material = yellowMaterial,
            });


            //positionArray[i + blueArray.Length] = entityManager.GetComponentData<Translation>(entity).Value;
            //entityManager.SetSharedComponentData(entity, new MoveSpeedComponent { moveSpeed = beeMoveSpeed });

            entityManager.SetComponentData(entity, new BeeComponent
            {
                team = 2,
                home = new float3(Field.size.x, 0, 0),
                randomGenerator = new Unity.Mathematics.Random((uint)i + 1)
            });

            entityManager.SetSharedComponentData(entity, new TeamYellow());

            actualYellow++;
        }
    }
}
