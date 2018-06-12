using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;


[ExecuteInEditMode]
public class TerrainRenderingSystem : ComponentSystem
{

    private Matrix4x4[] _matrixArray = new Matrix4x4[1023];
    private ComponentGroup _terrainRenderGroup;
    private List<TerrainRenderer> _cachedUniqueRendererTypes = new List<TerrainRenderer> (20);

    protected override void OnCreateManager (int capacity)
    {
        _terrainRenderGroup = GetComponentGroup (
            typeof (TerrainRenderer), typeof (TransformMatrix), typeof (Terrain));
    }

    protected override void OnUpdate ()
    {
        EntityManager.GetAllUniqueSharedComponentDatas<TerrainRenderer> (_cachedUniqueRendererTypes);

        var foreachFilter = _terrainRenderGroup.CreateForEachFilter (_cachedUniqueRendererTypes);
        for (int i = 0; i < _cachedUniqueRendererTypes.Count; i++)
        {
            var renderer = _cachedUniqueRendererTypes[i];
            var transforms = _terrainRenderGroup.GetComponentDataArray<TransformMatrix> (foreachFilter, i);

            int beginIdx = 0;
            while (beginIdx < transforms.Length)
            {
                int length = math.min (_matrixArray.Length, transforms.Length - beginIdx);
                CopyMatrices (transforms, beginIdx, length, _matrixArray);
                Graphics.DrawMeshInstanced (
                    mesh: renderer.Mesh,
                    submeshIndex: renderer.SubMesh,
                    material: renderer.Material,
                    matrices: _matrixArray,
                    count: length,
                    properties: null,
                    castShadows: ShadowCastingMode.Off,
                    receiveShadows: false,
                    layer: renderer.Layer.value,
                    camera: renderer.Camera,
                    lightProbeUsage: LightProbeUsage.Off,
                    lightProbeProxyVolume: null);

                beginIdx += length;
            }
        }

        _cachedUniqueRendererTypes.Clear ();
        foreachFilter.Dispose ();
    }


    // Use Unity build-in RenderingInstance.CopyMatrics
    private unsafe static void CopyMatrices (ComponentDataArray<TransformMatrix> transforms, int beginIndex, int length, Matrix4x4[] outMatrices)
    {
        fixed (Matrix4x4 * matricesPtr = outMatrices)
        {
            Assert.AreEqual (sizeof (Matrix4x4), sizeof (TransformMatrix));
            var matricesSlice = NativeSliceUnsafeUtility.ConvertExistingDataToNativeSlice<TransformMatrix> (matricesPtr, sizeof (Matrix4x4), length);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeSliceUnsafeUtility.SetAtomicSafetyHandle (ref matricesSlice, AtomicSafetyHandle.GetTempUnsafePtrSliceHandle ());
#endif
            transforms.CopyTo (matricesSlice, beginIndex);
        }
    }



}