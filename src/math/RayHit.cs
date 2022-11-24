using System;
using System.Collections.Generic;

namespace RayTracer
{
    /// <summary>
    /// Class to represent ray hit data, including the position and
    /// normal of a hit (and optionally, other computed vectors).
    /// </summary>
    public class RayHit
    {
        private Vector3 position;
        private Vector3 normal;
        private Vector3 incident;
        private Vector3? reflect = null;

        public RayHit(Vector3 position, Vector3 normal, Vector3 incident, Material material)
        {
            this.position = position;
            this.normal = normal;
            this.incident = incident;
        }

        // You may wish to write methods to compute other vectors, 
        // e.g. reflection, transmission, etc

        public Vector3 Position { get { return this.position; } }

        public Vector3 Normal { get { return this.normal; } }

        public Vector3 Incident { get { return this.incident; } }

        public Vector3 Reflect {
            get {
                // https://math.stackexchange.com/a/13263
                // Reflection vector math
                if (this.reflect == null) {
                    this.reflect = (this.Incident - 2 * (this.Incident.Dot(this.Normal) * this.Normal)).Normalized();
                }
                return (Vector3)this.reflect;
            }
        }

        public Vector3? Refract(double ref_ratio) {
                // https://physics.stackexchange.com/a/436252
                // Refraction vector math
                Vector3 N = -this.Normal.Normalized();
                Vector3 s1 = this.Incident.Normalized();

                Vector3 a_temp = ref_ratio * (s1 - N.Dot(s1) * N);
                double b_temp = 1 - Math.Pow(ref_ratio, 2.0) * (1 - Math.Pow(N.Dot(s1), 2.0));

                if (b_temp < 0) {
                    // Total internal reflection
                    return null;
                }

                Vector3 refracted_vector = (a_temp + N * Math.Sqrt(b_temp)).Normalized();
                return refracted_vector;
        }

    }
}
