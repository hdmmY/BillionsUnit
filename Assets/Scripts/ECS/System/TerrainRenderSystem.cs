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
        _terrainRenderGroup = GetComponentGroup (typeof (TerrainData),
            typeof (Position2D), typeof (Rotation2D), typeof (Scale2D));
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
            var rotDatas = _terrainRenderGroup.GetComponentDataArray<Rotation2D> (forFilter, i);
            var scaleDatas = _terrainRenderGroup.GetComponentDataArray<Scale2D> (forFilter, i);
            Render (renderData, posDatas, rotDatas, scaleDatas);
        }

        _cachedUniqueTerrainTypes.Clear ();
        forFilter.Dispose ();
    }


    #region Private Render Relative

    private static readonly int BatchSize = 64;

    private uint[] _argsArray;
    private Position2D[] _positionArray;
    private Rotation2D[] _rotationArray;
    private Scale2D[] _scaleArray;

    private readonly MaterialPropertyBlock _matPropertyBlock = new MaterialPropertyBlock ();
    private readonly ComputeBufferPool.Context _argsBuffers = ComputeBufferPool.GetShared (ComputeBufferType.IndirectArguments).CreateContext ();
    private readonly ComputeBufferPool.Context _positionBuffers = ComputeBufferPool.GetShared ().CreateContext ();
    private readonly ComputeBufferPool.Context _rotaionBuffers = ComputeBufferPool.GetShared ().CreateContext ();
    private readonly ComputeBufferPool.Context _scaleBuffers = ComputeBufferPool.GetShared ().CreateContext ();

    private static readonly int _positionPropertyId = Shader.PropertyToID ("positionBuffer");
    private static readonly int _rotationPropertyId = Shader.PropertyToID ("rotationBuffer");
    private static readonly int _scalePropertyId = Shader.PropertyToID ("scaleBuffer");

    private void SetUpRender ()
    {
        _argsArray = new uint[5];
        _positionArray = new Position2D[BatchSize];
        _rotationArray = new Rotation2D[BatchSize];
        _scaleArray = new Scale2D[BatchSize];
    }

    protected override void OnDestroyManager ()
    {
        DisposeBuffers ();
    }

    private void FlushBuffers ()
    {
        _argsBuffers.Flush ();
        _positionBuffers.Flush ();
        _rotaionBuffers.Flush ();
        _scaleBuffers.Flush ();
    }

    private void DisposeBuffers ()
    {
        _argsBuffers.Dispose ();
        _positionBuffers.Dispose ();
        _rotaionBuffers.Dispose ();
        _scaleBuffers.Dispose ();
    }

    private unsafe void Render (TerrainData renderData,
        ComponentDataArray<Position2D> positions,
        ComponentDataArray<Rotation2D> rotations,
        ComponentDataArray<Scale2D> scales)
    {
        int totalLen = positions.Length;

        var nativePositionArr = new NativeArray<Position2D> (totalLen, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        positions.CopyTo (nativePositionArr, 0);

        var nativeRotArr = new NativeArray<Rotation2D> (totalLen, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        rotations.CopyTo (nativeRotArr);

        var nativeScaArr = new NativeArray<Scale2D> (totalLen, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        scales.CopyTo (nativeScaArr);

        var srcPosPtr = (Position2D * ) nativePositionArr.GetUnsafeReadOnlyPtr ();
        var srcRotPtr = (Rotation2D * ) nativeRotArr.GetUnsafeReadOnlyPtr ();
        var srcScaPtr = (Scale2D * ) nativeScaArr.GetUnsafeReadOnlyPtr ();

        int srcBeginIdx = 0, dstBeginIdx = 0;
        while (srcBeginIdx < totalLen)
        {
            int count = math.min (BatchSize - dstBeginIdx, totalLen - srcBeginIdx);

            fixed (Position2D * dstPosPtr = _positionArray)
            {
                UnsafeUtility.MemCpy (dstPosPtr + dstBeginIdx, srcPosPtr + srcBeginIdx, count * sizeof (Position2D));
            }
            fixed (Rotation2D * dstRotPtr = _rotationArray)
            {
                UnsafeUtility.MemCpy (dstRotPtr + dstBeginIdx, srcRotPtr + srcBeginIdx, count * sizeof (Rotation2D));
            }
            fixed (Scale2D * dstScaPtr = _scaleArray)
            {
                UnsafeUtility.MemCpy (dstScaPtr + dstBeginIdx, srcScaPtr + srcBeginIdx, count * sizeof (Scale2D));
            }

            dstBeginIdx += count;
            dstBeginIdx %= BatchSize;
            srcBeginIdx += count;
            if (dstBeginIdx == 0) RenderBatch (renderData.Mesh, renderData.Material, BatchSize);
        }
        if (dstBeginIdx != 0) RenderBatch (renderData.Mesh, renderData.Material, dstBeginIdx);

        nativePositionArr.Dispose ();
        nativeRotArr.Dispose ();
        nativeScaArr.Dispose ();
    }

    private void RenderBatch (Mesh mesh, Material material, int length)
    {
        var argsBuffer = _argsBuffers.Rent (1, 5 * sizeof (int));
        var positionBuffer = _positionBuffers.Rent (BatchSize, 2 * sizeof (float));
        var rotationBuffer = _positionBuffers.Rent (BatchSize, sizeof (float));
        var scaleBuffer = _scaleBuffers.Rent (BatchSize, 2 * sizeof (float));

        _argsArray[0] = mesh.GetIndexCount (0);
        _argsArray[1] = (uint) length;
        argsBuffer.SetData (_argsArray);

        positionBuffer.SetData (_positionArray);
        rotationBuffer.SetData (_rotationArray);
        scaleBuffer.SetData (_scaleArray);

        _matPropertyBlock.SetBuffer (_positionPropertyId, positionBuffer);
        _matPropertyBlock.SetBuffer (_rotationPropertyId, rotationBuffer);
        _matPropertyBlock.SetBuffer (_scalePropertyId, scaleBuffer);

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