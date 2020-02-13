using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class BaseComponent : IComponentData
{
    public float teamAttraction;
    public float teamRepulsion;
    public float aggression;
    public float attackDistance;
    public float chaseForce;
    public float attackForce;
    public float hitDistance;
    public float flightJitter;
    public float damping;
}
