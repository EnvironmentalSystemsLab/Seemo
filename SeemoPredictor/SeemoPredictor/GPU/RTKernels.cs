using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Algorithms.Random;
using SeemoPredictor.Geometry;

namespace SeemoPredictor.GPU
{
    public class RTKernels
    {
        public static void RenderKernel(Index2D id,
            dFramebuffer framebuffer,
            dWorldBuffer world,
            SmoImage smoImage, int rngOffset)
        {
            int x = id.X;
            int y = id.Y;

            int index = ((y * smoImage.xres) + x);

            //there is probably a better way to do this, but it seems to work. seed = the tick * a large prime xor (index + 1) * even larger prime
            XorShift64Star rng = new XorShift64Star((((ulong)(rngOffset + 1) * 3727177) ^ ((ulong)(index + 1) * 113013596393)));
            //XorShift64Star rng = new XorShift64Star();

            Ray ray = smoImage.GetRay(x + rng.NextFloat(), y + rng.NextFloat());

            ColorRay(index, ray, framebuffer, world, rng, smoImage);
        }

        private static void ColorRay(int index,
            Ray ray,
            dFramebuffer framebuffer,
            dWorldBuffer world,
            XorShift64Star rng, SmoImage smoImage)
        {
            Point3 attenuation = new Point3(1f, 1f, 1f);
            Point3 lighting = new Point3();

            Ray working = ray;
            bool attenuationHasValue = false;

            float minT = 0.1f;

            for (int i = 0; i < smoImage.maxBounces; i++)
            {
                HitRecord rec = GetWorldHit(working, world, minT);

                if (rec.materialID == -1)
                {
                    if (i == 0 || attenuationHasValue)
                    {
                        framebuffer.LabelIDBuffer[index] = -2;
                    }

                    float t = 0.5f * (working.b.y + 1.0f);
                    attenuation *= (1.0f - t) * new Point3(1.0f, 1.0f, 1.0f) + t * new Point3(0.5f, 0.7f, 1.0f);
                    break;
                }
                else
                {
                    if (i == 0)
                    {
                        framebuffer.DepthBuffer[index] = rec.t;
                    }

                    ScatterRecord sRec = Scatter(working, rec, rng, world.materials, minT);
                    if (sRec.materialID != -1)
                    {
                        attenuationHasValue = sRec.mirrorSkyLightingFix;
                        attenuation *= sRec.attenuation;
                        working = sRec.scatterRay;
                    }
                    else
                    {
                        framebuffer.LabelIDBuffer[index] = -1;
                        break;
                    }
                }

                for (int j = 0; j < world.lightSphereIDs.Length; j++)
                {
                    Sphere s = world.spheres[world.lightSphereIDs[j]];
                    Point3 lightDir0 = s.center - rec.p;
                    HitRecord shadowRec = GetWorldHit(new Ray(rec.p, lightDir0), world, minT);

                    if (shadowRec.materialID != -1 && (shadowRec.p - rec.p).Length > lightDir0.Length - (s.radius * 1.1f)) // the second part of this IF could probably be much more efficent
                    {
                        MaterialData material = world.materials[shadowRec.materialID];
                        if (material.type != 1)
                        {
                            Point3 lightDir = lightDir0;
                            lightDir.Normalize();
                            lighting += material.color * XMath.Max(0.0f, Point3.Dot(lightDir, rec.normal));
                            lighting *= XMath.Pow(XMath.Max(0.0f, Point3.Dot(-Point3.reflect(rec.normal, -lightDir), ray.Direction)), material.reflectivity) * material.color;
                        }
                    }
                }
            }

            int rIndex = index * 3;
            int gIndex = rIndex + 1;
            int bIndex = rIndex + 2;

            framebuffer.RayBuffer[rIndex] = attenuation.X;
            framebuffer.RayBuffer[gIndex] = attenuation.Y;
            framebuffer.RayBuffer[bIndex] = attenuation.Z;

            framebuffer.HitsBuffer[rIndex] = lighting.X;
            framebuffer.HitsBuffer[gIndex] = lighting.Y;
            framebuffer.HitsBuffer[bIndex] = lighting.Z;
        }


        private static Point3 RandomUnitVector(XorShift64Star rng)
        {
            float a = 2f * XMath.PI * rng.NextFloat();
            float z = (rng.NextFloat() * 2f) - 1f;
            float r = XMath.Sqrt(1f - z * z);
            return new Point3(r * XMath.Cos(a), r * XMath.Sin(a), z);
        }

