using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeemoPredictor.Geometry
{
    public class SmoIntersect
    {

        private static double EPSILON = 0.0000001;
 
        public static SmoFace IsVisible(PointOctree<SmoFace> octree, Point3 pt, Point3 raydir, double maxNodeSize, out Point3 hitPt)
        {
            var ray = new Ray(pt, raydir);
            var testGeo = octree.GetNearby(ray, (float)maxNodeSize);

            SmoFace returnFace = null;
            hitPt = Point3.Zero;
            double closestdist = double.MaxValue;
            

            foreach (var g in testGeo)
            {
                Point3 ipt1;
                Point3 ipt2;
                bool i1 = false;
                bool i2 = false;

                SmoFace.SmoFaceType type;
                i1 = SmoIntersect.RayTriangle_MollerTrumbore(ray, g.VertexList[0], g.VertexList[1], g.VertexList[2], out ipt1);

                if (i1)
                {
                   
                    var dist = (ipt1 - pt).Length;
                    if (dist < closestdist) {
                        hitPt = ipt1;
                        closestdist = dist; 
                        returnFace = g; 
                    }
                    
                }
                else if (g.IsQuad)
                {
                    i2 = SmoIntersect.RayTriangle_MollerTrumbore(ray, g.VertexList[0], g.VertexList[2], g.VertexList[3], out ipt2);
                    if (i2)
                    {
                        var dist = (ipt2 - pt).Length;
                        if (dist < closestdist)
                        {
                            hitPt = ipt2;
                            closestdist = dist;
                            returnFace = g;
                        }
                    }
                }
            }
            return returnFace;
        }
       
        public static bool RayTriangle_MollerTrumbore(Ray ray, Point3 vertex0, Point3 vertex1, Point3 vertex2, out Point3 outIntersectionPoint)
        {
            Point3 rayOrigin = ray.Origin;
            Point3 rayVector = ray.Direction;

            Point3 q = new Point3();
            double f, u, v;
            Point3 edge1 = (vertex1 - vertex0);
            Point3 edge2 = (vertex2 - vertex0);
            Point3 h = Point3.Cross(rayVector, edge2);

            double a = Point3.Dot(edge1, h); // dot product

            if (a > -EPSILON && a < EPSILON)
            {
                outIntersectionPoint = new Point3();
                return false;  //This ray in parallel to this triangle.
            }
            f = 1.0 / a;
            Point3 s = rayOrigin - vertex0;
            u = f * Point3.Dot(s, h);
            if (u < 0.0 || u > 1.0)
            {
                outIntersectionPoint = new Point3();
                return false;
            }
            q = Point3.Cross(s, edge1);
            v = f * Point3.Dot(rayVector, q);
            if (v < 0.0 || u + v > 1.0)
            {
                outIntersectionPoint = new Point3();
                return false;
            }
            q = Point3.Cross(s, edge1);
            v = f * Point3.Dot(rayVector, q);
            if (v < 0.0 || u + v > 1.0)
            {
                outIntersectionPoint = new Point3();
                return false;
            }
            // At this stage we can compute t to find out where the intersection point is on the line.
            double t = f * Point3.Dot(edge2, q);
            if (t > EPSILON) // ray intersection
            {
                outIntersectionPoint = Point3.Zero;
                outIntersectionPoint += (float)t * rayVector + rayOrigin;
                return true;
            }
            else // This means that there is a line intersection but not a ray intersection
            {
                outIntersectionPoint = new Point3();
                return false;
            }



        }
    }
}
