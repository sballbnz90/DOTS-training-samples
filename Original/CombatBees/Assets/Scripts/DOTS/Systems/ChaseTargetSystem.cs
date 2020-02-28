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
        [ReadOnly] public ComponentDataFromEntity<LocalToWorld> translationData;
        public ArchetypeChunkComponentType<Translation> TransType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var chunkTranslations = chunk.GetNativeArray(TransType);
            Unity.Mathematics.Random rand = new Unity.Mathematics.Random(1);
            var chunkTrans = chunk.GetNativeArray(TransType);

            for (int i = 0; i < chunk.Count; i++)
            {
                var target = rand.NextInt(0, enemyBees.Length);
                float3 targetPos = new float3(0);
                float3 myPos = chunkTrans[i].Value;

                if (translationData.Exists(enemyBees[target]))
                {
                    targetPos = translationData[enemyBees[target]].Position;
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

    private EntityQuery enemyChase;

    protected override void OnCreate()
    {
        yellowBees = GetEntityQuery(typeof(Translation), typeof(BeeComponent), typeof(TeamYellow));
        blueBees = GetEntityQuery(typeof(Translation), typeof(BeeComponent), typeof(TeamBlue));
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
            chaseSpeed = BeeManagerDOTS.Instance.beeChaseSpeed,
            TransType = GetArchetypeChunkComponentType<Translation>(),
            translationData = GetComponentDataFromEntity<LocalToWorld>()
        }.Schedule(yellowBees, dep);
        dep = jobHandle;

        jobHandle = new ChaseBeeJob
        {
            enemyBees = teamYellow,
            deltaTime = Time.DeltaTime,
            chaseSpeed = BeeManagerDOTS.Instance.beeChaseSpeed,
            TransType = GetArchetypeChunkComponentType<Translation>(),
            translationData = GetComponentDataFromEntity<LocalToWorld>()
        }.Schedule(blueBees, dep);
        dep = jobHandle;

        return dep;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        teamYellow.Dispose();
        teamBlue.Dispose();
    }
}
