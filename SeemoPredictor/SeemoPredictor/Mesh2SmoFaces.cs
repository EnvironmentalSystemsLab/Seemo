using Rhino.Geometry;
using SeemoPredictor.Geometry;
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

                Point3[] pts;

                var a = m.Vertices[m.Faces[i].A];
                var b = m.Vertices[m.Faces[i].B];
                var c = m.Vertices[m.Faces[i].C];
                Point3 p0 = new Point3(a.X, a.Y, a.Z);
                Point3 p1 = new Point3(b.X, b.Y, b.Z);
                Point3 p2 = new Point3(c.X, c.Y, c.Z);

                if (m.Faces[i].IsQuad)  //if it's quad divide into two triangles and save
                {
                    
                    pts = new Point3[3];
                    var d = m.Vertices[m.Faces[i].D];
                    Point3 p3 = new Point3(d.X, d.Y, d.Z);
                    pts[0] = p0;
                    pts[1] = p1;
                    pts[2] = p2;

                    Point3[] pts2 = new Point3[3];
                    pts2[0] = p0;
                    pts2[1] = p2;
                    pts2[2] = p3;

                    SmoFace smoFace2 = new SmoFace(pts2, type);
                    smoFaces.Add(smoFace2);

                }
                else
                {
                    pts = new Point3[3];
                    pts[0] = p0;
                    pts[1] = p1;
                    pts[2] = p2;
                }

                SmoFace smoFace = new SmoFace(pts, type);
                smoFaces.Add(smoFace);
            }
            return smoFaces;
        }

        


    }
}
