using System;
using System.Collections.Generic;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a ray traced scene, including the objects,
    /// light sources, and associated rendering logic.
    /// </summary>
    public class Scene
    {
        private SceneOptions options;
        private ISet<SceneEntity> entities;
        private ISet<PointLight> lights;

        private double refractive_index = 1;

        /// <summary>
        /// Construct a new scene with provided options.
        /// </summary>
        /// <param name="options">Options data</param>
        public Scene(SceneOptions options = new SceneOptions())
        {
            this.options = options;
            this.entities = new HashSet<SceneEntity>();
            this.lights = new HashSet<PointLight>();
        }

        /// <summary>
        /// Add an entity to the scene that should be rendered.
        /// </summary>
        /// <param name="entity">Entity object</param>
        public void AddEntity(SceneEntity entity)
        {
            this.entities.Add(entity);
        }

        /// <summary>
        /// Add a point light to the scene that should be computed.
        /// </summary>
        /// <param name="light">Light structure</param>
        public void AddPointLight(PointLight light)
        {
            this.lights.Add(light);
        }

        /// <summary>
        /// Render the scene to an output image. This is where the bulk
        /// of your ray tracing logic should go... though you may wish to
        /// break it down into multiple functions as it gets more complex!
        /// </summary>
        /// <param name="outputImage">Image to store render output</param>
        public void Render(Image outputImage)
        {
            int aa_width = outputImage.Width * this.options.AAMultiplier;
            int aa_height = outputImage.Height * this.options.AAMultiplier;

            Color[,] pic = new Color[aa_width, aa_height];
            for (int i = 0; i < aa_width; i++) {
                for (int j = 0; j < aa_height; j++) {

                    Vector3 dir = this.Translate(outputImage, i, j);
                    Ray ray = new Ray(new Vector3(0, 0, 0), dir);

                    (SceneEntity closestEntity, RayHit rayHit) = this.GetClosest(ray);
                    if (closestEntity != null) {
                        Color pix_color = this.ColorPixel(closestEntity, rayHit, 0, new Stack<SceneEntity>());
                        pic[i, j] = pix_color;
                    }
                }
            }

            for (int i = 0; i < outputImage.Width; i++) {
                for (int j = 0; j < outputImage.Height; j++) {
                    int start_i = i * this.options.AAMultiplier;
                    int start_j = j * this.options.AAMultiplier;
                    Color avg_color = GetAverageColor(pic, start_i, start_j);
                    outputImage.SetPixel(i, j, avg_color);
                }
            }
        }

        private Color GetAverageColor(Color[,] colors, int start_i, int start_j) {
            Color sum_color = new Color(0, 0, 0);
            for (int i = 0; i < this.options.AAMultiplier; i++) {
                for (int j = 0; j < this.options.AAMultiplier; j++) {
                    sum_color = sum_color + colors[start_i + i, start_j + j];
                }
            }
            return sum_color / Math.Pow(this.options.AAMultiplier, 2);
        }

        private Vector3 Translate(Image outputImage, int i, int j) {
            double h_fov = Math.PI / 180 * 60;
            double aspect_ratio = outputImage.Width / outputImage.Height;
            double pixel_x = (i + 0.5) / (outputImage.Width * this.options.AAMultiplier);
            double pixel_y = (j + 0.5) / (outputImage.Height * this.options.AAMultiplier);
            double x_pos = (pixel_x * 2 - 1);
            double y_pos = 1 - (pixel_y * 2);
            double x_pos_fov = x_pos * Math.Tan(h_fov/2);
            double y_pos_fov = y_pos * Math.Tan(h_fov/2) / aspect_ratio;

            Vector3 dir = new Vector3(x_pos_fov, y_pos_fov, 1);
            return dir.Normalized();
        }

        private (SceneEntity, RayHit) GetClosest(Ray ray){
            double? closestDistance = null;
            SceneEntity closestEntity = null;
            RayHit closestHit = null;
            foreach (SceneEntity entity in this.entities) {
                RayHit hit = entity.Intersect(ray);
                if (hit != null) {
                    double distance = (hit.Position - ray.Origin).LengthSq();
                    if (closestDistance == null || distance < closestDistance) {
                        closestDistance = distance;
                        closestEntity = entity;
                        closestHit = hit;
                    }
                }
            }
            return (closestEntity, closestHit);
        }

        private double FresnelApprox(double re_1, double re_2, Vector3 normal, Vector3 incident) {
            // https://blog.demofox.org/2017/01/09/raytracing-reflection-refraction-fresnel-total-internal-reflection-and-beers-law/
            // Schlick approximation to Fresnel equation
            // I won't pretend to understand the math here

            double r0 = (re_1 - re_2) / (re_1 + re_2);
            r0 = Math.Pow(r0, 2);

            double cosX = -(normal.Dot(incident));
            if (re_1 > re_2) {
                double n = re_1 / re_2;
                double sinT2 = Math.Pow(n, 2) * (1 - Math.Pow(cosX, 2));
                // Total internal reflection
                if (sinT2 > 1) {
                    return 1.0;
                }
                cosX = Math.Sqrt(1 - sinT2);
            }

            double x = 1 - cosX;
            double ret = r0 + (1 - r0) * Math.Pow(x, 5);

            return ret;
        }

        private Color ColorPixel(SceneEntity entity, RayHit rayHit, int recurseCount, Stack<SceneEntity> refractPath) {

            int RECURSE_MAX = 10;

            if (entity == null || rayHit == null) {
                return new Color(0, 0, 0);
            }

            if (entity.Material.Type == Material.MaterialType.Diffuse) {
                Color canvas = new Color(0, 0, 0);
                foreach (PointLight light in this.lights) {
                    Vector3 Lhat = (light.Position - rayHit.Position).Normalized();

                    Ray lightRay = new Ray(light.Position, -Lhat);
                    (SceneEntity closestEntity, RayHit closestRayHit) = GetClosest(lightRay);

                    if (closestEntity == entity) {
                        double NL = rayHit.Normal.Dot(Lhat);
                        NL = NL > 0 ? NL : 0;
                        Color single_color = entity.Material.Color * light.Color * NL;
                        canvas = canvas + single_color;
                    }
                }

                return canvas;

            } else if (entity.Material.Type == Material.MaterialType.Reflective) {

                Ray reflected_ray = new Ray(rayHit.Position + rayHit.Reflect * 0.0001, rayHit.Reflect);
                (SceneEntity closestEntity_r, RayHit closestRayHit_r) = GetClosest(reflected_ray);

                if (closestEntity_r == null || recurseCount > RECURSE_MAX) {
                    return entity.Material.Color;
                }

                recurseCount = recurseCount + 1;

                return ColorPixel(closestEntity_r, closestRayHit_r, recurseCount, refractPath);

            } else if (entity.Material.Type == Material.MaterialType.Glossy) {
                // Math from:
                // https://en.wikipedia.org/wiki/Phong_reflection_model
                // Including approximations

                if (recurseCount > RECURSE_MAX) {
                    return entity.Material.Color;
                }

                double alpha = 12;
                double gamma = 4;
                double diffuse_weight = 0.5;
                double reflect_weight = 0.5;

                Color diffuse_canvas = new Color(0, 0, 0);
                Color reflect_canvas = new Color(0, 0, 0);
                foreach (PointLight light in this.lights) {
                    Vector3 Lhat = (light.Position - rayHit.Position).Normalized();

                    Ray lightRay = new Ray(light.Position, -Lhat);
                    (SceneEntity closestEntity, RayHit closestRayHit) = GetClosest(lightRay);

                    if (closestEntity == entity) {
                        // Diffuse component
                        double NL = rayHit.Normal.Dot(Lhat);
                        NL = NL > 0 ? NL : 0;
                        Color single_color = entity.Material.Color * light.Color * NL;
                        diffuse_canvas = diffuse_canvas + single_color;

                        // Reflect component
                        Ray reflected_ray = new Ray(closestRayHit.Position + closestRayHit.Reflect * 0.0001, closestRayHit.Reflect);
                        (SceneEntity closestEntity_r, RayHit closestRayHit_r) = GetClosest(reflected_ray);
                        if (closestEntity_r == null || recurseCount > RECURSE_MAX) {
                            continue;
                        }
                        recurseCount = recurseCount + 1;
                        Color i_ms = ColorPixel(closestEntity_r, closestRayHit_r, recurseCount, refractPath);
                        Vector3 temp = closestRayHit.Reflect.Cross(-rayHit.Incident);
                        double lambda = temp.Dot(temp)/2;
                        double beta = alpha/gamma;
                        double spec_coeff = Math.Pow(Math.Max(0, 1 - beta * lambda), gamma);
                        reflect_canvas = reflect_canvas + i_ms * spec_coeff;
                    }
                }

                Color final_color = diffuse_canvas * diffuse_weight + reflect_canvas * reflect_weight;

                return final_color;

            } else if (entity.Material.Type == Material.MaterialType.Refractive) {
                // Will NOT refract when going through triangle with back culling!
                // Requires editing SceneEntity interface, not sure if allowed

                // Don't even think about two refractive spheres touching
                // It probably won't crash, but it won't be pretty

                if (recurseCount > RECURSE_MAX) {
                    return entity.Material.Color;
                }

                // Copy the stack for reflection
                // C# copying stack code:
                // https://stackoverflow.com/a/7391388
                Stack<SceneEntity> refractPath_fresnel = new Stack<SceneEntity>(new Stack<SceneEntity>(refractPath));

                double ref_ratio;
                if (refractPath.Count == 0) {
                    // If this is the first material being refracted
                    ref_ratio = refractive_index / entity.Material.RefractiveIndex;
                    refractPath.Push(entity);
                } else if (this.CheckRefractionBound(refractPath.Peek(), entity)) {
                    // If we are entering the same material
                    // We must be exiting
                    refractPath.Pop();
                    if (refractPath.Count == 0) {
                        // Back to air
                        ref_ratio = entity.Material.RefractiveIndex / refractive_index;
                    } else {
                        // Back to the surrounding container
                        ref_ratio = entity.Material.RefractiveIndex / refractPath.Peek().Material.RefractiveIndex;
                    }
                } else if (!this.CheckRefractionBound(refractPath.Peek(), entity)) {
                        // Into another refractive container we go!
                        ref_ratio = refractPath.Peek().Material.RefractiveIndex / entity.Material.RefractiveIndex;
                        refractPath.Push(entity);
                } else {
                    // Really shouldn't happen
                    Console.WriteLine("Hmm");
                    ref_ratio = 1;
                }

                Vector3 N = rayHit.Normal.Normalized();
                Vector3 s1 = rayHit.Incident.Normalized();

                Vector3? refracted_vector = rayHit.Refract(ref_ratio);
                if (refracted_vector == null) {
                    return entity.Material.Color;
                }

                Ray refracted_ray = new Ray(rayHit.Position + (Vector3)refracted_vector * 0.0001, (Vector3)refracted_vector);
                (SceneEntity closestEntity, RayHit closestRayHit) = GetClosest(refracted_ray);

                Ray reflected_ray = new Ray(rayHit.Position + rayHit.Reflect * 0.0001, rayHit.Reflect);
                (SceneEntity closestEntity_r, RayHit closestRayHit_r) = GetClosest(reflected_ray);

                recurseCount = recurseCount + 2;

                Color refracted_color =
                    ColorPixel(closestEntity, closestRayHit, recurseCount, refractPath);

                Color reflected_color =
                    ColorPixel(closestEntity_r, closestRayHit_r, recurseCount, refractPath_fresnel);

                // Technically this assumes air -> material
                // Realistically you can't tell the difference if we're reaching recursive refrac
                // So probably should be ok
                double reflec_frac = FresnelApprox(refractive_index, entity.Material.RefractiveIndex, N, s1);

                return reflected_color * reflec_frac + refracted_color * (1 - reflec_frac);

            } else {
                return entity.Material.Color;
            }
        }

        private Boolean CheckRefractionBound(SceneEntity entity1, SceneEntity entity2) {
            // Pretty much fails any time a triangle is involved
            // But it works for spheres!
            if (entity1.Equals(entity2)) {
                return true;
            } else if (entity1.GetType() == typeof(RayTracer.Triangle) && entity1.GetType() == typeof(RayTracer.Triangle)) {
                return true;
            } else {
                return false;
            }
        }

    }
}
