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
