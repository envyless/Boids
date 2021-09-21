//
// Boids - Flocking behavior simulation.
//
// Copyright (C) 2014 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using UnityEngine;
using System.Collections;

public class BoidBehaviour : MonoBehaviour
{
    // Reference to the controller.
    public BoidController controller;

    // Options for animation playback.
    public float animationSpeedVariation = 0.2f;

    // Random seed.
    float noiseOffset;

    // Caluculates the separation vector with a target.
    Vector3 GetSeparationVector(Transform target)
    {
        var diff = transform.position - target.transform.position;
        var diffLen = diff.magnitude;
        var scaler = Mathf.Clamp01(1.0f - diffLen / controller.neighborDist);
        return diff * (scaler / diffLen);
    }

    void Start()
    {
        noiseOffset = Random.value * 10.0f;

        var animator = GetComponent<Animator>();
        if (animator)
            animator.speed = Random.Range(-1.0f, 1.0f) * animationSpeedVariation + 1.0f;
    }

    Collider [] nearbyBoids = new Collider[30];
    void Update()
    {
        if (controller.IsJob)
            return;

        var currentPosition = transform.position;
        var currentRotation = transform.rotation;

        // Current velocity randomized with noise.
        var noise = Mathf.PerlinNoise(Time.time, noiseOffset) * 2.0f - 1.0f;
        var velocity = controller.velocity * (1.0f + noise * controller.velocityVariation);

        // Initializes the vectors.
        var separation = Vector3.zero;
        var alignment = controller.transform.forward;
        var cohesion = controller.transform.position;

        // Looks up nearby boids
        var nearCount = Physics.OverlapSphereNonAlloc(currentPosition, controller.neighborDist, nearbyBoids);
        if (nearCount == 0)
            nearCount = 1;

        int inc = 0;
        // Accumulates the vectors.
        foreach (var boid in nearbyBoids)
        {
            inc++;
            if (boid.gameObject == gameObject) continue;
            var t = boid.transform;
            separation += GetSeparationVector(t);
            alignment += t.forward;
            cohesion += t.position;

            if (inc >= nearCount)
                break;
        }

        var avg = 1.0f / nearCount;
        alignment *= avg;
        cohesion *= avg;
        cohesion = (cohesion - currentPosition).normalized;

        // Calculates a rotation from the vectors.
        var direction = separation + alignment + cohesion;
        var rotation = Quaternion.FromToRotation(Vector3.forward, direction.normalized);

        // Applys the rotation with interpolation.
        if (rotation != currentRotation)
        {
            var ip = Mathf.Exp(-controller.rotationCoeff * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(rotation, currentRotation, ip);
        }

        // Moves forawrd.
        transform.position = currentPosition + transform.forward * (velocity * Time.deltaTime);
    }

    public void UpdateBySimulateResult(in Boids.BoidResult result)
    {
        // Calculates a rotation from the vectors.
        var tempDirFloat = result.separation + result.alignment + result.cohesion;
        var direction = new Vector3(tempDirFloat.x, tempDirFloat.y, tempDirFloat.z);
        var rotation = Quaternion.FromToRotation(Vector3.forward, direction.normalized);

        var currentPosition = transform.position;
        var currentRotation = transform.rotation;
        // Applys the rotation with interpolation.
        if (rotation != currentRotation)
        {
            var ip = Mathf.Exp(-controller.rotationCoeff * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(rotation, currentRotation, ip);
        }
        // Current velocity randomized with noise.
        var noise = Mathf.PerlinNoise(Time.time, noiseOffset) * 2.0f - 1.0f;
        var velocity = controller.velocity * (1.0f + noise * controller.velocityVariation);

        // Moves forawrd.
        transform.position = currentPosition + transform.forward * (velocity * Time.deltaTime);
    }
}
