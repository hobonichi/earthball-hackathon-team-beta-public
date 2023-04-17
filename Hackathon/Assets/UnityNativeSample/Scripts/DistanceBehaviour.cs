// Copyright (c) 2023 Hobonichi Co., Ltd.
// 
// This software is released under the MIT License.
// https://opensource.org/license/mit/
using UnityEngine;
using Hobonichi;

[ExecuteAlways]
public class DistanceBehaviour : MonoBehaviour {
    [SerializeField] SampleLine line;

    private void Update() {
        var d = Hobonichi.SphericalGeometry.GetDistanceOnSphere(line.from.location, line.to.location, 6371f);
        GetComponentInChildren<TMPro.TextMeshProUGUI>().text = string.Format("{0}km", (int)d);

        if (line.points != null) {
            var loc = SphericalGeometry.Pos2LLH(line.points[line.points.Length/2]);
            loc.Set(loc.x, loc.y, 1.05f);
            GetComponent<ObjectOnSphere>().location = loc;
        }
    }
}
