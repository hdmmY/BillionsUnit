using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections.LowLevel.Unsafe;


public class TerrainRenderSystem : ComponentSystem
{
    private List<TerrainData> _cachedUniqueTerrainTypes = new List<TerrainData> (10);

    [ReadOnly]
    private ComponentGroup _terrainRenderGroup;

    protected override void OnCreateManager (int capacity)
    {
        _terrainRenderGroup = GetComponentGroup (typeof (TerrainData), typeof (Position2D));
        SetUpRender ();
    }

    protected override void OnUpdate ()
    {
        EntityManager.GetAllUniqueSharedComponentDatas (_cachedUniqueTerrainTypes);

        var forFilter = _terrainRenderGroup.CreateForEachFilter (_cachedUniqueTerrainTypes);

        FlushBuffers ();
        for (int i = 0; i < _cachedUniqueTerrainTypes.Count; i++)
        {
            var renderData = _cachedUniqueTerrainTypes[i];
            var posDatas = _terrainRenderGroup.GetComponentDataArray<Position2D> (forFilter, i);

            Render (renderData, posDatas);
        }

        _cachedUniqueTerrainTypes.Clear ();
        forFilter.Dispose ();
    }


    #region Private Render Relative

    private static readonly int BatchSize = 1024;

    private uint[] _argsArray;
    private Position2D[] _positionArray;

    private readonly MaterialPropertyBlock _matPropertyBlock = new MaterialPropertyBlock ();
    private readonly ComputeBufferPool.Context _argsBuffers = ComputeBufferPool.GetShared (ComputeBufferType.IndirectArguments).CreateContext ();
    private readonly ComputeBufferPool.Context _positionBuffers = ComputeBufferPool.GetShared ().CreateContext ();

    private static int _positionPropertyId = Shader.PropertyToID ("positionBuffer");
    private static int _rotationPropertyId = Shader.PropertyToID ("rotation");

    private void SetUpRender ()
    {
        _argsArray = new uint[5];
        _positionArray = new Position2D[BatchSize];
    }

    protected override void OnDestroyManager ()
    {
        FlushBuffers ();
    }

    private void FlushBuffers ()
    {
        _argsBuffers.Flush ();
        _positionBuffers.Flush ();
    }

    private void OnDestroy ()
    {
        FlushBuffers ();
    }

    private unsafe void Render (TerrainData renderData, ComponentDataArray<Position2D> positions)
    {
        int totalLen = positions.Length;

        var nativePositionArr = new NativeArray<Position2D> (totalLen, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        positions.CopyTo (nativePositionArr, 0);

        var srcPosPtr = (Position2D * ) nativePositionArr.GetUnsafeReadOnlyPtr ();

        int srcBeginIdx = 0, dstBeginIdx = 0;
        while (srcBeginIdx < totalLen)
        {
            int count = math.min (BatchSize - dstBeginIdx, totalLen - srcBeginIdx);

            fixed (Position2D * dstPosPtr = _positionArray)
            {
                UnsafeUtility.MemCpy (dstPosPtr + dstBeginIdx, srcPosPtr + srcBeginIdx, count * sizeof (Position2D));
            }
            dstBeginIdx += count;
            dstBeginIdx %= BatchSize;
            srcBeginIdx += count;
            if (dstBeginIdx == 0) RenderBatch (renderData.Mesh, renderData.Material, renderData.Angle, BatchSize);
        }
        if (dstBeginIdx != 0) RenderBatch (renderData.Mesh, renderData.Material, renderData.Angle, dstBeginIdx);

        nativePositionArr.Dispose ();
    }

    private void RenderBatch (Mesh mesh, Material material, float angle, int length)
    {
        var argsBuffer = _argsBuffers.Rent (1, 5 * sizeof (int));
        var positionBuffer = _positionBuffers.Rent (BatchSize, 2 * sizeof (float));

        _argsArray[0] = mesh.GetIndexCount (0);
        _argsArray[1] = (uint) length;
        argsBuffer.SetData (_argsArray);

        positionBuffer.SetData (_positionArray);

        _matPropertyBlock.SetBuffer (_positionPropertyId, positionBuffer);
        _matPropertyBlock.SetFloat (_rotationPropertyId, angle * Mathf.Deg2Rad);

        Graphics.DrawMeshInstancedIndirect (
            mesh: mesh,
            submeshIndex: 0,
            material: material,
            bounds: new Bounds (Vector3.zero, new Vector3 (1000, 0, 1000)),
            bufferWithArgs : argsBuffer,
            argsOffset : 0,
            properties : _matPropertyBlock,
            castShadows : ShadowCastingMode.Off,
            receiveShadows : false,
            layer : 0, // It will be set correct later
            camera : null, // It will be set correct later
            lightProbeUsage : LightProbeUsage.Off,
            lightProbeProxyVolume : null
        );
    }
    #endregion
}