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

    [Header("Graphics")]
    [SerializeField] private Mesh beeMesh;
    [SerializeField] private Material blueMaterial, yellowMaterial;

    [Header("BeeStats")]
    public NativeArray<float3> positionArray;
    public float beeMoveSpeed = 5;
   // public float BeeMoveSpeed { get { return beeMoveSpeed; } }

    EntityManager entityManager;
    EntityArchetype blueArchetype, yellowArchetype;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        //classe che gestisce le entities
        entityManager = World.Active.EntityManager;

        //creo l'archetipo
        blueArchetype = entityManager.CreateArchetype(typeof(Translation), typeof(RenderMesh), typeof(LocalToWorld), typeof(BeeComponent));
        yellowArchetype = entityManager.CreateArchetype(typeof(Translation), typeof(RenderMesh), typeof(LocalToWorld), typeof(BeeComponent));

        //uso un array nativo per allocare le entities
        NativeArray<Entity> blueArray = new NativeArray<Entity>(startBeeCount / 2, Allocator.Temp);
        NativeArray<Entity> yellowArray = new NativeArray<Entity>(startBeeCount / 2, Allocator.Temp);

        positionArray = new NativeArray<float3>(startBeeCount, Allocator.Persistent);

        entityManager.CreateEntity(blueArchetype, blueArray);
        entityManager.CreateEntity(yellowArchetype, yellowArray);

        for (int i = 0; i < blueArray.Length; i++)
        {
            Entity entity = blueArray[i];

            entityManager.SetComponentData(entity, new Translation { Value = new float3(UnityEngine.Random.Range(-Field.size.x / 2, 0), UnityEngine.Random.Range(-Field.size.y / 2, Field.size.y / 2), UnityEngine.Random.Range(-Field.size.z / 2, Field.size.z / 2)) });

            entityManager.SetSharedComponentData(entity, new RenderMesh
            {
                mesh = beeMesh,
                material = blueMaterial,
            });

            positionArray[i] = entityManager.GetComponentData<Translation>(entity).Value;

            entityManager.SetComponentData(entity, new BeeComponent
            {
                team = 0,
                home = new float3(-Field.size.x, 0, 0)
            });
            //entityManager.SetSharedComponentData(entity, new MoveSpeedComponent { moveSpeed = beeMoveSpeed });
        }

        for (int i = 0; i < yellowArray.Length; i++)
        {
            Entity entity = yellowArray[i];

            entityManager.SetComponentData(entity, new Translation { Value = new float3(UnityEngine.Random.Range(0, Field.size.x / 2), UnityEngine.Random.Range(-Field.size.y / 2, Field.size.y / 2), UnityEngine.Random.Range(-Field.size.z / 2, Field.size.z / 2)) });

            entityManager.SetSharedComponentData(entity, new RenderMesh
            {
                mesh = beeMesh,
                material = yellowMaterial,
            });


            positionArray[i + blueArray.Length] = entityManager.GetComponentData<Translation>(entity).Value;
            //entityManager.SetSharedComponentData(entity, new MoveSpeedComponent { moveSpeed = beeMoveSpeed });

            entityManager.SetComponentData(entity, new BeeComponent
            {
                team = 1,
                home = new float3(Field.size.x, 0, 0)
            });
        }

        blueArray.Dispose();
        yellowArray.Dispose();
    }
}
