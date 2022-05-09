using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeemoPredictor.Geometry;

namespace SeemoPredictor.GPU
{
    public readonly struct HitRecord
    {
        public readonly float t;
        public readonly bool inside;
        public readonly Point3 p;
        public readonly Point3 normal;
        public readonly int materialID;

        public HitRecord(float t, Point3 p, Point3 normal, bool inside, int materialID)
        {
            this.t = t;
            this.inside = inside;
            this.p = p;
            this.normal = normal;
            this.materialID = materialID;
        }

        public HitRecord(float t, Point3 p, Point3 normal, Point3 rayDirection, int materialID)
        {
            this.t = t;
            inside = Point3.Dot(normal, rayDirection) > 0;
            this.p = p;
            this.normal = normal;
            this.materialID = materialID;
        }

    }
}
