﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct BeeComponent : IComponentData
{
	public Vector3 rotation;

	public Vector3 smoothPosition;
	public Vector3 smoothDirection;
	public Entity enemyTarget;
	public Entity imTargetof;
	public Entity resourceTarget;
	public int team;
	public float size;


	public float deathTimer;
	public bool isAttacking;
	public bool isHoldingResource;
	public int index;
}
