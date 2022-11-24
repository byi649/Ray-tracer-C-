using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent an (infinite) plane in a scene.
    /// </summary>
    public class Sphere : SceneEntity
    {
        private Vector3 center;
        private double radius;
        private Material material;

        /// <summary>
        /// Construct a sphere given its center point and a radius.
        /// </summary>
        /// <param name="center">Center of the sphere</param>
        /// <param name="radius">Radius of the spher</param>
        /// <param name="material">Material assigned to the sphere</param>
        public Sphere(Vector3 center, double radius, Material material)
        {
            this.center = center;
            this.radius = radius;
            this.material = material;
        }

        /// <summary>
        /// Determine if a ray intersects with the sphere, and if so, return hit data.
        /// </summary>
        /// <param name="ray">Ray to check</param>
        /// <returns>Hit data (or null if no intersection)</returns>
        public RayHit Intersect(Ray ray)
        {
            // https://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-sphere-intersection
            double a = ray.Direction.LengthSq();
            double b = 2 * ray.Direction.Normalized().Dot(ray.Origin - this.center);
            double c = (ray.Origin - this.center).LengthSq() - Math.Pow(this.radius, 2);
            
            double det = Math.Pow(b, 2) - 4 * a * c;

            double root = 0;
            bool from_inside = false;
            if (det > 0) {

                // To prevent catastropic cancellation
                double q = 0;
                if (b > 0) {
                    q = -0.5 * (b + Math.Sqrt(det));
                } else {
                    q = -0.5 * (b - Math.Sqrt(det));
                }
                double root_1 = q / a;
                double root_2 = c / q;

                if (root_1 >= 0 && root_2 >= 0) {
                    // both roots positive, ray intersects sphere twice
                    root = root_1 < root_2 ? root_1 : root_2;
                } else if (root_1 >= 0) {
                    // one root positive, ray originates inside sphere
                    root = root_1;
                    from_inside = true;
                } else if (root_2 >= 0) {
                    // one root positive, ray originates inside sphere
                    root = root_2;
                    from_inside = true;
                } else {
                    // no roots positive, sphere is behind camera
                    return null;
                }

            } else if (det == 0) {
                // ray tangent to sphere
                root = -b / (2 * a);
            } else {
                // ray does not intersect sphere
                return null;
            }

            double t = root;

            Vector3 hit_pos = ray.Origin + ray.Direction.Normalized() * t;
            Vector3 hit_norm = (hit_pos - this.center).Normalized();
            if (from_inside) {
                hit_norm = -hit_norm;
            }
            Vector3 hit_inc = ray.Direction.Normalized();
            RayHit hit = new RayHit(hit_pos, hit_norm, hit_inc, this.material);

            return hit;
        }

        /// <summary>
        /// The material of the sphere.
        /// </summary>
        public Material Material { get { return this.material; } }
    }

}
