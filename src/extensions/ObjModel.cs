using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace RayTracer
{
    /// <summary>
    /// Add-on option C. You should implement your solution in this class template.
    /// </summary>
    public class ObjModel : SceneEntity
    {
        private Material material;
        private List<Triangle> triangles;
        private List<BVHBox> boxes = new List<BVHBox>();

        /// <summary>
        /// Construct a new OBJ model.
        /// </summary>
        /// <param name="objFilePath">File path of .obj</param>
        /// <param name="offset">Vector each vertex should be offset by</param>
        /// <param name="scale">Uniform scale applied to each vertex</param>
        /// <param name="material">Material applied to the model</param>
        public ObjModel(string objFilePath, Vector3 offset, double scale, Material material)
        {
            this.material = material;

            // Here's some code to get you started reading the file...
            string[] lines = File.ReadAllLines(objFilePath);
            string[] substring;
            string[] v0;
            string[] v1;
            string[] v2;
            List<Vector3> vertices_unscaled = new List<Vector3>();
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> vertices_norms = new List<Vector3>();
            triangles = new List<Triangle>();
            double? minX = null;
            double? minY = null;
            double? minZ = null;
            double? maxX = null;
            double? maxY = null;
            double? maxZ = null;
            Vector3 center = new Vector3(0, 0, 0);


            for (int i = 0; i < lines.Length; i++) {
                if (lines[i].Substring(0, 2) == "v ") {
                    // Vertex
                    substring = lines[i].Substring(2).Split(" ");
                    Vector3 vertex = new Vector3(
                            double.Parse(substring[0]),
                            double.Parse(substring[1]),
                            -double.Parse(substring[2])); // RH coord
                    
                    vertices_unscaled.Add(vertex);

                    // Warning: bad code ahead
                    if (minX == null || vertex.X < minX) {
                        minX = vertex.X;
                    }

                    if (minY == null || vertex.Y < minY) {
                        minY = vertex.Y;
                    }

                    if (minZ == null || vertex.Z < minZ) {
                        minZ = vertex.Z;
                    }

                    if (maxX == null || vertex.X > maxX) {
                        maxX = vertex.X;
                    }

                    if (maxY == null || vertex.Y > maxY) {
                        maxY = vertex.Y;
                    }

                    if (maxZ == null || vertex.Z > maxZ) {
                        maxZ = vertex.Z;
                    }

                } else if (lines[i].Substring(0, 2) == "vn") {
                    // Vertex normal
                    substring = lines[i].Substring(3).Split(" ");
                    vertices_norms.Add(
                        new Vector3(
                            double.Parse(substring[0]),
                            double.Parse(substring[1]),
                            -double.Parse(substring[2]))); // RH coord
                }
            }

            // Scale and offset
            center = new Vector3((double)minX + (double)maxX,
                                    (double)minY + (double)maxY,
                                    (double)minZ + (double)maxZ);
            center = center / 2;
            foreach (Vector3 vertex in vertices_unscaled) {
                Vector3 scaled_vertex = center + (vertex - center) * scale + offset;
                vertices.Add(scaled_vertex);
                minX = Math.Min((double)minX, (double)scaled_vertex.X);
                minY = Math.Min((double)minY, (double)scaled_vertex.Y);
                minZ = Math.Min((double)minZ, (double)scaled_vertex.Z);
                maxX = Math.Max((double)maxX, (double)scaled_vertex.X);
                maxY = Math.Max((double)maxY, (double)scaled_vertex.Y);
                maxZ = Math.Max((double)maxZ, (double)scaled_vertex.Z);
            }

            // Run face logic after vertex in case face lines are first
            for (int i = 0; i < lines.Length; i++) {
                if (lines[i].Substring(0, 2) == "f ") {
                    substring = lines[i].Substring(2).Split(" ");
                    v0 = substring[0].Split("/");
                    v1 = substring[1].Split("/");
                    v2 = substring[2].Split("/");
                    triangles.Add(new Triangle(
                        vertices[-1 + int.Parse(v0[0])],
                        vertices[-1 + int.Parse(v1[0])],
                        vertices[-1 + int.Parse(v2[0])],
                        vertices_norms[-1 + int.Parse(v0[2])],
                        vertices_norms[-1 + int.Parse(v1[2])],
                        vertices_norms[-1 + int.Parse(v2[2])],
                        this.Material
                    ));
                }
                }

            // Generate (arbitrary number) bounding boxes
            double xrange = (double)(maxX - minX);
            double yrange = (double)(maxY - minY);
            double zrange = (double)(maxZ - minZ);
            int n = 4;
            for (int i = 0; i < n; i++) {
                for (int j = 0; j < n; j++) {
                    for (int k = 0; k < n; k++) {
                        Vector3 BFL = new Vector3((double)minX + (double)i/n * xrange,
                                                  (double)minY + (double)j/n * yrange,
                                                  (double)minZ + (double)k/n * zrange);
                        Vector3 TBR = new Vector3((double)minX + (double)(i+1)/n * xrange,
                                                  (double)minY + (double)(j+1)/n * yrange,
                                                  (double)minZ + (double)(k+1)/n * zrange);
                        this.boxes.Add(new BVHBox(BFL, TBR));
                    }
                }
            }

            // Sort triangles into boxes
            // Overlapping triangles go in all boxes
            foreach (BVHBox box in this.boxes) {
                foreach (Triangle triangle in this.triangles) {
                    if (box.InBox(triangle)) {
                            box.AddTriangle(triangle);
                    }
                }
                box.extendBox();
            }
            boxes.RemoveAll(box => box.triangles.Count == 0);

        }

        // I have no idea how a BVH works
        // This is complete guesswork based on 2 minutes of wikipedia research
        // I know its supposed to be a tree, but honestly a bunch of flat boxes can't
        // be that bad right? Not scalable, but whatever.
        private class BVHBox {
            private Vector3 BottomFrontLeft;
            private Vector3 BottomFrontRight;
            private Vector3 TopFrontLeft;
            private Vector3 TopFrontRight;
            private Vector3 BottomBackLeft;
            private Vector3 BottomBackRight;
            private Vector3 TopBackLeft;
            private Vector3 TopBackRight;

            public List<Triangle> Box = new List<Triangle>();

            public List<Triangle> triangles = new List<Triangle>();

            public BVHBox(Vector3 BottomFrontLeft, Vector3 TopBackRight) {
                this.BottomFrontLeft = BottomFrontLeft;
                this.TopBackRight = TopBackRight;

                double minX = BottomFrontLeft.X;
                double minY = BottomFrontLeft.Y;
                double minZ = BottomFrontLeft.Z;
                double maxX = TopBackRight.X;
                double maxY = TopBackRight.Y;
                double maxZ = TopBackRight.Z;

                this.MakeBox(minX, minY, minZ, maxX, maxY, maxZ);
            }

            private void MakeBox(double minX, double minY, double minZ, double maxX, double maxY, double maxZ) {
                this.BottomFrontLeft = new Vector3(minX, minY, minZ);
                this.TopBackRight = new Vector3(maxX, maxY, maxZ);
                this.BottomFrontRight = new Vector3(maxX, minY, minZ);
                this.TopFrontLeft = new Vector3(minX, maxY, minZ);
                this.TopFrontRight = new Vector3(maxX, maxY, minZ);
                this.BottomBackLeft = new Vector3(minX, minY, maxZ);
                this.BottomBackRight = new Vector3(maxX, minY, maxZ);
                this.TopBackLeft = new Vector3(minX, maxY, maxZ);

                Material fake_mat = new Material(
                    Material.MaterialType.Diffuse,
                    new Color(minX, minY, 0),
                    1);

                Box = new List<Triangle>();

                // Front face
                Box.Add(new Triangle(BottomFrontLeft, TopFrontLeft, BottomFrontRight, fake_mat));
                Box.Add(new Triangle(TopFrontLeft, TopFrontRight, BottomFrontRight, fake_mat));

                // Left face
                Box.Add(new Triangle(TopBackLeft, BottomFrontLeft, BottomBackLeft, fake_mat));
                Box.Add(new Triangle(TopBackLeft, TopFrontLeft, BottomFrontLeft, fake_mat));

                // Right face
                Box.Add(new Triangle(TopFrontRight, BottomBackRight, BottomFrontRight, fake_mat));
                Box.Add(new Triangle(TopFrontRight, TopBackRight, BottomBackRight, fake_mat));

                // Back face
                Box.Add(new Triangle(TopBackRight, BottomBackLeft, BottomBackRight, fake_mat));
                Box.Add(new Triangle(TopBackRight, TopBackLeft, BottomBackLeft, fake_mat));

                // Top face
                Box.Add(new Triangle(TopFrontLeft, TopBackLeft, TopFrontRight, fake_mat));
                Box.Add(new Triangle(TopFrontRight, TopBackLeft, TopBackRight, fake_mat));

                // Bottom face
                Box.Add(new Triangle(BottomFrontLeft, BottomFrontRight, BottomBackLeft, fake_mat));
                Box.Add(new Triangle(BottomFrontRight, BottomBackRight, BottomBackLeft, fake_mat));
            }

            public void extendBox() {

                double minX = this.BottomBackLeft.X;
                double minY = this.BottomBackLeft.Y;
                double minZ = this.BottomFrontLeft.Z;
                double maxX = this.BottomBackRight.X;
                double maxY = this.TopBackLeft.Y;
                double maxZ = this.BottomBackLeft.Z;
                foreach (Triangle triangle in this.triangles) {
                    List<Vector3> vertices = new List<Vector3>();
                    vertices.Add(triangle.v0);
                    vertices.Add(triangle.v1);
                    vertices.Add(triangle.v2);

                    foreach (Vector3 vertex in vertices) {
                        if (!InBox(vertex)) {
                            minX = Math.Min(vertex.X, minX);
                            minY = Math.Min(vertex.Y, minY);
                            minZ = Math.Min(vertex.Z, minZ);
                            maxX = Math.Max(vertex.X, maxX);
                            maxY = Math.Max(vertex.Y, maxY);
                            maxZ = Math.Max(vertex.Z, maxZ);
                        }
                    }
                }
                this.MakeBox(minX, minY, minZ, maxX, maxY, maxZ);
            }

            public Boolean InBox(Vector3 vector) {
                bool inX = vector.X >= BottomBackLeft.X && vector.X <= BottomBackRight.X;
                bool inY = vector.Y >= BottomBackLeft.Y && vector.Y <= TopBackLeft.Y;
                bool inZ = vector.Z >= BottomFrontLeft.Z && vector.Z <= BottomBackLeft.Z;
                return inX && inY && inZ;
            }

            public Boolean InBox(Triangle triangle) {

                List<Vector3> vertices = new List<Vector3>();
                vertices.Add(triangle.v0);
                vertices.Add(triangle.v1);
                vertices.Add(triangle.v2);
                vertices.Add((triangle.v0 + triangle.v1 + triangle.v2) / 3);

                // Sample multiple points inside triangle
                int n = 20;
                for (int i = 1; i < n; i++) {
                    double ratio = (double)i / n;
                    vertices.Add(ratio * triangle.v0 + (1 - ratio) * triangle.v1);
                    vertices.Add(ratio * triangle.v1 + (1 - ratio) * triangle.v2);
                    vertices.Add(ratio * triangle.v0 + (1 - ratio) * triangle.v2);

                    vertices.Add(ratio * triangle.v0
                                 + (1 - ratio) / 2 * triangle.v1
                                 + (1 - ratio) / 2 * triangle.v2);
                    vertices.Add(ratio * triangle.v1
                                 + (1 - ratio) / 2 * triangle.v0
                                 + (1 - ratio) / 2 * triangle.v2);
                    vertices.Add(ratio * triangle.v2
                                 + (1 - ratio) / 2 * triangle.v1
                                 + (1 - ratio) / 2 * triangle.v0);
                }

                foreach (Vector3 vertex in vertices) {
                    if (this.InBox(vertex)) {
                        return true;
                    }
                }

                return false;

            }

            public Boolean Intersect(Ray ray) {
                if (this.InBox(ray.Origin)) {
                    return true;
                }
                foreach (Triangle triangle in Box) {
                    RayHit hit = triangle.Intersect(ray);
                    if (hit != null) {
                        return true;
                    }
                }
                return false;
            }

            public void AddTriangle(Triangle triangle) {
                this.triangles.Add(triangle);
            }

        }

        /// <summary>
        /// Given a ray, determine whether the ray hits the object
        /// and if so, return relevant hit data (otherwise null).
        /// </summary>
        /// <param name="ray">Ray data</param>
        /// <returns>Ray hit data, or null if no hit</returns>
        public RayHit Intersect(Ray ray)
        {

            ConcurrentBag<(RayHit, double?)> hits = new ConcurrentBag<(RayHit, double?)>();

            Parallel.ForEach(this.boxes, box => {
                if (box.Intersect(ray)) {
                    Parallel.ForEach(box.triangles, triangle => {
                        RayHit hit = triangle.Intersect(ray);
                        if (hit != null) {
                            double distance = (hit.Position - ray.Origin).LengthSq();
                            hits.Add((hit, distance));
                        }
                    });
                }
            });

            RayHit hit_t = null;
            double? minDistance = null;
            foreach ((RayHit, double?) hit_data in hits) {
                if (hit_data.Item1 != null && (minDistance == null || hit_data.Item2 < minDistance)) {
                        hit_t = hit_data.Item1;
                        minDistance = hit_data.Item2;
                }
            }
            return hit_t;

        }

        /// <summary>
        /// The material attached to this object.
        /// </summary>
        public Material Material { get { return this.material; } }
    }

}
