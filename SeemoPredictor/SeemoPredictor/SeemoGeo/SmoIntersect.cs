using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeemoPredictor.SeemoGeo
{
    public class SmoIntersect
    {

        private static double EPSILON = 0.0000001;

        public static void MeshRayResultSave(ref ResultDataSet_ver2 result, SmoPointOctree<SmoFace> octree, SmoPoint3 pt)
        {
            int z1hit = result.sceneRayVectorsZ1.Count;
            int z2hit = result.sceneRayVectorsZ2.Count;
            int z3hit = result.sceneRayVectorsZ3.Count;
            int z4hit = result.sceneRayVectorsZ4.Count;

            List<double> buildingDists = new List<double>();
            List<double> equipmentDists = new List<double>();
            List<double> treeDists = new List<double>();
            List<double> pavementDists = new List<double>();
            List<double> grassDists = new List<double>();
            List<double> waterDists = new List<double>();
            List<double> dynamicDists = new List<double>();
            List<double> skyDists = new List<double>();


            //shoot z1 rays
            foreach (SmoPoint3 ray in result.sceneRayVectorsZ1)
            {
                SmoPoint3 hit = new SmoPoint3();
                var type = SmoIntersect.IsObstructed(ref hit, octree, pt, ray, 0.01f);
                double dist = SmoPoint3.Distance(hit, pt);

                switch (type)
                {
                    case SmoFace.SmoFaceType.Room:
                        z1hit--;
                        break;
                    case SmoFace.SmoFaceType.Building:
                        buildingDists.Add(dist);
                        break;
                    case SmoFace.SmoFaceType.Equipment:
                        equipmentDists.Add(dist);
                        break;
                    case SmoFace.SmoFaceType.Tree:
                        treeDists.Add(dist);
                        break;
                    case SmoFace.SmoFaceType.Pavement:
                        pavementDists.Add(dist);
                        break;
                    case SmoFace.SmoFaceType.Grass:
                        grassDists.Add(dist);
                        break;
                    case SmoFace.SmoFaceType.Water:
                        waterDists.Add(dist);
                        break;
                    case SmoFace.SmoFaceType.Dynamics:
                        dynamicDists.Add(dist);
                        break;
                    case SmoFace.SmoFaceType.Sky:
                        skyDists.Add(dist);
                        break;
                }
            }

            //shoot z2 rays
            foreach (SmoPoint3 ray in result.sceneRayVectorsZ2)
            {
                SmoPoint3 hit = new SmoPoint3();
                var type = SmoIntersect.IsObstructed(ref hit, octree, pt, ray, 0.01f);
                double dist = SmoPoint3.Distance(hit, pt);

                switch (type)
                {
                    case SmoFace.SmoFaceType.Room:
                        z2hit--;
                        break;
                    case SmoFace.SmoFaceType.Building:
                        buildingDists.Add(dist);
                        break;
                    case SmoFace.SmoFaceType.Equipment:
                        equipmentDists.Add(dist);
                        break;
                    case SmoFace.SmoFaceType.Tree:
                        treeDists.Add(dist);
                        break;
                    case SmoFace.SmoFaceType.Pavement:
                        pavementDists.Add(dist);
                        break;
                    case SmoFace.SmoFaceType.Grass:
                        grassDists.Add(dist);
                        break;
                    case SmoFace.SmoFaceType.Water:
                        waterDists.Add(dist);
                        break;
                    case SmoFace.SmoFaceType.Dynamics:
                        dynamicDists.Add(dist);
                        break;
                    case SmoFace.SmoFaceType.Sky:
                        skyDists.Add(dist);
                        break;
                }
            }

            //shoot z3 rays
            foreach (SmoPoint3 ray in result.sceneRayVectorsZ3)
            {
                SmoPoint3 hit = new SmoPoint3();
                var type = SmoIntersect.IsObstructed(ref hit, octree, pt, ray, 0.01f);
                double dist = SmoPoint3.Distance(hit, pt);

                switch (type)
                {
                    case SmoFace.SmoFaceType.Room:
                        z3hit--;
                        break;
                    case SmoFace.SmoFaceType.Building:
                        buildingDists.Add(dist);
                        break;
                    case SmoFace.SmoFaceType.Equipment:
                        equipmentDists.Add(dist);
                        break;
                    case SmoFace.SmoFaceType.Tree:
                        treeDists.Add(dist);
                        break;
                    case SmoFace.SmoFaceType.Pavement:
                        pavementDists.Add(dist);
                        break;
                    case SmoFace.SmoFaceType.Grass:
                        grassDists.Add(dist);
                        break;
                    case SmoFace.SmoFaceType.Water:
                        waterDists.Add(dist);
                        break;
                    case SmoFace.SmoFaceType.Dynamics:
                        dynamicDists.Add(dist);
                        break;
                    case SmoFace.SmoFaceType.Sky:
                        skyDists.Add(dist);
                        break;
                }
            }

            //shoot z4 rays
            foreach (SmoPoint3 ray in result.sceneRayVectorsZ4)
            {
                SmoPoint3 hit = new SmoPoint3();
                var type = SmoIntersect.IsObstructed(ref hit, octree, pt, ray, 0.01f);
                double dist = SmoPoint3.Distance(hit, pt);

                switch (type)
                {
                    case SmoFace.SmoFaceType.Room:
                        z4hit--;
                        break;
                    case SmoFace.SmoFaceType.Building:
                        buildingDists.Add(dist);
                        break;
                    case SmoFace.SmoFaceType.Equipment:
                        equipmentDists.Add(dist);
                        break;
                    case SmoFace.SmoFaceType.Tree:
                        treeDists.Add(dist);
                        break;
                    case SmoFace.SmoFaceType.Pavement:
                        pavementDists.Add(dist);
                        break;
                    case SmoFace.SmoFaceType.Grass:
                        grassDists.Add(dist);
                        break;
                    case SmoFace.SmoFaceType.Water:
                        waterDists.Add(dist);
                        break;
                    case SmoFace.SmoFaceType.Dynamics:
                        dynamicDists.Add(dist);
                        break;
                    case SmoFace.SmoFaceType.Sky:
                        skyDists.Add(dist);
                        break;
                }
            }

            //calculate ml inputs
            int zhitSum = (z1hit + z2hit + z3hit + z4hit);
            result.WindowAreaSum = (zhitSum / (5288.02083158));
            result.Z1PtsCountRatio = (z1hit) / zhitSum;
            result.Z2PtsCountRatio = (z2hit) / zhitSum;
            result.Z3PtsCountRatio = (z3hit) / zhitSum;
            result.Z4PtsCountRatio = (z4hit) / zhitSum;

            //***********Without WindowMesh How can I count window number???
            ////Previous Method: Count how many window are visible
            ////int[] winIhit;
            //List<int> hitWindows = new List<int>();

            //for (int i = 0; i < winRayVectors0.Count; i++)
            //{
            //    Vector3d ray = winRayVectors0[i];
            //    ray.Unitize();
            //    Ray3d ray1 = new Ray3d(vp, ray);

            //    for (int q = 0; q < windowMeshs.Count; q++)
            //    {
            //        var dtr = Rhino.Geometry.Intersect.Intersection.MeshRay(windowMeshs[q], ray1);
            //        if (dtr > 0)
            //        {
            //            hitWindows.Add(q);
            //        }
            //    }
            //}
            //hitWindows = hitWindows.Distinct().ToList();
            //wn = hitWindows.Count;
            result.WindowNumber = 2;
            result.SkyCondition = 1;

            result.BuildingPtsCountRatio = (buildingDists.Count / zhitSum);
            result.EquipmentPtsCountRatio = (equipmentDists.Count / zhitSum);
            result.TreePtsCountRatio = (treeDists.Count / zhitSum);
            result.PavementPtsCountRatio = (pavementDists.Count / zhitSum);
            result.GrassPtsCountRatio = (grassDists.Count / zhitSum);
            result.WaterPtsCountRatio = (waterDists.Count / zhitSum);
            result.DynamicPtsRatio = (dynamicDists.Count / zhitSum);
            result.SkyPtsCountRatio = (skyDists.Count / zhitSum);

            //calculate object's closest distance and type
            double ComputeClosestDist(List<double> dists)
            {
                double closeDistance = 0;
                if (dists.Count == 0)
                {
                    return 50000; //50000에서 바꿈 //double.NaN
                }
                else
                {
                    dists.Sort();
                    for (int i = 0; i < dists.Count * 0.3 + 0.001; i++)
                    {
                        closeDistance = closeDistance + dists[i];
                    }
                    closeDistance = closeDistance / (Math.Ceiling(dists.Count * 0.3));
                    return closeDistance;
                }
            }

            double buildingClosestDist = ComputeClosestDist(buildingDists);
            double equipmentClosestDist = ComputeClosestDist(equipmentDists);
            double treeClosestDist = ComputeClosestDist(treeDists);
            //double pavementClosestDist = ComputeClosestDist(pavementDists);
            double grassClosestDist = ComputeClosestDist(grassDists);
            double waterClosestDist = ComputeClosestDist(waterDists);
            double dynamicClosestDist = ComputeClosestDist(dynamicDists);

            result.BuildingClosestDist = buildingClosestDist;
            result.EquipmentClosestDist = equipmentClosestDist;
            result.TreeClosestDist = treeClosestDist;
            result.GrassClosestDist = grassClosestDist;
            result.WaterClosestDist = waterClosestDist;
            result.DynamicClosestDist = dynamicClosestDist;


            //calculate visible element types count
            List<int> elementPts = new List<int> { buildingDists.Count, equipmentDists.Count, treeDists.Count, pavementDists.Count, grassDists.Count, waterDists.Count, dynamicDists.Count, skyDists.Count };
            
            int elementNumber = 0;
            for (int i = 0; i < elementPts.Count; i++)
            {
                if (elementPts[i] > 10)
                {
                    elementNumber++;
                }
            }

            result.ElementNumber = elementNumber;

        }

        public static SmoFace.SmoFaceType IsObstructed(ref SmoPoint3 hitPt, SmoPointOctree<SmoFace> octree, SmoPoint3 pt, SmoPoint3 raySmoPoint3, double maxNodeSize)
        {
            var ray = new SmoRay(pt, raySmoPoint3);
            var testGeo = octree.GetNearby(ray, (float)maxNodeSize);

            foreach (var g in testGeo)
            {
                SmoPoint3 ipt1;
                SmoPoint3 ipt2;
                bool i1 = false;
                bool i2 = false;

                SmoFace.SmoFaceType type = g.Material;

                i1 = SmoIntersect.RayTriangle_MollerTrumbore(ray, g.VertexList[0], g.VertexList[1], g.VertexList[2], out ipt1);

                if (i1) 
                {
                    hitPt = ipt1;
                    return type; 
                }

                if (g.IsQuad)
                {
                    i2 = SmoIntersect.RayTriangle_MollerTrumbore(ray, g.VertexList[0], g.VertexList[2], g.VertexList[3], out ipt2);
                    if (i2) 
                    {
                        hitPt = ipt2;
                        return type; 
                    }
                }
            }
            return SmoFace.SmoFaceType._UNSET_;
        }

        public static bool IsObstructedBool(SmoPointOctree<SmoFace> octree, SmoPoint3 pt, SmoPoint3 vd, double maxNodeSize)
        {
            var ray = new SmoRay(pt, vd);
            var testGeo = octree.GetNearby(ray, (float)maxNodeSize);

            foreach (var g in testGeo)
            {

                SmoPoint3 ipt1;
                SmoPoint3 ipt2;
                bool i1 = false;
                bool i2 = false;

                i1 = SmoIntersect.RayTriangle_MollerTrumbore(ray, g.VertexList[0], g.VertexList[1], g.VertexList[2], out ipt1);

                if (i1) { return i1; }

                if (g.IsQuad)
                {
                    i2 = SmoIntersect.RayTriangle_MollerTrumbore(ray, g.VertexList[0], g.VertexList[2], g.VertexList[3], out ipt2);
                    if (i2) { return i2; }
                }

                SmoFace.SmoFaceType type = g.Material;
            }
            return false;
        }

        public static bool RayTriangle_MollerTrumbore(SmoRay ray,SmoPoint3 vertex0, SmoPoint3 vertex1, SmoPoint3 vertex2, out SmoPoint3 outIntersectionPoint)
        {
            SmoPoint3 rayOrigin = ray.Origin;
            SmoPoint3 rayVector = ray.Direction;

            SmoPoint3 q = new SmoPoint3();
            double f, u, v;
            SmoPoint3 edge1 = (vertex1 - vertex0);
            SmoPoint3 edge2 = (vertex2 - vertex0);
            SmoPoint3 h = SmoPoint3.Cross(rayVector, edge2);

            double a = SmoPoint3.Dot(edge1, h); // dot product

            if(a > -EPSILON && a < EPSILON)
            {
                outIntersectionPoint = new SmoPoint3();
                return false;  //This ray in parallel to this triangle.
            }
            f = 1.0 / a;
            SmoPoint3 s = rayOrigin - vertex0;
            u = f * SmoPoint3.Dot(s, h);
            if(u < 0.0 || u > 1.0)
            {
                outIntersectionPoint = new SmoPoint3();
                return false;
            }
            q = SmoPoint3.Cross(s, edge1);
            v = f * SmoPoint3.Dot(rayVector, q);
            if ( v < 0.0 || u + v > 1.0)
            {
                outIntersectionPoint = new SmoPoint3();
                return false;
            }
            q = SmoPoint3.Cross(s, edge1);
            v = f * SmoPoint3.Dot(rayVector, q);
            if (v < 0.0 || u + v > 1.0)
            {
                outIntersectionPoint = new SmoPoint3();
                return false;
            }
            // At this stage we can compute t to find out where the intersection point is on the line.
            double t = f * SmoPoint3.Dot(edge2, q);
            if (t > EPSILON) // ray intersection
            {
                outIntersectionPoint = SmoPoint3.Zero;
                outIntersectionPoint += (float)t * rayVector + rayOrigin;
                return true;
            }
            else // This means that there is a line intersection but not a ray intersection
            {
                outIntersectionPoint = new SmoPoint3();
                return false;
            }



        }
    }
}
