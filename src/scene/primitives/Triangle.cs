using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a triangle in a scene represented by three vertices.
    /// </summary>
    public class Triangle : SceneEntity
    {
        public Vector3 v0, v1, v2;
        private Material material;
        private Vector3 norm;
        private Vector3 vn0, vn1, vn2;
        private Boolean smoothed;

        /// <summary>
        /// Construct a triangle object given three vertices.
        /// </summary>
        /// <param name="v0">First vertex position</param>
        /// <param name="v1">Second vertex position</param>
        /// <param name="v2">Third vertex position</param>
        /// <param name="material">Material assigned to the triangle</param>
        public Triangle(Vector3 v0, Vector3 v1, Vector3 v2, Material material)
        {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
            this.norm = (v1 - v0).Cross(v2 - v0).Normalized();
            this.smoothed = false;
            this.material = material;
        }

        public Triangle(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 vn0, Vector3 vn1, Vector3 vn2, Material material) {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
            this.vn0 = vn0;
            this.vn1 = vn1;
            this.vn2 = vn2;
            this.norm = (vn0 + vn1 + vn2).Normalized(); // Temporary to calculate culling
            this.smoothed = true;
            this.material = material;
            this.ArrangeWinding();
        }

        private void ArrangeWinding() {
            // Swap vertices order if necessary for triangle-in-plane detection
            Vector3 calculatedNorm = (this.v1 - this.v0).Cross(this.v2 - this.v0).Normalized();
            if ((calculatedNorm - this.norm).LengthSq() > 1e-6) {
                Vector3 temp = this.v1;
                this.v1 = this.v2;
                this.v2 = temp;
                Vector3 temp_n = this.vn1;
                this.vn1 = this.vn2;
                this.vn2 = temp_n;
                // Fake norm for culling and hit_pos purposes
                this.norm = (this.v1 - this.v0).Cross(this.v2 - this.v0).Normalized();
            }
        }

        /// <summary>
        /// Determine if a ray intersects with the triangle, and if so, return hit data.
        /// </summary>
        /// <param name="ray">Ray to check</param>
        /// <returns>Hit data (or null if no intersection)</returns>
        public RayHit Intersect(Ray ray)
        {
            // Check front-facing or parallel
            double denom = ray.Direction.Dot(this.norm);
            if (smoothed == false) {
                if (denom >= 0) {
                    return null;
                }
            } else {
                if (!( ray.Direction.Dot(this.vn0) < 0
                    || ray.Direction.Dot(this.vn1) < 0
                    || ray.Direction.Dot(this.vn2) < 0)) {
                        return null;
                    }
            }

            double t = - (ray.Origin.Dot(this.norm) - this.norm.Dot(v1)) / denom;

            // Check camera direction
            if (t < 0) {
                return null;
            }

            Vector3 hit_pos = ray.Origin + ray.Direction.Normalized() * t;

            // Check ray hits triangle
            if (!PointInTriangle(this.norm, hit_pos)) {
                return null;
            }

            Vector3 hit_norm;
            if (smoothed == false) {
                hit_norm = this.norm;
            } else {
                // Weight norms to get smoothed norm
                double v0_dis = (hit_pos - v0).LengthSq();
                double v1_dis = (hit_pos - v1).LengthSq();
                double v2_dis = (hit_pos - v2).LengthSq();
                double eps = 1e-5;
                hit_norm = (vn0 / (eps + v0_dis) + vn1 / (eps + v1_dis) + vn2 / (eps + v2_dis)).Normalized();
            }
            
            Vector3 hit_inc = ray.Direction;
            RayHit hit = new RayHit(hit_pos, hit_norm, hit_inc, this.material);

            return hit;
        }

        private Boolean PointInTriangle(Vector3 norm, Vector3 point) {
            // https://www.scratchapixel.com/lessons/3d-basic-rendering/ray-tracing-rendering-a-triangle/ray-triangle-intersection-geometric-solution
            // Adapted from inside-out test in pseudo-code
            Vector3 edge10 = this.v1 - this.v0;
            Vector3 edge21 = this.v2 - this.v1;
            Vector3 edge02 = this.v0 - this.v2;

            Boolean in_bool =  norm.Dot(edge10.Cross(point - this.v0)) >= 0
                            && norm.Dot(edge21.Cross(point - this.v1)) >= 0
                            && norm.Dot(edge02.Cross(point - this.v2)) >= 0;

            return in_bool;
        }

        /// <summary>
        /// The material of the triangle.
        /// </summary>
        public Material Material { get { return this.material; } }
    }

}
