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
        public RayHit Intersect(Ray ray, bool exitHit)
        {
            // variables
            double t0, t1;
            Vector3 LentoCenter = ray.Origin-center;
            double a = ray.Direction.Dot(ray.Direction);
            double b = 2*ray.Direction.Dot(LentoCenter);
            double c = LentoCenter.Dot(LentoCenter) - radius*radius;
            
            // both intersections of ray with sphere
            if (b*b-4*a*c > 0) {
                t0 = (-b+Math.Sqrt(b*b-4*a*c))/2*a;
                t1 = (-b-Math.Sqrt(b*b-4*a*c))/2*a;
            // if only one intersection
            } else if (b*b - 4*a*c == 0) {
                t0 = t1 = -b/2*a;
            } else {
                return null;
            }
            
            // swap if wrong way around
            if (t0 > t1) {
                double temp = 0;
                temp = t1;
                t1 = t0;
                t0 = temp;
            }
            // check if solution is in front
            if (t0 < 0) {
                t0 = t1;
                if (t0 < 0) {
                    return null;
                }
            }
            // set entry and exit points
            double entry = t0;
            double exit = t1;
            if (entry > 0 && exit > 0) {
                Vector3 POI = ray.Origin + ray.Direction*entry;
                Vector3 POIexit = ray.Origin + ray.Direction*exit;
                Vector3 normal = (POI - center).Normalized();
                if (exitHit) {
                    return new RayHit(POIexit, normal, ray.Direction.Normalized(), this.material);
                } else {
                    return new RayHit(POI, normal, ray.Direction.Normalized(), this.material);
                }
            } else {
                return null;
            }
            // NOTE FOR LATER
            // CALCULATE OTHER INTERSECTION AND RETURN (FOR REFRACTION)
        }

        /// <summary>
        /// The material of the sphere.
        /// </summary>
        public Material Material { get { return this.material; } }
    }

}
