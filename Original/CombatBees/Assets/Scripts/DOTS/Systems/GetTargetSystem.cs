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

public class GetTargetSystem : JobComponentSystem
{
    private EntityCommandBufferSystem entityCommandBufferSystem;
    private EntityQuery yellowBees;
    private EntityQuery blueBees;

    [ReadOnly] NativeArray<Entity> yellowTargets;
    [ReadOnly] NativeArray<Entity> blueTargets;

    protected override void OnCreate()
    {
        entityCommandBufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();
        yellowBees = GetEntityQuery(typeof(Translation), typeof(BeeComponent), typeof(TeamYellow)/*, typeof(TargetComponent)*/);
        blueBees = GetEntityQuery(typeof(Translation), typeof(BeeComponent), typeof(TeamBlue)/*, typeof(TargetComponent)*/);
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        yellowTargets = blueBees.ToEntityArray(Allocator.TempJob);
        blueTargets = yellowBees.ToEntityArray(Allocator.TempJob);
        EntityCommandBuffer.Concurrent ecb = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();
        var dep = inputDeps;
        int ecbCount = 0;
        float aggroTreshold = BeeManagerDOTS.Instance.aggressivenessTreshold;
        NativeArray<Entity> yellowEnemyBees = new NativeArray<Entity>(yellowTargets, Allocator.TempJob);
        NativeArray<Entity> blueEnemyBees = new NativeArray<Entity>(blueTargets, Allocator.TempJob);

        //yellowEnemyBees = yellowTargets;
        //blueEnemyBees = blueTargets;
        //NativeArray<Entity>.Copy(yellowTargets, yellowEnemyBees);
        //NativeArray<Entity>.Copy(blueTargets, blueEnemyBees);



        JobHandle yellowJob = Entities.WithNone<TargetComponent>()
            .WithAll<TeamYellow>()
            .WithNone<DeathComponent>()
            //.WithoutBurst()
            .ForEach((Entity entity, in BeeComponent beeComp) =>
            {

                if (beeComp.aggressiveness > aggroTreshold && yellowEnemyBees.Length > 0)
                {
                    TargetComponent target = new TargetComponent();
                    target.target = yellowEnemyBees[beeComp.randomGenerator.NextInt(0, yellowEnemyBees.Length)];
                    target.type = TargetTypes.EnemyBee;
                    ecb.AddComponent(ecbCount, entity, target);
                }
                else
                {
                    TargetComponent target = new TargetComponent();
                    target.type = TargetTypes.Resource;
                    ecb.AddComponent(ecbCount, entity, target);
                }

            }).Schedule(dep);

        dep = yellowJob;

        JobHandle blueJob = Entities.WithNone<TargetComponent>()
            .WithAll<TeamBlue>()
            .WithNone<DeathComponent>()
            // .WithoutBurst()
            .ForEach((Entity entity, in BeeComponent beeComp) =>
            {
                //NativeArray<Entity> enemyBees = new NativeArray<Entity>();

                if (beeComp.aggressiveness > aggroTreshold && blueEnemyBees.Length > 0)
                {

                    TargetComponent target = new TargetComponent();
                    target.type = TargetTypes.EnemyBee;
                    target.target = blueEnemyBees[beeComp.randomGenerator.NextInt(0, blueEnemyBees.Length)];

                    ecb.AddComponent(ecbCount, entity, target);
                }
                else
                {
                    TargetComponent target = new TargetComponent();
                    target.type = TargetTypes.Resource;

                    ecb.AddComponent(ecbCount, entity, target);
                }


            }).Schedule(dep);

        dep = blueJob;

        dep.Complete();

        yellowTargets.Dispose();
        blueTargets.Dispose();

        return dep;
    }
}
