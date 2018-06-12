using System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Rendering;


[Serializable]
public struct TerrainRenderer : ISharedComponentData
{
    public Mesh Mesh;

    public Material Material;

    public int SubMesh;
    
    public LayerMask Layer;

    public Camera Camera;
}

public class TerrainRendererComponent : SharedComponentDataWrapper<TerrainRenderer> { }