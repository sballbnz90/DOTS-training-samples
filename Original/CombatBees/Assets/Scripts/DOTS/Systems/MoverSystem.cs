using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Burst;

public class MoverSystem : JobComponentSystem
{
    [BurstCompile]
    private struct MoveJob : IJobForEach<Translation, BeeComponent>
    {
        //public EntityCommandBuffer.Concurrent ecb;
        [ReadOnly] public NativeArray<Entity> teammates;
        public float deltaTime;
        public float beeMoveSpeed;
        public int teamRepulsion;
        public int teamAttraction;

        //non può essere translation perchè non posso leggerci e scriverci in contemporanea
        [ReadOnly] public ComponentDataFromEntity<LocalToWorld> translationData;

        public void Execute(ref Translation c0, ref BeeComponent c1)
        {
            float3 value = new float3(0);
            int attractor = c1.randomGenerator.NextInt(0, teammates.Length - 1);
            int repulsor = c1.randomGenerator.NextInt(0, teammates.Length - 1);

            value += math.normalizesafe(translationData[teammates[attractor]].Position - c0.Value, 0.0f) * beeMoveSpeed * deltaTime * teamAttraction;
            value -= math.normalizesafe(translationData[teammates[repulsor]].Position - c0.Value, 0.0f) * beeMoveSpeed * deltaTime * teamRepulsion;

            c0.Value += value * deltaTime;
        }
    }


    private EntityQuery yellowBees;
    private EntityQuery blueBees;
    private EntityCommandBufferSystem m_entityCommandBufferSystem;


    protected override void OnCreate()
    {
        m_entityCommandBufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();

        yellowBees = GetEntityQuery(typeof(Translation), typeof(BeeComponent), typeof(TeamYellow));
        blueBees = GetEntityQuery(typeof(Translation), typeof(BeeComponent), typeof(TeamBlue));
    }

    NativeArray<Entity> teamYellow;
    NativeArray<Entity> teamBlue;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var dep = inputDeps;
        ComponentDataFromEntity<LocalToWorld> trData = GetComponentDataFromEntity<LocalToWorld>(true);

        if (teamYellow.IsCreated)
        {
            teamYellow.Dispose();
        }
        teamYellow = yellowBees.ToEntityArray(Allocator.TempJob);
        var yellowJob = new MoveJob
        {
            // ecb = m_entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
            //positionArray = BeeManagerDOTS.Instance.positionArray,
            teammates = teamYellow,
            deltaTime = Time.deltaTime,
            beeMoveSpeed = BeeManagerDOTS.Instance.beeMoveSpeed,
            //attractor = UnityEngine.Random.Range(0, BeeManagerDOTS.Instance.actualYellow),
            //repulsor = UnityEngine.Random.Range(0, BeeManagerDOTS.Instance.actualYellow),
            translationData = trData,
            teamRepulsion = BeeManagerDOTS.Instance.teamRepulsion,
            teamAttraction = BeeManagerDOTS.Instance.teamAttraction,

        }.Schedule(yellowBees, dep);

        dep = yellowJob;

        if (teamBlue.IsCreated)
        {
            teamBlue.Dispose();
        }
        teamBlue = blueBees.ToEntityArray(Allocator.TempJob);
        var blueJob = new MoveJob
        {
            teammates = teamBlue,
            deltaTime = Time.deltaTime,
            beeMoveSpeed = BeeManagerDOTS.Instance.beeMoveSpeed * 2,
            //attractor = UnityEngine.Random.Range(0, BeeManagerDOTS.Instance.actualBlue),
            //repulsor = UnityEngine.Random.Range(0, BeeManagerDOTS.Instance.actualBlue),
            translationData = trData,
            teamRepulsion = BeeManagerDOTS.Instance.teamRepulsion,
            teamAttraction = BeeManagerDOTS.Instance.teamAttraction,

        }.Schedule(blueBees, dep);

        // m_entityCommandBufferSystem.AddJobHandleForProducer(moverJob);
        dep = blueJob;

        return dep;
        //return moverJob.Schedule(yellowBees);
    }


    protected override void OnDestroy()
    {
        base.OnDestroy();

        teamYellow.Dispose();
        teamBlue.Dispose();
    }
}
