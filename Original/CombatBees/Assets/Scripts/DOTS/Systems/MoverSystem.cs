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
    private struct MoveJob : IJobParallelFor
    {
        public EntityCommandBuffer.Concurrent ecb;
        public NativeArray<float3> positionArray;
        //public NativeArray<float> moveArray;
        public float deltaTime;


        public void Execute(int index)
        {
            positionArray[index] += new float3(1, 0, 0) * BeeManagerDOTS.Instance.beeMoveSpeed * deltaTime;
        }
    }


    private EntityQuery beesToMove;
    private EntityCommandBufferSystem m_entityCommandBufferSystem;


    protected override void OnCreate()
    {
        m_entityCommandBufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();

        beesToMove = GetEntityQuery(typeof(Translation));

    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var moverJob = new MoveJob
        {
            positionArray = BeeManagerDOTS.Instance.positionArray,
            deltaTime = Time.deltaTime,
            ecb = m_entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
        }.Schedule(BeeManagerDOTS.Instance.positionArray.Length, 1000);


        m_entityCommandBufferSystem.AddJobHandleForProducer(moverJob);

        
        return moverJob;

    }
}
