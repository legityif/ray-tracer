using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent an (infinite) plane in a scene.
    /// </summary>
    public class Plane : SceneEntity
    {
        private Vector3 center;
        private Vector3 normal;
        private Material material;

        /// <summary>
        /// Construct an infinite plane object.
        /// </summary>
        /// <param name="center">Position of the center of the plane</param>
        /// <param name="normal">Direction that the plane faces</param>
        /// <param name="material">Material assigned to the plane</param>
        public Plane(Vector3 center, Vector3 normal, Material material)
        {
            this.center = center;
            this.normal = normal.Normalized();
            this.material = material;
        }

        /// <summary>
        /// Determine if a ray intersects with the plane, and if so, return hit data.
        /// </summary>
        /// <param name="ray">Ray to check</param>
        /// <returns>Hit data (or null if no intersection)</returns>
        public RayHit Intersect(Ray ray, bool exitHit)
        {
            // calculate normal dotproduct with direction of ray
            double dotP = ray.Direction.Dot(this.normal);
            // if dotproduct IS NOT 0, then HIT
            if (Math.Abs(dotP-0)>1e-5) {
                // t = amount of units of ray before collision
                double t = (this.normal).Dot(this.center - ray.Origin)/dotP;
                if (t>0) {
                    Vector3 POI = ray.Origin + ray.Direction*t;
                    return new RayHit(POI, this.normal.Normalized(), ray.Direction.Normalized(), this.material);
                }
            } 
            return null;
        }

        /// <summary>
        /// The material of the plane.
        /// </summary>
        public Material Material { get { return this.material; } }
    }

}
