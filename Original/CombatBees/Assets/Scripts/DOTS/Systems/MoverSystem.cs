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
    private struct MoveJob : IJobForEach<Translation>
    {
        public EntityCommandBuffer.Concurrent ecb;
        public NativeArray<float3> positionArray;
        //public NativeArray<float> moveArray;
        public float deltaTime;
        public float beeMoveSpeed;


        //public void Execute(int index)
        //{
        //    positionArray[index] += new float3(1, 0, 0) * beeMoveSpeed * deltaTime;
        //}

        public void Execute(ref Translation c0)
        {
            c0.Value += new float3(1, 0, 0) * beeMoveSpeed * deltaTime;

            if(c0.Value.x > 10 || c0.Value.x < -10)
            {
                beeMoveSpeed *= -1;
            }
        }
    }


    private EntityQuery yellowBees;
    private EntityQuery blueBees;
    private EntityCommandBufferSystem m_entityCommandBufferSystem;


    protected override void OnCreate()
    {
        m_entityCommandBufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();

        yellowBees = GetEntityQuery(typeof(Translation));
        blueBees = GetEntityQuery(typeof(Translation));

    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        

        var moverJob = new MoveJob
        {
            positionArray = BeeManagerDOTS.Instance.positionArray,
            deltaTime = Time.deltaTime,
            ecb = m_entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
            beeMoveSpeed = BeeManagerDOTS.Instance.beeMoveSpeed

        };//.Schedule(BeeManagerDOTS.Instance.positionArray.Length, 1000);


        // m_entityCommandBufferSystem.AddJobHandleForProducer(moverJob);


        return moverJob.Schedule(yellowBees);

    }
}
