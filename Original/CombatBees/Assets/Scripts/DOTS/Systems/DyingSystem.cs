using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.Jobs;
using Unity.Collections;

public class DyingSystem : JobComponentSystem
{
    private EntityCommandBufferSystem entityCommandBufferSystem;

    protected override void OnCreate()
    {
        entityCommandBufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        EntityCommandBuffer.Concurrent ecb = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();
        float3 fallValue = new float3(0, -5f, 0);
        float delta = Time.DeltaTime;
        float minY = -Field.size.y;

        JobHandle dieJob = Entities
            .ForEach((Entity entity, ref Translation transComp, ref DeathComponent deathComp) =>
            {
                if (deathComp.timeRemaining <= 0)
                {
                    ecb.DestroyEntity(0, entity);
                }

                
                if (transComp.Value.y > minY)
                {
                    transComp.Value += fallValue * delta;
                }

                deathComp.timeRemaining -= delta;

            }).Schedule(inputDeps);

        return dieJob;
    }
}
