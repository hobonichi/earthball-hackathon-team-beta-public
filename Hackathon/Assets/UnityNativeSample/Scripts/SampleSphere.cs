// Copyright (c) 2023 Hobonichi Co., Ltd.
// 
// This software is released under the MIT License.
// https://opensource.org/license/mit/
using UnityEngine;
using UnityEngine.EventSystems;
using Hobonichi;


public class SampleSphere : MonoBehaviour {
    [SerializeField] GameObject currentTarget;

    void Start() {
        if (currentTarget) {
            currentTarget.GetComponent<Collider>().enabled = false;
        }
    }

    public void SetDragTarget(GameObject target) {
        if (currentTarget) {
            currentTarget.GetComponent<Collider>().enabled = true;
        }

        currentTarget = target;
        currentTarget.GetComponent<Collider>().enabled = false;
    }

    public void OnTapDown(BaseEventData args) {
        if (currentTarget) {
            var p = args as PointerEventData;
            var loc = SphericalGeometry.Pos2LLH(transform.InverseTransformPoint(p.pointerCurrentRaycast.worldPosition));
            loc.Set(loc.x, loc.y, 1);
            currentTarget.GetComponent<ObjectOnSphere>().location = loc;
        }
    }
}
