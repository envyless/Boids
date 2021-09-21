//
// ProbePolisher - Light Probe Editor Plugin for Unity
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
using UnityEditor;
using System.Collections;

// Custom editor for polishable light probes.
[CustomEditor(typeof(LightProbes))]
class PolishableProbeEditor : Editor
{
    // Inspector GUI function.
    override public void OnInspectorGUI()
    {
        if (ProbePolisher.CheckPolishable(target as LightProbes))
        {
            EditorGUILayout.HelpBox("This is a polishable LightProbes asset.", MessageType.None);
            ShowPolisherGUI(target as LightProbes);
        }
        else
        {
            EditorGUILayout.HelpBox("This is not a polishable LightProbes asset.", MessageType.None);
            base.OnInspectorGUI();
        }
    }

    // Inspector GUI function for polishable probes.
    void ShowPolisherGUI(LightProbes probes)
    {
        
    }
}

// Cutom tool for baking polishable light probes.
static class ProbeBakingJig
{
    [MenuItem("GameObject/Create Other/Baking Jig")]
    static void CreateBakingJig ()
    {
        var go = GameObject.CreatePrimitive (PrimitiveType.Sphere);
        go.name = "Baking Jig";
        var group = go.AddComponent<LightProbeGroup> ();
        Object.DestroyImmediate (go.GetComponent<SphereCollider> ());
        var positions = new Vector3[]{
            new Vector3 (0, 20000, 0),
            new Vector3 (0, 40000, 0),
            new Vector3 (-10000, -10000, -10000),
            new Vector3 (+10000, -10000, -10000),
            new Vector3 (-10000, +10000, -10000),
            new Vector3 (+10000, +10000, -10000),
            new Vector3 (-10000, -10000, +10000),
            new Vector3 (+10000, -10000, +10000),
            new Vector3 (-10000, +10000, +10000),
            new Vector3 (+10000, +10000, +10000)
        };
        group.probePositions = positions;
        go.isStatic = true;
    }
}
