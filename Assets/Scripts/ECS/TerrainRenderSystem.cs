using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Assertions;

public class TerrainRenderSystem : ComponentSystem
{
    private List<TerrainData> _cachedUniqueTerrainTypes = new List<TerrainData> (10);

    private ComponentGroup _terrainRenderGroup;

    // Instance renderer takes only batches of 512
    private Matrix4x4[] _posArray = new Matrix4x4[512];

    // Ugly code here 
    private unsafe void CopyPositions (float2 size, float angle,
        ComponentDataArray<Position2D> positions, int beginIdx, int length, Matrix4x4[] outMatrices)
    {
        int idx = 0;

        for (int i = 0; i < length; i++)
        {
            idx = beginIdx + i;

            float r = angle * Mathf.Deg2Rad;

            float cosr = math.cos (r);
            float sinr = math.sin (r);

            float2 pos = positions[i].CarValue;
           
            // trans tile to the center, then rotate, then tranlate to the target point

            float4x4 mat1 = new float4x4 (
                1, 0, 0, -size.x / 2,
                0, 1, 0, 0,
                0, 0, 1, -size.y / 2,
                0, 0, 0, 1
            );

            float4x4 mat2 = new float4x4 (
                 cosr,  0,  sinr,  pos.x,
                    0,  1,     0,  0, 
                -sinr,  0,  cosr,  pos.y,
                    0,  0,     0,  1
            );

            float4x4 final = math.mul (mat1, mat2);

            float4 m0 = final.m0;
            float4 m1 = final.m1;
            float4 m2 = final.m2;
            float4 m3 = final.m3;

            outMatrices[i] = new Matrix4x4 (
                new Vector4 (m0.x, m1.x, m2.x, m3.x),
                new Vector4 (m0.y, m1.y, m2.y, m3.y),
                new Vector4 (m0.z, m1.z, m2.z, m3.z),
                new Vector4 (m0.w, m1.w, m2.w, m3.w)
            );
        }
    }

    protected override void OnCreateManager (int capacity)
    {
        _terrainRenderGroup = GetComponentGroup (typeof (TerrainData), typeof (Position2D));
    }

    protected override void OnUpdate ()
    {
        EntityManager.GetAllUniqueSharedComponentDatas (_cachedUniqueTerrainTypes);

        var forFilter = _terrainRenderGroup.CreateForEachFilter (_cachedUniqueTerrainTypes);

        for (int i = 0; i < _cachedUniqueTerrainTypes.Count; i++)
        {
            var renderData = _cachedUniqueTerrainTypes[i];
            var posDatas = _terrainRenderGroup.GetComponentDataArray<Position2D> (forFilter, i);

            int beginIdx = 0;
            while (beginIdx < posDatas.Length)
            {
                int length = math.min (_posArray.Length, posDatas.Length - beginIdx);
                CopyPositions (renderData.Size, renderData.Angle,
                    posDatas, beginIdx, length, _posArray);
                Graphics.DrawMeshInstanced (renderData.Mesh, 0, renderData.Material, _posArray, length);
                beginIdx += length;
            }
        }

        _cachedUniqueTerrainTypes.Clear ();
        forFilter.Dispose ();
    }
}