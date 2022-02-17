namespace SeemoPredictor.Octree
{
    public class Intersect
    {
        //public static double RayBox(BBox b, Ray r)
        //{
        //    Point3 rpos = r.Origin;

        //    Point3 rdir = r.Direction;
        //    Point3 vmin = b.Min;
        //    Point3 vmax = b.Max;

        //    double t1 = (vmin.X - rpos.X) / rdir.X;
        //    double t2 = (vmax.X - rpos.X) / rdir.X;
        //    double t3 = (vmin.Y - rpos.Y) / rdir.Y;
        //    double t4 = (vmax.Y - rpos.Y) / rdir.Y;
        //    double t5 = (vmin.Z - rpos.Z) / rdir.Z;
        //    double t6 = (vmax.Z - rpos.Z) / rdir.Z;

        //    double aMin = t1 < t2 ? t1 : t2;
        //    double bMin = t3 < t4 ? t3 : t4;
        //    double cMin = t5 < t6 ? t5 : t6;

        //    double aMax = t1 > t2 ? t1 : t2;
        //    double bMax = t3 > t4 ? t3 : t4;
        //    double cMax = t5 > t6 ? t5 : t6;

        //    double fMax = aMin > bMin ? aMin : bMin;
        //    double fMin = aMax < bMax ? aMax : bMax;

        //    double t7 = fMax > cMin ? fMax : cMin;
        //    double t8 = fMin < cMax ? fMin : cMax;

        //    double t9 = (t8 < 0 || t7 > t8) ? -1 : t7;

        //    return t9;
        //}

        private static double EPSILON = 0.0000001;

        public static bool RayTriangle_MollerTrumbore(Ray ray,
                                                 Point3 vertex0,
                                                 Point3 vertex1,
                                                 Point3 vertex2,
                                                 out Point3 outIntersectionPoint)
        {

            Point3 rayOrigin = ray.Origin;
            Point3 rayVector = ray.Direction;


            Point3 q = new Point3();
            double f, u, v;
            Point3 edge1 = (vertex1 - vertex0);
            Point3 edge2 = (vertex2 - vertex0);
            Point3 h = Point3.Cross(rayVector, edge2);

            double a = Point3.Dot(edge1 , h); //dot prod

            if (a > -EPSILON && a < EPSILON)
            {
                outIntersectionPoint = new Point3();
                return false;    // This ray is parallel to this triangle.
            }
            f = 1.0 / a;
            Point3 s = rayOrigin - vertex0;
            u = f * Point3.Dot(s , h);
            if (u < 0.0 || u > 1.0)
            {
                outIntersectionPoint = new Point3();
                return false;
            }
            q = Point3.Cross(s, edge1);
            v = f * Point3.Dot(rayVector , q);
            if (v < 0.0 || u + v > 1.0)
            {
                outIntersectionPoint = new Point3();
                return false;
            }
            // At this stage we can compute t to find out where the intersection point is on the line.
            double t = f * Point3.Dot(edge2 , q);
            if (t > EPSILON) // ray intersection
            {
                outIntersectionPoint = Point3.Zero;
                outIntersectionPoint +=  (float)t * rayVector + rayOrigin;
                return true;
            }
            else // This means that there is a line intersection but not a ray intersection.
            {
                outIntersectionPoint = new Point3();
                return false;
            }
        }
    }
}
