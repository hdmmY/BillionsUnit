using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

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

        for (int i = 0; i < _cachedUniqueTerrainTypes.Count; i++)
        {
            var renderData = _cachedUniqueTerrainTypes[i];
            var posDatas = _terrainRenderGroup.GetComponentDataArray<Position2D> (forFilter, i);

            int dataLength = posDatas.Length;

            // Get the pos data array
            var nativePosDataArray = new NativeArray<Position2D> (posDatas.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            posDatas.CopyTo (nativePosDataArray, 0);
            var posDataArray = nativePosDataArray.ToArray ();
            nativePosDataArray.Dispose ();

            int beginIdx = 0;
            while (beginIdx < dataLength)
            {
                int length = math.min (BatchSize, dataLength - beginIdx);
                if (length <= 0) break;

                // Set up position data and rotation data
                Array.Copy (posDataArray, beginIdx, _positionArray, 0, length); // use Array.Copy to save time
                for (int t = 0; t < length; t++) _rotationArray[t] = renderData.Angle;

                // Render
                RenderBatch (renderData.Mesh, renderData.Material, length);

                beginIdx += length;
            }
        }

        _cachedUniqueTerrainTypes.Clear ();
        forFilter.Dispose ();
    }


    #region Private Render Relative

    private static readonly int BatchSize = 512;
    private uint[] _argsArray;
    private Position2D[] _positionArray;
    private float[] _rotationArray;
    private ComputeBuffer _argsBuffer;
    private ComputeBuffer _positionBuffer;
    private ComputeBuffer _rotationBuffer;


    private void SetUpRender ()
    {
        _argsArray = new uint[5];
        _positionArray = new Position2D[BatchSize];
        _rotationArray = new float[BatchSize];
        _argsBuffer = new ComputeBuffer (1, 5 * sizeof (int), ComputeBufferType.IndirectArguments);
        _positionBuffer = new ComputeBuffer (BatchSize, 2 * sizeof (float));
        _rotationBuffer = new ComputeBuffer (BatchSize, sizeof (float));
    }

    protected override void OnDestroyManager ()
    {
        _argsBuffer.Dispose ();
        _positionBuffer.Dispose ();
        _rotationBuffer.Dispose ();
    }

    private void RenderBatch (Mesh mesh, Material material, int length)
    {
        _argsArray[0] = mesh.GetIndexCount (0);
        _argsArray[1] = (uint) length;
        _argsBuffer.SetData (_argsArray);

        _positionBuffer.SetData (_positionArray);
        _rotationBuffer.SetData (_rotationArray);

        material.SetBuffer ("positionBuffer", _positionBuffer);
        material.SetBuffer ("rotationBuffer", _rotationBuffer);

        Graphics.DrawMeshInstancedIndirect (
            mesh: mesh,
            submeshIndex: 0,
            material: material,
            bounds: new Bounds (Vector3.zero, new Vector3 (1000, 0, 1000)),
            bufferWithArgs : _argsBuffer,
            argsOffset : 0,
            properties : null,
            castShadows : ShadowCastingMode.Off,
            receiveShadows : false,
            layer : 0,
            camera : null,
            lightProbeUsage : LightProbeUsage.Off,
            lightProbeProxyVolume : null
        );
    }
    #endregion
}