        //important calculation
        private static HitRecord GetWorldHit(Ray r, dWorldBuffer world, float minT)
        {
            HitRecord rec = GetSphereHit(r, world.spheres, minT);
            HitRecord vRec = world.VoxelChunk.hit(r, minT, rec.t);
            HitRecord triRec = GetMeshHit(r, world, vRec.t);

            if (rec.t < vRec.t && rec.t < triRec.t)
            {
                return rec;
            }
            else if (vRec.t < rec.t && vRec.t < triRec.t)
            {
                return vRec;
            }
            else
            {
                return triRec;
            }
        }


        private static HitRecord GetSphereHit(Ray r, ArrayView<Sphere> spheres, float minT)
        {
            float closestT = 10000;
            int sphereIndex = -1;

            Sphere s;
            Point3 oc;

            for (int i = 0; i < spheres.Length; i++)
            {
                s = spheres[i];
                oc = r.a - s.center;

                float b = Point3.Dot(oc, r.Direction);
                float c = Point3.Dot(oc, oc) - s.radiusSquared;
                float discr = (b * b) - (c);

                if (discr > 0.1f)
                {
                    float sqrtdisc = XMath.Sqrt(discr);
                    float temp = (-b - sqrtdisc);
                    if (temp < closestT && temp > minT)
                    {
                        closestT = temp;
                        sphereIndex = i;
                    }
                    else
                    {
                        temp = (-b + sqrtdisc);
                        if (temp < closestT && temp > minT)
                        {
                            closestT = temp;
                            sphereIndex = i;
                        }
                    }
                }
            }

            if (sphereIndex != -1)
            {
                oc = r.PointAtParameter(closestT);
                s = spheres[sphereIndex];
                return new HitRecord(closestT, oc, (oc - s.center) / s.radius, r.b, s.materialIndex);
            }
            else
            {
                return new HitRecord(float.MaxValue, new Point3(), new Point3(), false, -1);
            }
        }


        private static HitRecord GetMeshHit(Ray r, dWorldBuffer world, float nearerThan)
        {
            float dist = nearerThan;
            HitRecord rec = new HitRecord(float.MaxValue, new Point3(), new Point3(), false, -1);

            for (int i = 0; i < world.meshes.Length; i++)
            {
                if (world.meshes[i].aabb.hit(r, nearerThan, dist))
                {
                    HitRecord meshHit = GetTriangleHit(r, world, world.meshes[i], dist);
                    if (meshHit.t < dist)
                    {
                        dist = meshHit.t;
                        rec = meshHit;
                    }
                }
            }

            return rec;
        }

        //Important hit algorithm
        private static HitRecord GetTriangleHit(Ray r, dWorldBuffer world, dGPUMesh mesh, float nearerThan)
        {
            SmoFace t = new SmoFace();
            float currentNearestDist = nearerThan;
            int NcurrentIndex = -1;
            int material = 0;
            float Ndet = 0;

            for (int i = 0; i < mesh.triangleCount; i++)
            {
                t = mesh.GetTriangle(i, world);
                Point3 tuVec = t.uVector();
                Point3 tvVec = t.vVector();
                Point3 pVec = Point3.Cross(r.Direction, tvVec);
                float det = Point3.Dot(tuVec, pVec);

                if (XMath.Abs(det) > nearerThan)
                {
                    float invDet = 1.0f / det;
                    Point3 tVec = r.Origin - t.Vert0;
                    float u = Point3.Dot(tVec, pVec) * invDet;
                    Point3 qVec = Point3.Cross(tVec, tuVec);
                    float v = Point3.Dot(r.Direction, qVec) * invDet;

                    if (u > 0 && u <= 1.0f && v > 0 && u + v <= 1.0f)
                    {
                        float temp = Point3.Dot(tvVec, qVec) * invDet;
                        if (temp > nearerThan && temp < currentNearestDist)
                        {
                            currentNearestDist = temp;
                            NcurrentIndex = i;
                            Ndet = det;
                            material = t.MaterialID;
                        }
                    }
                }
            }

            if (NcurrentIndex == -1)
            {
                return new HitRecord(float.MaxValue, new Point3(), new Point3(), false, -1);
            }
            else
            {
                if (Ndet < 0)
                {
                    return new HitRecord(currentNearestDist, r.PointAtParameter(currentNearestDist), -t.faceNormal(), true, material);
                }
                else
                {
                    return new HitRecord(currentNearestDist, r.PointAtParameter(currentNearestDist), t.faceNormal(), false, material);
                }
            }
        }
    }
}
