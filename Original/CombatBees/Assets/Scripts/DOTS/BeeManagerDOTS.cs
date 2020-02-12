using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;     //mi serve per il component Translation
using Unity.Rendering;      //mi serve per il component RenderMesh
using Unity.Collections;    //mi serve per i NativeArray
using Unity.Mathematics;


public class BeeManagerDOTS : MonoBehaviour
{
    [Header("GameStats")]
    [SerializeField] private int startBeeCount = 50;

    [Header("Graphics")]
    [SerializeField] private Mesh beeMesh;
    [SerializeField] private Material blueMaterial, yellowMaterial;

    [Header("BeeStats")]
    private float beeMoveSpeed = 5;

    EntityManager entityManager;
    EntityArchetype blueArchetype, yellowArchetype;

    private void Start()
    {
        //classe che gestisce le entities
        entityManager = World.Active.EntityManager;

        //creo l'archetipo
        blueArchetype = entityManager.CreateArchetype(typeof(Translation), typeof(RenderMesh), typeof(LocalToWorld), typeof(MoveSpeedComponent));
        yellowArchetype = entityManager.CreateArchetype(typeof(Translation), typeof(RenderMesh), typeof(LocalToWorld), typeof(MoveSpeedComponent));

        //uso un array nativo per allocare le entities
        NativeArray<Entity> blueArray = new NativeArray<Entity>(startBeeCount / 2, Allocator.Temp);
        NativeArray<Entity> yellowArray = new NativeArray<Entity>(startBeeCount / 2, Allocator.Temp);

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

            entityManager.SetSharedComponentData(entity, new MoveSpeedComponent { moveSpeed = beeMoveSpeed });
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

            entityManager.SetSharedComponentData(entity, new MoveSpeedComponent { moveSpeed = beeMoveSpeed });
        }

        blueArray.Dispose();
    }
}
