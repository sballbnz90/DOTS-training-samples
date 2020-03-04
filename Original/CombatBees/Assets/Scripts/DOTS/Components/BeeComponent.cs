using Unity.Entities;
using Unity.Mathematics;

public struct BeeComponent : IComponentData
{
    public int team;
    public int target;
    public float aggressiveness;
    public float3 home;
    public Random randomGenerator;
    public bool isDead;
}
