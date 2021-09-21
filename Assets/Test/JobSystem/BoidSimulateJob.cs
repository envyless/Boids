using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public static class Boids
{
    public struct BoidData
    {
        public Quaternion rotation;
        public float3 position;
        public float3 forward;
    }

    public struct BoidResult
    {
        public float3 separation;
        public float3 alignment;
        public float3 cohesion;
    }

    [BurstCompile]
    public struct BoidSimulateJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<BoidData> boidData;
        public NativeArray<BoidResult> result;

        public float3 c_forward;
        public float3 c_position;

        public void Execute(int index)
        {
            // get boid
            var boid = boidData[index];
            var resultIndex = result[index];
            resultIndex.separation = float3.zero;
            resultIndex.alignment = c_forward;
            resultIndex.cohesion = c_position;
            var nearCount = 0;

            for (int i = 0; i < boidData.Length; ++i)
            {
                if (i == index)
                    continue;

                // boid sum
                var otherBoid = boidData[i];

                if (!IsNear(boid.position, otherBoid.position))
                    continue;

                nearCount++;
                resultIndex.separation += GetSeparationVector(in boid, in otherBoid);
                resultIndex.alignment += otherBoid.forward;
                resultIndex.cohesion += otherBoid.position;
            }

            var avg = nearCount == 0 ? 1.0f : 1.0f / nearCount;
            resultIndex.alignment *= avg;
            resultIndex.cohesion *= avg;

            var toVec = resultIndex.cohesion - boid.position;
            resultIndex.cohesion = math.normalize(toVec);
        }

        const float neighborDist = 5;

        bool IsNear(float3 from, float3 to)
        {
            return math.distancesq(from, to) < neighborDist * neighborDist;
        }

        float3 GetSeparationVector(in BoidData from, in BoidData target)
        {
            
            var diff = from.position - target.position;
            var diffLen =  Unity.Mathematics.math.distance(from.position, target.position);
            var scaler = Mathf.Clamp01(1.0f - diffLen / neighborDist);
            return diff * (scaler / diffLen);
        }
    }

    public static NativeArray<BoidResult> result;
    public static NativeArray<BoidData> boidData;
    public static JobHandle jobHandle;
    public static System.Action<NativeArray<BoidResult>> onComplete;
    public static BoidSimulateJob boidSimulateJob;
    public static bool isRequested = false;

    public static IEnumerator RequestJob(List<BoidBehaviour> boidBehaviours, System.Action<NativeArray<BoidResult>> inOnComplete, BoidController boidController)
    {
        if (isRequested)
            yield break;
        
        isRequested = true;
        onComplete = inOnComplete;
        var boidCount = boidBehaviours.Count;
        boidData = new NativeArray<BoidData>(boidCount, Allocator.TempJob);
        result = new NativeArray<BoidResult>(boidCount, Allocator.TempJob);

        boidSimulateJob = new BoidSimulateJob
        {
            boidData = boidData,
            result = result,
            c_forward = boidController.transform.forward,
            c_position = new float3(boidController.transform.position.x, boidController.transform.position.y, boidController.transform.position.z)
        };
        jobHandle = boidSimulateJob.Schedule(boidCount, 64);

        //while(!jobHandle.IsCompleted)
        //{
        //    yield return null;
        //}

        jobHandle.Complete();
        onComplete(result);
        onComplete = null;
        isRequested = false;

        boidData.Dispose();
        result.Dispose();

        yield return null;
    }
}