using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.Jobs;
using Unity.Collections;

public class MoverSystem : JobComponentSystem
{
    public NativeArray<float> moveYArray;
    public float deltaTime;
    public MoveSpeedComponent moveData;

    private struct MoveJob : IJobParallelForTransform
    {
        public void Execute(int index, TransformAccess transform)
        {
            transform.position
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        
    }
}
