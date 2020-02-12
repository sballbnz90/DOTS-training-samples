using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct BeeComponent : IComponentData
{
    public int team;
    public float3 home;
}
