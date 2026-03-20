using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a triangle in a scene represented by three vertices.
    /// </summary>
    public class Triangle : SceneEntity
    {
        private Vector3 v0, v1, v2;
        private Material material;

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
            this.material = material;
        }

        /// <summary>
        /// Determine if a ray intersects with the triangle, and if so, return hit data.
        /// </summary>
        /// <param name="ray">Ray to check</param>
        /// <returns>Hit data (or null if no intersection)</returns>
        public RayHit Intersect(Ray ray, bool exitHit) 
        {
            Vector3 normal = (v1-v0).Cross(v2-v0).Normalized();
            double dotP = ray.Direction.Dot(normal);
            // if dotproduct of normal and ray direction is 0, no possible intersection
            if (Math.Abs(dotP-0)>1e-5) {
                double t = (v1-ray.Origin).Dot(normal)/dotP;
                // Is POI inside or outside of the triangle?
                if (t>0) {
                    Vector3 POI = ray.Origin + ray.Direction*t;

                    Vector3 edge0 = v1-v0;
                    Vector3 edge1 = v2-v1;
                    Vector3 edge2 = v0-v2;

                    Vector3 C0 = POI - v0;
                    Vector3 C1 = POI - v1;
                    Vector3 C2 = POI - v2;

                    if ((normal.Dot(C0.Cross(edge0)) <= 0) && (normal.Dot(C1.Cross(edge1)) <= 0) && (normal.Dot(C2.Cross(edge2)) <= 0)) {
                        return new RayHit(POI, normal.Normalized(), ray.Direction.Normalized(), this.material);
                    } else {
                        return null;
                    }
                    // if (normal.Dot(C0.Cross(edge0))>0) {
                    //     return null;
                    // }
                    // if (normal.Dot(C1.Cross(edge1))>0) {
                    //     return null;
                    // }
                    // if (normal.Dot(C2.Cross(edge2))>0) {
                    //     return null;
                    // }
                    // return new RayHit(POI, normal.Normalized(), ray.Direction.Normalized(), this.material);
                } else {
                    return null;
                }
            } else {
                return null;
            }
        }

        /// <summary>
        /// The material of the triangle.
        /// </summary>
        public Material Material { get { return this.material; } }
    }

}
