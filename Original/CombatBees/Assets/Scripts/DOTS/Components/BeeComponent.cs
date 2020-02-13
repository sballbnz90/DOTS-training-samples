using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct BeeComponent : IComponentData
{
    public int team;
    public int target;
    public float3 home;
    public Unity.Mathematics.Random randomGenerator;
    public bool isDead;
}
