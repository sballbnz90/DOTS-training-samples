using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Rendering;

public class ResourceComponentSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;
        Entities.ForEach((Entity e, ref ResourceComponent rcomp, ref Translation translation) =>
        {
            if (rcomp.holder != Entity.Null)
            {
                if (EntityManager.HasComponent<BeeDeadandVelocityComponent>(rcomp.holder))
                {
                    if (EntityManager.GetComponentData<BeeDeadandVelocityComponent>(rcomp.holder).dead)
                    {
                        rcomp.holder = Entity.Null;
                    }
                }
            }
            else if (rcomp.stacked == false)
            {
                translation.Value = Vector3.Lerp(translation.Value, ResouceEntityManager.rinstance.NearestSnappedPos(translation.Value), ResouceEntityManager.rinstance.snapStiffness * Time.DeltaTime);
                rcomp.velocity.y = Field.gravity * Time.DeltaTime;
                translation.Value += (float3)rcomp.velocity;
                ResouceEntityManager.rinstance.GetGridIndex(translation.Value, out rcomp.gridX, out rcomp.gridY);
                float floorY = ResouceEntityManager.rinstance.GetStackPos(rcomp.gridX, rcomp.gridY, ResouceEntityManager.rinstance.stackHeights[rcomp.gridX, rcomp.gridY]).y;
                for (int j = 0; j < 3; j++)
                {
                    if (System.Math.Abs(translation.Value[j]) > Field.size[j] * .5f)
                    {
                        translation.Value[j] = Field.size[j] * .5f * Mathf.Sign(translation.Value[j]);
                        rcomp.velocity[j] *= -.5f;
                        rcomp.velocity[(j + 1) % 3] *= .8f;
                        rcomp.velocity[(j + 2) % 3] *= .8f;
                    }
                }
                if (translation.Value.y < floorY)
                {
                    translation.Value.y = floorY;
                    if (Mathf.Abs(translation.Value.x) > Field.size.x * .4f)
                    {
                        int team = 0;
                        if (translation.Value.x > 0f)
                        {
                            team = 1;
                        }
                        //DA FUCK IS THIS HO TOLTO UNA PARTE DOVE UN FOR SPAWNAVA UN ALTRA BEE SE OLTRE UNA POSIZIONE IN X PIU UNA COSA DEL PARTICLE MANAGER

                        EntityManager.RemoveComponent<ResourceComponent>(e);
                        if (!EntityManager.HasComponent<ResourceComponent>(e))
                        {
                            EntityManager.RemoveComponent<RenderMesh>(e);
                            EntityManager.RemoveComponent<Scale>(e);
                            EntityManager.RemoveComponent<LocalToWorld>(e);
                        }
                        else
                        {
                            rcomp.stacked = true;
                            rcomp.stackIndex = ResouceEntityManager.rinstance.stackHeights[rcomp.gridX, rcomp.gridY];
                            if ((rcomp.stackIndex + 1) * ResouceEntityManager.rinstance.resourceSize < Field.size.y)
                            {
                                ResouceEntityManager.rinstance.stackHeights[rcomp.gridX, rcomp.gridY]++;
                            }
                            else
                            {
                                EntityManager.RemoveComponent<ResourceComponent>(e);
                                if (!EntityManager.HasComponent<ResourceComponent>(e))
                                {
                                    EntityManager.RemoveComponent<RenderMesh>(e);
                                    EntityManager.RemoveComponent<Scale>(e);
                                    EntityManager.RemoveComponent<LocalToWorld>(e);
                                }
                            }

                        }

                    }
                }
            }

        });

        
    }
}
