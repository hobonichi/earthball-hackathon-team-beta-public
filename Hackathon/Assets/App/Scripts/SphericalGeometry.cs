// Copyright (c) 2023 Hobonichi Co., Ltd.
// 
// This software is released under the MIT License.
// https://opensource.org/license/mit/
using UnityEngine;

namespace Hobonichi {
    public class SphericalGeometry {
        public static Vector3 LLH2Pos(float lon, float lat, float h) {
            return new Vector3(
                h * Mathf.Cos((-lon + 90) * Mathf.Deg2Rad) * Mathf.Cos(lat * Mathf.Deg2Rad),
                h * Mathf.Sin(lat * Mathf.Deg2Rad),
                -h * Mathf.Sin((-lon + 90) * Mathf.Deg2Rad) * Mathf.Cos(lat * Mathf.Deg2Rad)
            );
        }

        public static Vector3 LLH2Pos(Vector3 llh) {
            return LLH2Pos(llh.x, llh.y, llh.z);
        }

        public static Vector3 Pos2LLH(Vector3 pos) {
            return Pos2LLH(pos.x, pos.y, pos.z);
        }

        public static Vector3 Pos2LLH(float x, float y, float z) {
            var r = Mathf.Sqrt(x * x + y * y + z * z);
            return new Vector3(
                Mathf.Atan2(x, -z) * Mathf.Rad2Deg,
                90-Mathf.Acos(y / r) * Mathf.Rad2Deg,
                r
            );
        }

        public static float GetDistanceOnSphere(Vector2 from, Vector2 to, float radius) {
            return radius * Mathf.Acos(Mathf.Sin(from.y * Mathf.Deg2Rad) * Mathf.Sin(to.y * Mathf.Deg2Rad) +
                                       Mathf.Cos(from.y * Mathf.Deg2Rad) * Mathf.Cos(to.y * Mathf.Deg2Rad) *
                                       Mathf.Cos((to.x - from.x) * Mathf.Deg2Rad));
        }

        public static Vector3[] GenerateGreatCirclePoints(Vector2 from, Vector2 to, float radius, int numPoints) {
            return GenerateGreatCirclePoints(from.x, from.y, to.x, to.y, radius, numPoints);
        }

        public static Vector3[] GenerateGreatCirclePoints(float lon1, float lat1, float lon2, float lat2, float radius, int numPoints) {
            Debug.Assert(numPoints >= 2);

            var p1 = LLH2Pos(lon1, lat1, 1).normalized;
            var p2 = LLH2Pos(lon2, lat2, 1).normalized;

            if (Mathf.Abs(Vector3.Dot(p1, p2)) == 1) {
                return null;
            }

            var ret = new Vector3[numPoints];

            for (int i = 0; i < numPoints; ++i) {
                var p = Vector3.Slerp(p1, p2, (float)i / (numPoints - 1)).normalized;
                ret[i] = p * radius;
            }

            return ret;
        }
        
        public static Vector3 GetLocationWithDistanceAndDirection(Vector2 loc, float radius, float distance, float direction) {
            // http://hp.vector.co.jp/authors/VA002244/yacht/geo.htm
            var e2 = (float)6.69437999019758e-03;
            var wt = Mathf.Sqrt(1 - e2 * Mathf.Pow(Mathf.Sin(loc.y * Mathf.Deg2Rad), 2));
            var mt = radius * (1 - e2) / Mathf.Pow(wt, 3);
            var dit = distance * Mathf.Cos(direction * Mathf.Deg2Rad) / mt;
            var i = loc.y * Mathf.Deg2Rad + dit / 2;

            var w = Mathf.Sqrt(1 - e2 * Mathf.Pow(Mathf.Sin(i), 2));
            var m = radius * (1 - e2) / Mathf.Pow(w, 3);
            var di = distance * Mathf.Cos(direction * Mathf.Deg2Rad) / m;
            var y = loc.y + di * Mathf.Rad2Deg;

            var n = radius / w;
            var dk = distance * Mathf.Sin(direction * Mathf.Deg2Rad) / (n * Mathf.Cos(i));
            var x = loc.x + dk * Mathf.Rad2Deg;

            return new Vector3(x, y, radius);
        }

        public static float GetDirectionOnSphere(Vector2 l1, Vector2 l2, float radius) {
            // http://hp.vector.co.jp/authors/VA002244/yacht/geo.htm
            var e2 = (float)6.69437999019758e-03;

            var d = l2-l1;
            var i = (l1.y + l2.y) / 2;

            var w = Mathf.Sqrt(1 - e2 * Mathf.Pow(Mathf.Sin(i * Mathf.Deg2Rad), 2));
            var m = radius * (1 - e2) / Mathf.Pow(w, 3);
            var n = radius / w;

            var ddy = d.y * Mathf.Deg2Rad * m;
            var ddx = d.x * Mathf.Deg2Rad * n * Mathf.Cos(i * Mathf.Deg2Rad);

            return (90 - Mathf.Atan2(ddy, ddx) * Mathf.Rad2Deg) % 360;
        }
    }
}
