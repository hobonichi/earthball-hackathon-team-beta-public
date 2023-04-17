// Copyright (c) 2023 Hobonichi Co., Ltd.
// 
// This software is released under the MIT License.
// https://opensource.org/license/mit/
using UnityEngine;

namespace Hobonichi {

    public class LineOnSphere : MonoBehaviour {

        public float Width { get; set; }
        float lastScale;

        public void Setup(ObjectInfo info) {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            var line = GetComponent<LineRenderer>();
            var points = SphericalGeometry.GenerateGreatCirclePoints(info.From, info.To, info.From.z, info.split);
            line.positionCount = points.Length;
            line.SetPositions(points);
            line.startColor = info.MainColor;
            line.endColor = info.MainColor;
            Width = info.width;

            lastScale = transform.lossyScale.x;
            var w = Width * lastScale;
            line.widthCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0, w), new Keyframe(1, w) });

            GetComponent<Renderer>().enabled = FindObjectOfType<Controller>().IsTracking;
        }

        void Start() {
            lastScale = transform.lossyScale.x;
        }

        // Update is called once per frame
        void Update() {
            if (transform.lossyScale.x != lastScale) {
                lastScale = transform.lossyScale.x;
                var w = Width * lastScale;
                var l = GetComponent<LineRenderer>();
                l.widthCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0, w), new Keyframe(1, w) });
            }
        }
    }

}
