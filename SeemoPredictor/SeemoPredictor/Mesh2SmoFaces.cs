using Rhino.Geometry;
using SeemoPredictor.SeemoGeo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeemoPredictor
{
    public class Mesh2SmoFaces
    {
        public static List<SmoFace> MeshToSmoFaces(Mesh m, SmoFace.SmoFaceType type)
        {
            List<SmoFace> smoFaces = new List<SmoFace>();

            for (int i = 0; i < m.Faces.Count; i++)
            {

                SmoPoint3[] pts;

                var a = m.Vertices[m.Faces[i].A];
                var b = m.Vertices[m.Faces[i].B];
                var c = m.Vertices[m.Faces[i].C];
                SmoPoint3 p0 = new SmoPoint3(a.X, a.Y, a.Z);
                SmoPoint3 p1 = new SmoPoint3(b.X, b.Y, b.Z);
                SmoPoint3 p2 = new SmoPoint3(c.X, c.Y, c.Z);

                if (m.Faces[i].IsQuad)
                {
                    pts = new SmoPoint3[4];
                    var d = m.Vertices[m.Faces[i].D];
                    SmoPoint3 p3 = new SmoPoint3(d.X, d.Y, d.Z);
                    pts[0] = p0;
                    pts[1] = p1;
                    pts[2] = p2;
                    pts[3] = p3;
                }
                else
                {
                    pts = new SmoPoint3[3];
                    pts[0] = p0;
                    pts[1] = p1;
                    pts[2] = p2;
                }

                SmoFace smoFace = new SmoFace(pts);

                smoFace.Material = type;
                smoFaces.Add(smoFace);
            }
            return smoFaces;
        }


    }
}
