using Unity.Entities;

public enum TargetTypes
{
    EnemyBee,
    Resource
}

public struct TargetComponent : IComponentData
{
    public TargetTypes type;
    public Entity target;
}
