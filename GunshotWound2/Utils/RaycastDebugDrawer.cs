namespace GunshotWound2.Utils {
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using GTA;
    using GTA.Math;
    using GTA.Native;

    public static class RaycastDebugDrawer {
        private readonly struct DebugRay {
            public readonly Vector3 start;
            public readonly Vector3 end;
            public readonly List<Vector3> hitPoints;

            public DebugRay(Vector3 start, Vector3 end, List<Vector3> hitPoints) {
                this.start = start;
                this.end = end;
                this.hitPoints = hitPoints;
            }
        }

        private static readonly List<DebugRay> ACTIVE_RAYS = new List<DebugRay>();

        public static void RegisterRay(Vector3 start, Vector3 end, List<Vector3> hits) {
            ACTIVE_RAYS.Add(new DebugRay(start, end, hits));
        }

        public static void Draw() {
            foreach (var ray in ACTIVE_RAYS) {
                Function.Call(Hash.DRAW_LINE, ray.start.X, ray.start.Y, ray.start.Z, ray.end.X, ray.end.Y, ray.end.Z, 255, 0, 0, 255);

                foreach (var hit in ray.hitPoints) {
                    World.DrawMarker(MarkerType.Sphere, hit, Vector3.Zero, Vector3.Zero, new Vector3(0.05f, 0.05f, 0.05f),
                                     Color.Yellow);

                    Function.Call(Hash.DRAW_LINE, hit.X, hit.Y, hit.Z, hit.X, hit.Y, hit.Z + 0.5f, 255, 255, 0, 255);
                }
            }
        }
    }
}