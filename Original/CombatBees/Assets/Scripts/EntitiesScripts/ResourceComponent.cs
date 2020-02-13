using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct ResourceComponent : IComponentData
{
	public Vector3 position;
	public bool stacked;
	public int stackIndex;
	public int gridX;
	public int gridY;
	public Entity holder;
	public Vector3 velocity;
	public bool dead;
}
