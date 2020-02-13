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
            float3 value = c0.Value;
            int attractor = c1.randomGenerator.NextInt(0, teammates.Length - 1);
            int repulsor = c1.randomGenerator.NextInt(0, teammates.Length - 1);

            
           // value += baseVel  * (beeMoveSpeed * deltaTime);
            value += (translationData[teammates[attractor]].Position - c0.Value) * deltaTime;
            value -= (translationData[teammates[repulsor]].Position - c0.Value) * deltaTime;
            //value -= math.normalize(c1.home) * beeMoveSpeed * deltaTime;
            //value += (translationData[teammates[attractor]].Position - c0.Value) * (teamAttraction * deltaTime / math.distance(translationData[teammates[attractor]].Position, c0.Value));
            //value -= (translationData[teammates[repulsor]].Position - c0.Value) * (teamRepulsion * deltaTime / math.distance(translationData[teammates[repulsor]].Position, c0.Value));

            c0.Value = value;
        }
    }


    private EntityQuery yellowBees;
    private EntityQuery blueBees;
    private EntityCommandBufferSystem m_entityCommandBufferSystem;


    protected override void OnCreate()
    {
        m_entityCommandBufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();

        yellowBees = GetEntityQuery(typeof(Translation), typeof(BeeComponent), typeof(TeamComponent));
        blueBees = GetEntityQuery(typeof(Translation), typeof(BeeComponent), typeof(TeamComponent));

    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var dep = inputDeps;
        ComponentDataFromEntity<LocalToWorld> trData = GetComponentDataFromEntity<LocalToWorld>(true);

        if (BeeManagerDOTS.Instance.yellowArray.Length != 0)
        {


            yellowBees.SetFilter(new TeamComponent() { team = 2 });

            var yellowJob = new MoveJob
            {
                // ecb = m_entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                //positionArray = BeeManagerDOTS.Instance.positionArray,
                teammates = BeeManagerDOTS.Instance.yellowArray,
                deltaTime = Time.deltaTime,
                beeMoveSpeed = BeeManagerDOTS.Instance.beeMoveSpeed,
                //attractor = UnityEngine.Random.Range(0, BeeManagerDOTS.Instance.actualYellow),
                //repulsor = UnityEngine.Random.Range(0, BeeManagerDOTS.Instance.actualYellow),
                translationData = trData,
                teamRepulsion = BeeManagerDOTS.Instance.teamRepulsion,
                teamAttraction = BeeManagerDOTS.Instance.teamAttraction,
                
            }.Schedule(yellowBees, dep);

            dep = yellowJob;

            blueBees.SetFilter(new TeamComponent() { team = 1 });

            var blueJob = new MoveJob
            {
                teammates = BeeManagerDOTS.Instance.blueArray,
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
        }

        return dep;
        //return moverJob.Schedule(yellowBees);

    }
}
