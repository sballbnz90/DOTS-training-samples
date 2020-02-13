using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Rendering;
using Unity.Transforms;
using System;
using System.Collections.Generic;

public class ResouceEntityManager : MonoBehaviour
{
    [Header("Meshes and Mats")]
    public Mesh resourceMesh;
    public Material rescourceMat;

    [Header("Stats and Counters")]
    public float resourceSize;
    public float snapStiffness;
    public float carryStiffness;
    public float spawnRate = .1f;
    public int beesPerResource;
    [Space(10)]
    public int startResourceCount;

    List<Entity> resources;
    Vector2Int gridCounts;
    Vector2 gridSize;
    Vector2 minGridPos;

    int[,] stackHeights;

    float spawnTimer = 0f;

    public static ResouceEntityManager rinstance;

    private void Awake()
    {

        if (rinstance != null && rinstance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            rinstance = this;
        }
    }

    void Start()
    {
        
    }

}
