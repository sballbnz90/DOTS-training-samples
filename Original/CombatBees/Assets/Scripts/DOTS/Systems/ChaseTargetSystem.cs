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
        public float deltaTime;
        public float chaseSpeed;
        public float attackRange;
        public float beeTimeToDeath;

        [ReadOnly] public NativeArray<Entity> enemyBees;
        [ReadOnly] public ComponentDataFromEntity<LocalToWorld> translationData;
        [ReadOnly] public ComponentDataFromEntity<DeathComponent> deathData;
        [ReadOnly] public ArchetypeChunkComponentType<TargetComponent> targetType;
        public ArchetypeChunkComponentType<Translation> transType;
        [ReadOnly] public ArchetypeChunkEntityType entityType;

        public EntityCommandBuffer.Concurrent ecb;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            Unity.Mathematics.Random rand = new Unity.Mathematics.Random(1);
            var chunkTrans = chunk.GetNativeArray(transType);
            var chunkTargets = chunk.GetNativeArray(targetType);
            var entities = chunk.GetNativeArray(entityType);

            for (int i = 0; i < chunk.Count; i++)
            {
                if (chunkTargets[i].type == TargetTypes.EnemyBee)
                {
                    if (deathData.Exists(chunkTargets[i].target))
                    {
                        ecb.RemoveComponent(chunkIndex, entities[i], typeof(TargetComponent));
                        continue;
                    }

                    float3 myPos = chunkTrans[i].Value;
                    float3 targetPos = translationData[chunkTargets[i].target].Position;
                    float3 delta = targetPos - myPos;
                    float distanceSquared = delta.x * delta.x + delta.y * delta.y + delta.z + delta.z;

                    if (distanceSquared < attackRange * attackRange)
                    {
                        DeathComponent death = new DeathComponent()
                        {
                            timeRemaining = beeTimeToDeath
                        };

                        ecb.AddComponent(chunkIndex, chunkTargets[i].target, death);
                        ecb.RemoveComponent(chunkIndex, entities[i], typeof(TargetComponent));
                        
                    }

                    float3 value = new float3(0);
                    value += math.normalizesafe(targetPos - myPos) * deltaTime * chaseSpeed;

                    chunkTrans[i] = new Translation
                    {
                        Value = myPos + value
                    };

                }
            }
        }
    }

    private EntityQuery enemyChase;
    private EntityCommandBufferSystem entityCommandBufferSystem;


    protected override void OnCreate()
    {
        entityCommandBufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();

        yellowBees = GetEntityQuery(typeof(Translation), typeof(BeeComponent), typeof(TeamYellow), typeof(TargetComponent));
        blueBees = GetEntityQuery(typeof(Translation), typeof(BeeComponent), typeof(TeamBlue), typeof(TargetComponent));
    }

    private EntityQuery yellowBees;
    private EntityQuery blueBees;
    NativeArray<Entity> teamYellow;
    NativeArray<Entity> teamBlue;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        ComponentDataFromEntity<LocalToWorld> trData = GetComponentDataFromEntity<LocalToWorld>(true);
        JobHandle dep = inputDeps;

        if (teamYellow.IsCreated)
        {
            teamYellow.Dispose();
        }
        teamYellow = yellowBees.ToEntityArray(Allocator.TempJob);

        if (teamBlue.IsCreated)
        {
            teamBlue.Dispose();
        }
        teamBlue = blueBees.ToEntityArray(Allocator.TempJob);

        var jobHandle = new ChaseBeeJob
        {
            enemyBees = teamBlue,
            deltaTime = Time.DeltaTime,
            attackRange = BeeManagerDOTS.Instance.attackRange,
            chaseSpeed = BeeManagerDOTS.Instance.beeChaseSpeed,
            beeTimeToDeath = BeeManagerDOTS.Instance.timeToDeath,
            translationData = GetComponentDataFromEntity<LocalToWorld>(),
            deathData = GetComponentDataFromEntity<DeathComponent>(),
            targetType = GetArchetypeChunkComponentType<TargetComponent>(),
            transType = GetArchetypeChunkComponentType<Translation>(),
            entityType = GetArchetypeChunkEntityType(),
            ecb = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
        }.Schedule(yellowBees, dep);


        dep = jobHandle;

        jobHandle = new ChaseBeeJob
        {
            enemyBees = teamYellow,
            deltaTime = Time.DeltaTime,
            attackRange = BeeManagerDOTS.Instance.attackRange,
            chaseSpeed = BeeManagerDOTS.Instance.beeChaseSpeed,
            beeTimeToDeath = BeeManagerDOTS.Instance.timeToDeath,
            translationData = GetComponentDataFromEntity<LocalToWorld>(),
            deathData = GetComponentDataFromEntity<DeathComponent>(),
            targetType = GetArchetypeChunkComponentType<TargetComponent>(),
            transType = GetArchetypeChunkComponentType<Translation>(),
            entityType = GetArchetypeChunkEntityType(),
            ecb = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
        }.Schedule(blueBees, dep);


        dep = jobHandle;



        return dep;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        //yellowBees.Dispose();
        //blueBees.Dispose();

        teamYellow.Dispose();
        teamBlue.Dispose();
    }
}
