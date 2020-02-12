using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;


public class BeeComponentSystem : ComponentSystem
{
    [NativeDisableParallelForRestriction] public ComponentDataFromEntity<BeeComponent> trans;
    protected override void OnUpdate()
    {
        Entities.ForEach((ref Translation translation, ref BeeComponent beeComponent) =>
        {
            beeComponent.isAttacking = false;
            beeComponent.isHoldingResource = false;
            float deltaTime = Time.DeltaTime;
            if (beeComponent.dead == false)
            {
                beeComponent.velocity += UnityEngine.Random.insideUnitSphere * BeeEntityManager.bemInstance.flightJitter * deltaTime;
                beeComponent.velocity *= (1-BeeEntityManager.bemInstance.damping);
                NativeList<Entity> allies = new NativeList<Entity>();
                if (beeComponent.team == 0)
                {
                    allies = BeeEntityManager.bemInstance.yellowBeesTeam;
                }
                else
                {
                    allies = BeeEntityManager.bemInstance.yellowBeesTeam;
                }

                Entity attractiveFriend = allies[UnityEngine.Random.Range(0, allies.Length)];
                Vector3 delta = EntityManager.GetComponentData<Translation>(attractiveFriend).Value - translation.Value;
                float dist = Mathf.Sqrt(delta.x * delta.x + delta.y * delta.y + delta.z * delta.z);
                if(dist > 0f)
                {
                    beeComponent.velocity += delta * (BeeEntityManager.bemInstance.teamAttraction * deltaTime / dist);
                }

                Entity repellentFriend = allies[UnityEngine.Random.Range(0, allies.Length)];
                delta = EntityManager.GetComponentData<Translation>(attractiveFriend).Value - translation.Value;
                dist = Mathf.Sqrt(delta.x * delta.x + delta.y * delta.y + delta.z * delta.z);
                if (dist > 0f)
                {
                    beeComponent.velocity -= delta * (BeeEntityManager.bemInstance.teamRepulsion * deltaTime / dist);
                }

                if(beeComponent.enemyTarget == Entity.Null && beeComponent.resourceTarget == Entity.Null)
                {
                    if(UnityEngine.Random.value < BeeEntityManager.bemInstance.aggression)
                    {
                        NativeList<Entity> enemyTeam = new NativeList<Entity>();
                        if (beeComponent.team == 0)
                        {
                            enemyTeam = BeeEntityManager.bemInstance.blueBeesTeam;
                        }
                        else
                        {
                            enemyTeam = BeeEntityManager.bemInstance.yellowBeesTeam;
                        }
                        if(enemyTeam.Length > 0)
                        {
                            beeComponent.enemyTarget = enemyTeam[UnityEngine.Random.Range(0, enemyTeam.Length)];
                        }                        
                    }
                    else
                    {
                        //da aggiungere la parte delle risorse
                        beeComponent.resourceTarget = Entity.Null;
                    }
                }
                else if(beeComponent.enemyTarget != Entity.Null)
                {
                    if(EntityManager.GetComponentData<BeeComponent>(beeComponent.enemyTarget).dead)
                    {
                        beeComponent.enemyTarget = Entity.Null;
                    }
                    else
                    {
                        delta = EntityManager.GetComponentData<Translation>(beeComponent.enemyTarget).Value - translation.Value;
                        float sqrDist = delta.x * delta.x + delta.y * delta.y + delta.z * delta.z;
                        if(sqrDist > BeeEntityManager.bemInstance.attackDistance * BeeEntityManager.bemInstance.attackDistance)
                        {
                            beeComponent.velocity += delta * (BeeEntityManager.bemInstance.chaseForce * deltaTime / Mathf.Sqrt(sqrDist));
                        }
                        else
                        {
                            beeComponent.isAttacking = true;
                            beeComponent.velocity += delta * (BeeEntityManager.bemInstance.attackForce * deltaTime / Mathf.Sqrt(sqrDist));
                            if(sqrDist< BeeEntityManager.bemInstance.hitDistance * BeeEntityManager.bemInstance.hitDistance)
                            {
                                //spawn particella da aggiungere come entity
                                EntityManager.SetComponentData(beeComponent.enemyTarget, new BeeComponent
                                {
                                    dead = true,
                                    velocity = EntityManager.GetComponentData<BeeComponent>(beeComponent.enemyTarget).velocity * 0.5f
                                });
                                beeComponent.enemyTarget = Entity.Null;
                            }
                        }
                    }
                }
                else if(beeComponent.resourceTarget != Entity.Null)
                {
                    //aggiungere tutta la parte di resources
                }
            }
            else
            {
                beeComponent.velocity.y += Field.gravity * deltaTime;
                beeComponent.deathTimer -= deltaTime / 10f;
                if(beeComponent.deathTimer <0)
                {
                    //trovare come distruggere la entity=??????
                }
               
            }
            translation.Value += deltaTime * (float3)beeComponent.velocity;

            if (System.Math.Abs(translation.Value.x) > Field.size.x * .5f)
            {
                translation.Value.x = (Field.size.x * .5f) * Mathf.Sign(translation.Value.x);
                beeComponent.velocity.x *= -.5f;
                beeComponent.velocity.y *= .8f;
                beeComponent.velocity.z *= .8f;
            }
            if (System.Math.Abs(translation.Value.z) > Field.size.z * .5f)
            {
                translation.Value.z = (Field.size.z * .5f) * Mathf.Sign(translation.Value.z);
                beeComponent.velocity.z *= -.5f;
                beeComponent.velocity.x *= .8f;
                beeComponent.velocity.y *= .8f;
            }

        });
    }
        
}
