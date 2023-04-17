// Copyright (c) 2023 Hobonichi Co., Ltd.
// 
// This software is released under the MIT License.
// https://opensource.org/license/mit/
using Hobonichi;
using UnityEngine;

[ExecuteAlways]
public class SampleLine : MonoBehaviour {
    public ObjectOnSphere from;
    public ObjectOnSphere to;
    public float width;

    Vector3 lastFrom;
    Vector3 lastTo;
    float lastWidth;
    float lastScale;

    public Vector3[] points { get; private set; }


    void Setup() {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        var line = GetComponent<LineRenderer>();
        points = SphericalGeometry.GenerateGreatCirclePoints(from.location, to.location, from.location.z, 32);
        if (points != null) {
            line.positionCount = points.Length;
            line.SetPositions(points);
        }

        var w = width * lastScale;
        line.widthCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0, w), new Keyframe(1, w) });

        lastFrom = from.location;
        lastTo = to.location;
        lastScale = transform.lossyScale.x;
    }

    void Start() {
        Setup();
    }

    void Update() {
        if (from.location != lastFrom || to.location != lastTo || width != lastWidth || transform.lossyScale.x != lastScale) {
            Setup();
        }
    }
}
