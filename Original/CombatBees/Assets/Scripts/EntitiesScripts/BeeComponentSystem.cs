using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Rendering;


public class BeeComponentSystem : ComponentSystem
{
    List<Entity> allies = new List<Entity>();
    List<Entity> enemyTeam = new List<Entity>();
    List<Entity> delete = new List<Entity>();


    protected override void OnUpdate()
    {
        Entities.ForEach((Entity e, ref Translation translation, ref BeeComponent beeComponent, ref BeeDeadandVelocityComponent bdevcomponent) =>
        {
            beeComponent.isAttacking = false;
            beeComponent.isHoldingResource = false;
            float deltaTime = Time.DeltaTime;
            if (bdevcomponent.dead == false)
            {
                bdevcomponent.velocity += UnityEngine.Random.insideUnitSphere * BeeEntityManager.bemInstance.flightJitter * deltaTime;
                bdevcomponent.velocity *= (1 - BeeEntityManager.bemInstance.damping);
                if (beeComponent.team == 0)
                {
                    allies = BeeEntityManager.bemInstance.yellowBeesTeam;
                }
                else
                {
                    allies = BeeEntityManager.bemInstance.blueBeesTeam;
                }

                Entity attractiveFriend = allies[UnityEngine.Random.Range(0, allies.Count)];
                Vector3 delta = EntityManager.GetComponentData<Translation>(attractiveFriend).Value - translation.Value;
                float dist = Mathf.Sqrt(delta.x * delta.x + delta.y * delta.y + delta.z * delta.z);
                if (dist > 0f)
                {
                    bdevcomponent.velocity += delta * (BeeEntityManager.bemInstance.teamAttraction * deltaTime / dist);
                }

                Entity repellentFriend = allies[UnityEngine.Random.Range(0, allies.Count)];
                delta = EntityManager.GetComponentData<Translation>(attractiveFriend).Value - translation.Value;
                dist = Mathf.Sqrt(delta.x * delta.x + delta.y * delta.y + delta.z * delta.z);
                if (dist > 0f)
                {
                    bdevcomponent.velocity -= delta * (BeeEntityManager.bemInstance.teamRepulsion * deltaTime / dist);
                }

                if (beeComponent.enemyTarget == Entity.Null && beeComponent.resourceTarget == Entity.Null)
                {
                    if (UnityEngine.Random.value < BeeEntityManager.bemInstance.aggression)
                    {
                        if (beeComponent.team == 0)
                        {
                            enemyTeam = BeeEntityManager.bemInstance.blueBeesTeam;
                        }
                        else
                        {
                            enemyTeam = BeeEntityManager.bemInstance.yellowBeesTeam;
                        }
                        if (enemyTeam.Count > 0)
                        {
                            int temp = UnityEngine.Random.Range(0, enemyTeam.Count);
                            beeComponent.enemyTarget = enemyTeam[temp];
                        }
                    }
                    else
                    {
                        //da aggiungere la parte delle risorse
                        beeComponent.resourceTarget = Entity.Null;
                    }
                }
                else if (beeComponent.enemyTarget != Entity.Null)
                {
                    if (EntityManager.HasComponent<BeeComponent>(beeComponent.enemyTarget))
                    {
                        if (EntityManager.GetComponentData<BeeDeadandVelocityComponent>(beeComponent.enemyTarget).dead)
                        {
                            beeComponent.enemyTarget = Entity.Null;
                        }
                        else
                        {
                            delta = EntityManager.GetComponentData<Translation>(beeComponent.enemyTarget).Value - translation.Value;
                            float sqrDist = delta.x * delta.x + delta.y * delta.y + delta.z * delta.z;
                            if (sqrDist > BeeEntityManager.bemInstance.attackDistance * BeeEntityManager.bemInstance.attackDistance)
                            {
                                bdevcomponent.velocity += delta * (BeeEntityManager.bemInstance.chaseForce * deltaTime / Mathf.Sqrt(sqrDist));
                            }
                            else
                            {
                                beeComponent.isAttacking = true;
                                bdevcomponent.velocity += delta * (BeeEntityManager.bemInstance.attackForce * deltaTime / Mathf.Sqrt(sqrDist));
                                if (sqrDist < BeeEntityManager.bemInstance.hitDistance * BeeEntityManager.bemInstance.hitDistance)
                                {
                                    //spawn particella da aggiungere come entity
                                    EntityManager.SetComponentData(beeComponent.enemyTarget, new BeeDeadandVelocityComponent
                                    {
                                        dead = true,
                                        velocity = EntityManager.GetComponentData<BeeDeadandVelocityComponent>(beeComponent.enemyTarget).velocity * 0.5f,
                                    });
                                    beeComponent.enemyTarget = Entity.Null;
                                }
                            }
                        }
                    }
                }
                else if (beeComponent.resourceTarget != Entity.Null)
                {
                    //aggiungere tutta la parte di resources
                }
            }
            else
            {
                bdevcomponent.velocity.y += Field.gravity * deltaTime;
                beeComponent.deathTimer -= deltaTime / 10f;
                if (beeComponent.deathTimer < 0)
                {
                    //trovare come distruggere la entity=??????
                    EntityManager.RemoveComponent<BeeDeadandVelocityComponent>(e);
                    if (!EntityManager.HasComponent<BeeDeadandVelocityComponent>(e))
                    {
                        EntityManager.RemoveComponent<RenderMesh>(e);
                        EntityManager.RemoveComponent<Rotation>(e);
                        EntityManager.RemoveComponent<Scale>(e);
                        EntityManager.RemoveComponent<LocalToWorld>(e);
                        EntityManager.RemoveComponent<BeeComponent>(e);
                    }
                    delete = new List<Entity>();
                    delete.Add(e);
                }

            }
            if (EntityManager.HasComponent<BeeDeadandVelocityComponent>(e))
            {
                translation.Value += deltaTime * (float3)bdevcomponent.velocity;
            }

            if (System.Math.Abs(translation.Value.x) > Field.size.x * .5f)
            {
                translation.Value.x = (Field.size.x * .5f) * Mathf.Sign(translation.Value.x);
                bdevcomponent.velocity.x *= -.5f;
                bdevcomponent.velocity.y *= .8f;
                bdevcomponent.velocity.z *= .8f;
            }
            if (System.Math.Abs(translation.Value.z) > Field.size.z * .5f)
            {
                translation.Value.z = (Field.size.z * .5f) * Mathf.Sign(translation.Value.z);
                bdevcomponent.velocity.z *= -.5f;
                bdevcomponent.velocity.x *= .8f;
                bdevcomponent.velocity.y *= .8f;
            }

            for (int i = 0; i < delete.Count; i++)
            {
                if (allies.Contains(delete[i]))
                {
                    allies.Remove(delete[i]);
                }
                if (enemyTeam.Contains(delete[i]))
                {
                    enemyTeam.Remove(delete[i]);
                }
                delete.Clear();
            }

            //}


        });


    }



}
