using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct TeamComponent : ISharedComponentData
{
    public int team;
}
