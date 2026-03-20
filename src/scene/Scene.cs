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
            // dotnet run -- -f tests/sample_scene_1.txt -o output.png
            // iterating through each pixel
            // set origin vector and aspect ratio
            double aspectRatio = (double)outputImage.Width/(double)outputImage.Height;
            // fire a ray through each pixel, construct corresponding ray that fires into world
            //int count = 0;
            Vector3 zero = new Vector3(0,0,0);
            for (int i=0; i<outputImage.Width; i++) {
                for (int j=0; j<outputImage.Height; j++) {
                    //count ++;
                    //Console.WriteLine(count);
                    // Anti-Aliasing
                    Color summedCol = new Color(0,0,0);
                    // for horizontal grid per pixel in AA
                    for (int k=1; k<=options.AAMultiplier; k++) {
                        // for vertical grid per pixel in AA
                        for (int l=1; l<=options.AAMultiplier; l++) {                            
                            double incrementx = k*(1d/(options.AAMultiplier+1d));
                            double incrementy = l*(1d/(options.AAMultiplier+1d));
                            // scaled pixelvector
                            Vector3 scaledVec = new Vector3((2d*((i+incrementx)/outputImage.Width)-1d), (1d-2d*((j+incrementy)/outputImage.Height))/aspectRatio, 1d);
                            // fov scaled vector
                            Vector3 fovScaledVec = FOVscaled(scaledVec, aspectRatio);
                            // fire ray for every AA subpixel in pixel
                            Ray initialray = new Ray(zero, fovScaledVec);
                            // adjust ray if any camera adjustments
                            Ray centralray = cameraAdjustedRay(initialray);
                            Vector3 origin = centralray.Origin;

                            // DEPTH OF FIELD
                            // calculate focal point
                            Vector3 focalPoint = origin + centralray.Direction*options.FocalLength;
                            int samples = 10;
                            Color blurCol = new Color(0,0,0);
                            // calculate random number between -0.5 and 0.5
                            var random = new System.Random();
                            // if no sample
                            if (options.ApertureRadius == 0.0) {
                                double minLen = double.MaxValue;
                                RayHit minHit = null;
                                SceneEntity closestEntity = null;
                                // iterate through every entity
                                foreach (SceneEntity entity in this.entities) {
                                    RayHit hit = entity.Intersect(centralray, false);
                                    // find minimum hit distance with entity
                                    if (hit != null) {
                                        if ((hit.Position - centralray.Origin).LengthSq() < minLen) {
                                            minLen = (hit.Position - centralray.Origin).LengthSq();
                                            minHit = hit;
                                            closestEntity = entity;
                                        }
                                    }
                                }
                                if (minHit!=null) {
                                    summedCol += Colour(minHit, closestEntity, centralray);
                                }
                            // else if sampling
                            } else {
                                for (int m=0; m<samples; m++) {
                                    // shift each sample ray by a random
                                    double shiftx = randDouble(-0.5, 0.5)*options.ApertureRadius;
                                    double shifty = randDouble(-0.5, 0.5)*options.ApertureRadius;
                                    Vector3 newOrigin = new Vector3(origin.X+shiftx, origin.Y+shifty, 0d);
                                    Vector3 newDirection = (focalPoint - newOrigin).Normalized();
                                    Ray ray = new Ray(newOrigin, newDirection);
                                    
                                    // render closest hit entity in scene
                                    double minLen = double.MaxValue;
                                    RayHit minHit = null;
                                    SceneEntity closestEntity = null;
                                    // iterate through every entity
                                    foreach (SceneEntity entity in this.entities) {
                                        RayHit hit = entity.Intersect(ray, false);
                                        // find minimum hit distance with entity
                                        if (hit != null) {
                                            if ((hit.Position - ray.Origin).LengthSq() < minLen) {
                                                minLen = (hit.Position - ray.Origin).LengthSq();
                                                minHit = hit;
                                                closestEntity = entity;
                                            }
                                        }
                                    }
                                    if (minHit!=null) {
                                        blurCol += Colour(minHit, closestEntity, ray);
                                    }
                                }   
                                // add the averaged blur colour to summed color before AA
                                summedCol += blurCol/samples;
                            }   
                        }
                    }
                    // calculate average colour of summed up ray colours from AA
                    Color AAcol = summedCol/(double)(Math.Pow(options.AAMultiplier, 2));
                    outputImage.SetPixel(i, j, AAcol);
                }
            }
        }
        // calculates adjusted ray based on camera options
        public Ray cameraAdjustedRay(Ray ray) {
            // fetch camera options
            Vector3 axis = options.CameraAxis;
            double a = (options.CameraAngle)*(Math.PI/180);
            double[] p = new double[4]{0, ray.Direction.X, ray.Direction.Y, ray.Direction.Z};
            // calculate quaternions
            double[] r = new double[4]{Math.Cos(a/2), Math.Sin(a/2)*axis.X, Math.Sin(a/2)*axis.Y, Math.Sin(a/2)*axis.Z};
            double[] rd = new double[4]{Math.Cos(a/2), -Math.Sin(a/2)*axis.X, -Math.Sin(a/2)*axis.Y, -Math.Sin(a/2)*axis.Z};
            // apply hamilton transformation
            double[] rp = Hamilton(r, p);
            double[] rprd = Hamilton(rp, rd);
            // new direction;
            Vector3 newCamDir = new Vector3(rprd[1], rprd[2], rprd[3]);
            // return transformed ray
            Ray newRay = new Ray(ray.Origin + options.CameraPosition, newCamDir.Normalized());
            return newRay;
        }
        // calculates hamilton product between 2 quaternions
        public double[] Hamilton(double[] r, double[] rd) {
            double a = r[0]*rd[0] - r[1]*rd[1] - r[2]*rd[2] - r[3]*rd[3];
            double b = r[0]*rd[1] + r[1]*rd[0] + r[2]*rd[3] - r[3]*rd[2];
            double c = r[0]*rd[2] - r[1]*rd[3] + r[2]*rd[0] + r[3]*rd[1];
            double d = r[0]*rd[3] + r[1]*rd[2] - r[2]*rd[1] + r[3]*rd[0];
            return new double[4]{a, b, c, d};
        }
        // scales vector to vertical and horizontal FOV
        public Vector3 FOVscaled(Vector3 scaledVec, double aspectRatio) {
            double FOV = Math.PI/3;
            double VFOV = 2*Math.Atan(Math.Tan(FOV/2)*aspectRatio);
            double fovscaledX = scaledVec.X*Math.Tan(FOV/2);
            double fovscaledY = scaledVec.Y*(Math.Tan(VFOV/2))/aspectRatio;
            return new Vector3(fovscaledX, fovscaledY, 1.0d).Normalized();
        }
        // generate random double in range, referenced from code-maze
        public static double randDouble(double lowerBound, double upperBound) {
            var random = new Random();
            var rDouble = random.NextDouble();
            var rRangeDouble = rDouble * (upperBound - lowerBound) + lowerBound;
            return rRangeDouble;
        }
        public Color Colour(RayHit hit, SceneEntity entity, Ray ray) {
            Color finalCol = new Color(0,0,0);
            int MAX_DEPTH = 5;
            if (entity.Material.Type == Material.MaterialType.Diffuse) {
                foreach(PointLight light in lights)  {
                    bool shadow = Shadow(light, hit);
                    if (shadow == true) {continue;}
                    // otherwise, sum up diffused colors 
                    finalCol += DiffuseCol(light, hit, entity);
                }
            // recursively trace ray if surface is reflective
            } else if (entity.Material.Type == Material.MaterialType.Reflective) {
                finalCol = reflectRay(hit, 0, MAX_DEPTH);
            // recursively trace ray if surface is refractive (with fresnel effect)
            } else if (entity.Material.Type == Material.MaterialType.Refractive) {
                finalCol = refractRay(hit, 0, MAX_DEPTH, entity);
            // glossy material, both reflective and diffusive
            } else if (entity.Material.Type == Material.MaterialType.Glossy) {
                finalCol = glossCol(hit, 0, MAX_DEPTH, entity);
            }
            return finalCol;
        }
        // check if a shadow exists for the pixel
        public bool Shadow(PointLight light, RayHit hit) {
            bool shad = false;
            Vector3 toLight = light.Position - hit.Position;
            Vector3 hitAdj = hit.Position + 1e-5*(toLight.Normalized());
            // fire a reverse shadow ray
            Ray shadowRay = new Ray(hitAdj, toLight.Normalized());
            // check each entity, to see if reverse ray collides with closer entity
            foreach(SceneEntity entity2 in this.entities) {
                RayHit hit2 = entity2.Intersect(shadowRay, false);
                if (hit2!=null) {
                    // if it is a shadow ray, color of pixel is black
                    if ((hit2.Position - hit.Position).LengthSq() < (light.Position - hit.Position).LengthSq()) {
                        shad = true;
                        break;
                    } 
                }
            }
            if (shad == true) {
                return true;
            }
            return false;
        }
        // calculate diffused colour with light
        public Color DiffuseCol(PointLight light, RayHit hit, SceneEntity entity) {
            Vector3 lightDir = (light.Position - hit.Position).Normalized();
            double factor;
            if (hit.Normal.Dot(lightDir) < 0) {
                factor = 0;
            } else {
                factor = hit.Normal.Dot(lightDir);
            }
            Color lightCol = entity.Material.Color*light.Color;
            Color lightColFactored = new Color(lightCol.R*factor, lightCol.G*factor, lightCol.B*factor);
            return lightColFactored;
        }
        // function to generate refracted ray 
        public Ray refractedRay(RayHit hit, double refractiveIndex) {
            Vector3 normal = hit.Normal;
            double NdotI = normal.Dot(hit.Incident.Normalized());
            double eta = 0.0;
            if (NdotI < 0) {
                NdotI = -NdotI;
                eta = 1.0/refractiveIndex;
            } else {
                normal = -normal;
                eta = refractiveIndex/1.0;
            }
            // reference scratchapixel
            double k = 1 - eta*eta*(1-(NdotI)*(NdotI));
            if (k<0) {
                // null?
                return new Ray(new Vector3(0,0,0), new Vector3(0,0,0));
            } else {
                Vector3 refrDir = eta*hit.Incident.Normalized() + (eta*NdotI - Math.Sqrt(k))*normal;
                return new Ray(hit.Position + 1e-5*refrDir, refrDir);
            }
        }
        // recursive function to return pixel colour with refractive material
        public Color refractRay(RayHit hit, int depth, int MAX_DEPTH, SceneEntity entity) {
            // check less than max depth
            if (depth == MAX_DEPTH) {
                return new Color(0,0,0);
            }
            // calculated refracted ray into refractive entity
            Ray refrRay = refractedRay(hit, entity.Material.RefractiveIndex);
            // fresnel effect, referenced from scratchapixel
            Vector3 normal = hit.Normal;
            double NdotI = normal.Dot(hit.Incident.Normalized());
            double etai = 0.0, etat = 0.0;
            // check if air->entity or entity->air
            if (NdotI < 0) {
                NdotI = -NdotI;
                etai = 1.0;
                etat = entity.Material.RefractiveIndex;
            } else {
                normal = -normal;
                etat = 1.0;
                etai = entity.Material.RefractiveIndex;
            }
            // calculate min hit (could be with entity or itself)
            RayHit minHit = null;
            SceneEntity minEnt = null;
            double maxDist = double.MaxValue;
            foreach(SceneEntity entity2 in this.entities) {
                // also calculate distance travelled inside
                RayHit hit2 = entity2.Intersect(refrRay, false);
                if (hit2!=null) {
                    double hitdist = (hit2.Position - refrRay.Origin).LengthSq();
                    if (hitdist<maxDist) {
                        maxDist = hitdist;
                        minHit = hit2;
                        minEnt = entity2;
                    }
                }
            }
            // FRESNEL CALCULATIONS
            // setup variables to calculate fresnel effect
            double kr = 0.0;
            double cosi = hit.Incident.Dot(hit.Normal);
            double sint = (etai/etat)*Math.Sqrt(Math.Max(0d, 1-(cosi)*(cosi)));
            if (cosi<-1) {
                cosi = -1;
            } else if (cosi > 1) {
                cosi = 1;
            }
            if (sint>=1) {
                kr = 1.0;
            // calculate proportion in fresnel
            } else {
                double cost = Math.Sqrt(Math.Max(0d, 1-sint*sint));
                cosi = Math.Abs(cosi);
                double Rs = ((etat * cosi) - (etai * cost)) / ((etat * cosi) + (etai * cost)); 
                double Rp = ((etai * cosi) - (etat * cost)) / ((etai * cosi) + (etat * cost));
                kr = (Rs * Rs + Rp * Rp) / 2; 
            }
            // colours for fresnel
            Color refrCol = new Color(0,0,0);
            Color reflCol = new Color(0,0,0);
            Color refractedCol = new Color(0,0,0);
            // calculate reflectedColor
            if (kr < 1) {
                reflCol = reflectRay(hit, depth, MAX_DEPTH);
            }
            // calculate refractedColor
            if (minHit!=null) {
                if (minEnt.Material.Type == Material.MaterialType.Reflective) {
                    depth++;
                    refrCol += reflectRay(minHit, depth, MAX_DEPTH);
                } else if (minEnt.Material.Type == Material.MaterialType.Refractive) {
                    depth++;
                    refrCol += refractRay(minHit, depth, MAX_DEPTH, minEnt);
                } else {
                    refrCol += Colour(minHit, minEnt, refrRay);
                }
            } 
            // Calculate fresnel'd Colour
            refractedCol = reflCol*kr + refrCol*(1 - kr);
            return refractedCol;
        }   
        // recursive function to return pixel colour with reflective material
        public Color reflectRay(RayHit hit, int depth, int MAX_DEPTH) {
            // reflected direction
            Vector3 refDir = (hit.Incident - 2*((hit.Incident.Dot(hit.Normal))*hit.Normal)).Normalized();
            double epsilon = 1e-5;
            // set reflected ray
            Vector3 origin = hit.Position + epsilon*(refDir);
            Ray reflectedRay = new Ray(origin, refDir);
            // check for collision with other entities
            double maxDist = double.MaxValue;
            RayHit minHit = null;
            SceneEntity minEnt = null;
            foreach(SceneEntity entity in this.entities) {
                RayHit reflectedhit = entity.Intersect(reflectedRay, false);
                // no hit at all
                if (reflectedhit!=null) {
                    double hitdist = (reflectedhit.Position - origin).LengthSq();
                    // set collision for shortest hit and valid entity   
                    if (hitdist<maxDist) {
                        maxDist = hitdist;
                        minHit = reflectedhit;
                        minEnt = entity;
                    }
                }
            }
            // no hit, return black
            if (minHit != null) {
                if (depth == MAX_DEPTH) {
                    if ((minEnt.Material.Type == Material.MaterialType.Reflective) 
                    || (minEnt.Material.Type == Material.MaterialType.Refractive) || 
                    (minEnt.Material.Type == Material.MaterialType.Glossy)) {
                        return new Color(0,0,0);
                    } else {
                        return Colour(minHit, minEnt, reflectedRay);
                    }
                } else if (minEnt.Material.Type == Material.MaterialType.Reflective) {
                    depth++;
                    return reflectRay(minHit, depth, MAX_DEPTH);
                } else if (minEnt.Material.Type == Material.MaterialType.Refractive) {
                    depth++;
                    return refractRay(minHit, depth, MAX_DEPTH, minEnt);
                } else if (minEnt.Material.Type == Material.MaterialType.Glossy) {
                    depth++;
                    return glossCol(minHit, depth, MAX_DEPTH, minEnt);
                } else {
                    return Colour(minHit, minEnt, reflectedRay);
                }
            } 
            return new Color(0,0,0);
        }
        // calculates colour of glossy material pixel
        public Color glossCol(RayHit hit, int depth, int MAX_DEPTH, SceneEntity entity) {
            Color diffuse = new Color(0,0,0);
            Color specular = new Color(0,0,0);
            Color glossy = new Color(0,0,0);
            if (depth == MAX_DEPTH) {
                return new Color(0,0,0);
            }
            foreach(PointLight light in lights)  {
                bool shadow = Shadow(light, hit);
                if (shadow == true) {continue;}
                // diffuse component
                diffuse += DiffuseCol(light, hit, entity);
            }
            foreach(PointLight light in lights) {
                bool shadow = Shadow(light, hit);
                if (shadow == true) {continue;}
                // reflect direction and light direction
                Vector3 refDir = (hit.Incident - 2*((hit.Incident.Dot(hit.Normal))*hit.Normal)).Normalized();
                Vector3 hitAdj = hit.Position + 1e-5*(refDir);
                Vector3 lightDir = (hitAdj - light.Position).Normalized();
                double pow = Math.Pow(Math.Max(0d, refDir.Dot(-lightDir)), 30);
                // specular highlight
                specular += reflectRay(hit, depth, MAX_DEPTH)*pow;
            }
            return (diffuse*0.55 + specular*0.45);
        }
    }
}
