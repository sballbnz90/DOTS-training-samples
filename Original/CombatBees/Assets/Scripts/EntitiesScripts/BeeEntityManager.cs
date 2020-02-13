using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Rendering;
using Unity.Transforms;
using System;
using System.Collections.Generic;

public class BeeEntityManager : MonoBehaviour
{
    [Header("Meshes and Mats")]
    public Mesh beeMesh;
    public Material yMat;
    public Material bMat;

    [Header("Stats and Counters")]
    public int initBeeCount = 2;
    public float minBeeSize;
    public float maxBeeSize;
    public float maxSpawnSpeed;
    public float flightJitter;
    public float damping;

    public List<Entity> yellowBeesTeam = new List<Entity>();
    public List<Entity> blueBeesTeam = new List<Entity>();
    public float teamAttraction;
    public float teamRepulsion;
    public float aggression;
    public float grabDistance;
    public float carryForce;
    public float attackDistance;
    public float chaseForce;
    public float attackForce;
    public float hitDistance;


    public static BeeEntityManager bemInstance { get; private set; }

    private void Awake()
    {

        if (bemInstance != null && bemInstance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            bemInstance = this;
        }
    }

    void Start()
    {
        EntityManager EM = World.DefaultGameObjectInjectionWorld.EntityManager;


        EntityArchetype beeArchetype = EM.CreateArchetype(
            typeof(BeeComponent),
            typeof(BeeDeadandVelocityComponent),
            typeof(RenderMesh),
            typeof(LocalToWorld),
            typeof(Translation), 
            typeof(Rotation),
            typeof(Scale)
            );

        NativeArray<Entity> yellowBeeArray = new NativeArray<Entity>(initBeeCount, Allocator.Temp);
        NativeArray<Entity> blueBeeArray = new NativeArray<Entity>(initBeeCount, Allocator.Temp);

        EM.CreateEntity(beeArchetype, yellowBeeArray);
        EM.CreateEntity(beeArchetype, blueBeeArray);

        for (int i = 0; i < yellowBeeArray.Length; i++)
        {
            Entity yBee = yellowBeeArray[i];
            yellowBeesTeam.Add(yBee);
            Vector3 pos = Vector3.right * (-Field.size.x * .4f + Field.size.x * .8f);
            EM.SetComponentData(yBee, new BeeComponent
            {
                team = 0,
                enemyTarget = Entity.Null,
                imTargetof = Entity.Null,
                resourceTarget = Entity.Null,
                deathTimer = 0.2f
            });

            EM.SetComponentData(yBee, new BeeDeadandVelocityComponent
            {
                velocity = UnityEngine.Random.insideUnitSphere * maxSpawnSpeed,
            });

            EM.SetComponentData(yBee, new Translation
            {
                Value = pos
            });

            EM.SetComponentData(yBee, new Rotation
            {
                Value = Quaternion.Euler(new Vector3(0, 0, 90))
            });

            EM.SetComponentData(yBee, new Scale
            {
                Value = UnityEngine.Random.Range(minBeeSize, maxBeeSize)
            });

            EM.SetSharedComponentData(yBee, new RenderMesh
            {
                mesh = beeMesh,
                material = yMat
            });
        }

        for (int i = 0; i < blueBeeArray.Length; i++)
        {
            Entity bBee = blueBeeArray[i];
            blueBeesTeam.Add(bBee);
            Vector3 pos = Vector3.right * (-Field.size.x * .4f);
            EM.SetComponentData(bBee, new BeeComponent
            { 
                team = 1,
                enemyTarget = Entity.Null,
                imTargetof = Entity.Null,
                resourceTarget = Entity.Null,
                deathTimer = 0.2f
            });

            EM.SetComponentData(bBee, new BeeDeadandVelocityComponent
            {
                velocity = UnityEngine.Random.insideUnitSphere * maxSpawnSpeed,
            });

            EM.SetComponentData(bBee, new Translation
            {
                Value = pos
            });

            EM.SetComponentData(bBee, new Rotation
            {
                Value = Quaternion.Euler(new Vector3(0, 0, 90))
            });

            EM.SetComponentData(bBee, new Scale
            {
                Value = UnityEngine.Random.Range(minBeeSize, maxBeeSize)
            });

            EM.SetSharedComponentData(bBee, new RenderMesh
            {
                mesh = beeMesh,
                material = bMat
            });
        }

        yellowBeeArray.Dispose();
        blueBeeArray.Dispose();
    }

}
