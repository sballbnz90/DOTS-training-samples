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

        public ArchetypeChunkComponentType<Translation> TransType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var chunkTranslations = chunk.GetNativeArray(TransType);
            for (int i = 0; i < chunk.Count; i++)
            {
                var target = enemyBees[i];
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
        teamBlue = yellowBees.ToEntityArray(Allocator.TempJob);

        var jobHandle = new ChaseBeeJob
        {
            enemyBees = teamBlue,
            deltaTime = Time.deltaTime,
            chaseSpeed = BeeManagerDOTS.Instance.beeMoveSpeed,
            TransType = GetArchetypeChunkComponentType<Translation>()
        }.Schedule(yellowBees, dep);
        dep = jobHandle;

        jobHandle = new ChaseBeeJob
        {
            enemyBees = teamYellow,
            deltaTime = Time.deltaTime,
            chaseSpeed = BeeManagerDOTS.Instance.beeMoveSpeed,
            TransType = GetArchetypeChunkComponentType<Translation>()
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
