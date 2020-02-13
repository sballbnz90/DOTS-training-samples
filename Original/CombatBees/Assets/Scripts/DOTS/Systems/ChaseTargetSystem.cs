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

public class ChaseTargetSystem : JobComponentSystem
{
    private struct ChaseBeeJob : IJobChunk
    {
        [ReadOnly] public NativeArray<Entity> enemyBees;
        public float deltaTime;
        public float chaseSpeed;
        public ArchetypeChunkComponentType<BeeComponent> beeType;
        public ArchetypeChunkComponentType<Translation> translations;
        //public ArchetypeChunkComponentType<TeamComponent> teamType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var teams = chunk.GetNativeArray(beeType);

            for (int i = 0; i < chunk.Count; i++)
            {
                



                var target = enemyBees[i];
            }

            throw new System.NotImplementedException();
        }
    }

    private EntityQuery enemyChase;
    private EntityCommandBufferSystem m_entityCommandBufferSystem;


    protected override void OnCreate()
    {
        m_entityCommandBufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();

        enemyChase = GetEntityQuery(typeof(Translation), typeof(BeeComponent), typeof(TeamComponent));

    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        



        throw new System.NotImplementedException();
    }
}
