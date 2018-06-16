using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
[UpdateAfter (typeof (TerrainCullingSystem))]
public class TerrainCullingBarrier : BarrierSystem { }

[ExecuteInEditMode]
public class TerrainCullingSystem : JobComponentSystem
{
    struct BoundingSphere
    {
        [ReadOnly] public ComponentDataArray<TerrainCulling> Spheres;
        [ReadOnly] public ComponentDataArray<TransformMatrix> Transforms;
        public EntityArray Entities;
        public int Length;
    }

    [ComputeJobOptimization]
    struct TransformCenterJob : IJobParallelForBatch
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<float4> Outputs;
        [NativeDisableParallelForRestriction]
        public NativeArray<float4> OldCullStatus;

        [ReadOnly]
        public ComponentDataArray<TerrainCulling> Spheres;
        [ReadOnly]
        public ComponentDataArray<TransformMatrix> Transforms;
        public void Execute (int start, int count)
        {
            float4 x = (float4) 0.0f;
            float4 y = (float4) 0.0f;
            float4 z = (float4) 0.0f;
            float4 r = (float4) 0.0f;
            float4 cull = (float4) 0.0f;
            for (int i = 0; i < count; ++i)
            {
                var center = math.mul (Transforms[start + i].Value, new float4 (Spheres[start + i].BoundingSphereCenter, 1.0f)).xyz;
                x[i] = center.x;
                y[i] = center.y;
                z[i] = center.z;
                r[i] = Spheres[start + i].BoundingSphereRadius;
                cull[i] = Spheres[start + i].CullStatus;
            }
            Outputs[start] = x;
            Outputs[start + 1] = y;
            Outputs[start + 2] = z;
            Outputs[start + 3] = r;
            OldCullStatus[start / 4] = cull;
        }
    }

    struct FrustumPlanes
    {
        public float4 LeftX;
        public float4 LeftY;
        public float4 LeftZ;
        public float4 LeftDist;
        public float4 RightX;
        public float4 RightY;
        public float4 RightZ;
        public float4 RightDist;
    }

    [ComputeJobOptimization]
    struct FrustumCullJob : IJobParallelFor
    {
        [DeallocateOnJobCompletion][ReadOnly]
        public NativeArray<float4> Centers;
        public NativeArray<float4> Culleds;

        [DeallocateOnJobCompletion][ReadOnly]
        public NativeArray<FrustumPlanes> Planes;

        public void Execute (int i)
        {
            var x = Centers[i * 4];
            var y = Centers[i * 4 + 1];
            var z = Centers[i * 4 + 2];
            var r = Centers[i * 4 + 3];

            float4 cullDist = float.MinValue;
            for (int p = 0; p < Planes.Length; ++p)
            {
                var leftDist = Planes[p].LeftX * x + Planes[p].LeftY * y + Planes[p].LeftZ * z - Planes[p].LeftDist + r;
                var rightDist = Planes[p].RightX * x + Planes[p].RightY * y + Planes[p].RightZ * z - Planes[p].RightDist + r;

                var newCullDist = math.min (leftDist, rightDist);
                cullDist = math.max (cullDist, newCullDist);
            }

            // set to 1 if culled - 0 if visible
            Culleds[i] = math.select ((float4) 1f, (float4) 0.0f, cullDist >= (float4) 0.0f);;
        }
    }

    struct CullStatusUpdatejob : IJob
    {
        public EntityCommandBuffer CommandBuffer;
        [ReadOnly] public EntityArray Entities;
        public ComponentDataArray<TerrainCulling> Spheres;
        [DeallocateOnJobCompletion][ReadOnly] public NativeArray<float4> CullStatus;
        [DeallocateOnJobCompletion][ReadOnly] public NativeArray<float4> OldCullStatus;

        public void Execute ()
        {
            // Check for meshes which changed culling status, 4 at a time
            for (int i = 0; i < Spheres.Length / 4; ++i)
            {
                if (math.any (OldCullStatus[i] != CullStatus[i]))
                {
                    for (int j = 0; j < 4; ++j)
                    {
                        if (OldCullStatus[i][j] != CullStatus[i][j])
                        {
                            var temp = Spheres[i * 4 + j];
                            temp.CullStatus = CullStatus[i][j];
                            Spheres[i * 4 + j] = temp;
                            if (CullStatus[i][j] == 0.0f)
                                CommandBuffer.RemoveComponent<TerrainCulled> (Entities[i * 4 + j]);
                            else
                                CommandBuffer.AddComponent (Entities[i * 4 + j], new TerrainCulled ());
                        }
                    }
                }
            }

            if ((Spheres.Length & 3) != 0)
            {
                int baseIndex = Spheres.Length / 4;
                for (int i = 0; i < (Spheres.Length & 3); ++i)
                {
                    if (OldCullStatus[baseIndex][i] != CullStatus[baseIndex][i])
                    {
                        var temp = Spheres[baseIndex * 4 + i];
                        temp.CullStatus = CullStatus[baseIndex][i];
                        Spheres[baseIndex * 4 + i] = temp;
                        if (CullStatus[baseIndex][i] == 0.0f)
                            CommandBuffer.RemoveComponent<TerrainCulled> (Entities[baseIndex * 4 + i]);
                        else
                            CommandBuffer.AddComponent (Entities[baseIndex * 4 + i], new TerrainCulled ());
                    }
                }
            }
        }
    }

    FrustumPlanes generatePlane (Camera cam)
    {
        GeometryUtility.CalculateFrustumPlanes (cam, _cameraPlanes);
        float3 leftPlaneNormal = _cameraPlanes[0].normal;
        float leftPlaneDist = -_cameraPlanes[0].distance;
        float3 rightPlaneNormal = _cameraPlanes[1].normal;
        float rightPlaneDist = -_cameraPlanes[1].distance;

        return new FrustumPlanes
        {
            LeftX = leftPlaneNormal.xxxx,
                LeftY = leftPlaneNormal.yyyy,
                LeftZ = leftPlaneNormal.zzzz,
                LeftDist = new float4 (leftPlaneDist),
                RightX = rightPlaneNormal.xxxx,
                RightY = rightPlaneNormal.yyyy,
                RightZ = rightPlaneNormal.zzzz,
                RightDist = new float4 (rightPlaneDist),
        };
    }

    [Inject] private BoundingSphere _boundingSpheres;
    [Inject] private TerrainCullingBarrier _barrier;
    private Plane[] _cameraPlanes;


    protected override void OnCreateManager (int capacity)
    {
        _cameraPlanes = new Plane[6];
    }

    protected override JobHandle OnUpdate (JobHandle inputDeps)
    {
        int numCameras = Camera.allCamerasCount;
#if UNITY_EDITOR
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            numCameras += SceneView.GetAllSceneCameras ().Length;
        else
            numCameras = SceneView.GetAllSceneCameras ().Length;
#endif
        if (numCameras == 0)
            return inputDeps;
        var planes = new NativeArray<FrustumPlanes> (numCameras, Allocator.TempJob);

#if UNITY_EDITOR
        for (int cam = 0; cam < SceneView.GetAllSceneCameras ().Length; ++cam)
            planes[cam] = generatePlane (SceneView.GetAllSceneCameras () [cam]);

        if (EditorApplication.isPlayingOrWillChangePlaymode)
#endif
        {
            for (int i = 0; i < Camera.allCamerasCount; ++i)
                planes[numCameras - Camera.allCamerasCount + i] = generatePlane (Camera.allCameras[i]);
        }

        var centers = new NativeArray<float4> ((_boundingSpheres.Length + 3) & ~3, Allocator.TempJob);
        var cullStatus = new NativeArray<float4> ((_boundingSpheres.Length + 3) & ~3, Allocator.TempJob);
        var oldCullStatus = new NativeArray<float4> ((_boundingSpheres.Length + 3) & ~3, Allocator.TempJob);
        var transJob = new TransformCenterJob { Outputs = centers, OldCullStatus = oldCullStatus, Spheres = _boundingSpheres.Spheres, Transforms = _boundingSpheres.Transforms };
        var cullJob = new FrustumCullJob
        {
            Centers = centers, Culleds = cullStatus,
            Planes = planes
        };

        // First run a job which calculates the center positions of the meshes and stores them as float4(x1,x2,x3,x4), float4(y1,y2,y3,y4), ..., float4(x5, x6, x7, x8)
        var trans = transJob.ScheduleBatch (_boundingSpheres.Length, 4, inputDeps);
        // Check four meshes at a time agains the plains, possible since we changed how positions are stored in the previous job
        var cullHandle = cullJob.Schedule ((_boundingSpheres.Length + 3) / 4, 1, trans);

        var cullStatusUpdateJob = new CullStatusUpdatejob
        {
            CommandBuffer = _barrier.CreateCommandBuffer (),
            Entities = _boundingSpheres.Entities,
            Spheres = _boundingSpheres.Spheres,
            CullStatus = cullStatus,
            OldCullStatus = oldCullStatus
        };

        return cullStatusUpdateJob.Schedule (cullHandle);
    }
}