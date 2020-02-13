using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct BeeDeadandVelocityComponent : IComponentData
{
    public Vector3 velocity;
    public bool dead;
}
