
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace SeemoPredictor.Geometry
{

    public class SmoFace

    {
        public int Id { get; set; }
        public Point3[] VertexList { get; set; }
        public bool IsQuad { get; set; }
        public Point3 Normal { get; set; }
        public Point3 Center { get; set; }
        public SmoFaceType ViewContentType { get; set; }



        public BBox BoundingBox { get; set; }



        public SmoFace() { }

        //public SmoFace(Point3[] points)
        //{
        //    if (!(points.Length > 2 && points.Length < 5)) return;
        //    VertexList = points;
        //    if (VertexList.Length == 4) IsQuad = true;
        //    Normal = ComputeNormal();
        //    BoundingBox = GetBoundingBox();
        //}

        public SmoFace(Point3[] points, SmoFaceType type)
        {
            if (!(points.Length > 2 && points.Length < 5)) return;
            this.VertexList = points;
            if (VertexList.Length == 4) IsQuad = true;
            this.Normal = ComputeNormal();
            this.BoundingBox = GetBoundingBox();
            this.ViewContentType = type;
            this.ComputeCenter();
            //smoFace.ComputeNormal();
            //smoFace.ComputeArea();
            //smoFace.ComputeAngleToNorth();
        }

        internal BBox GetBoundingBox(){
            return new BBox(VertexList);
        }

        internal int ComputeAngleToNorth()
        {
            Point3 vec = new Point3(this.Normal.X, this.Normal.Y, 0);
            vec.Normalize();
            vec = Point3.Rotate(vec, new Point3(0, 0, 1), (float)-Math.PI);

            double angl = Math.Atan2(vec.X, vec.Y);
            int ori = (int)Math.Round(angl * 180 / Math.PI + 180);
            if (ori > 359) ori = 0;
            return ori;
        }

        internal Point3 ComputeCenter()
        {
            Center = Point3.Zero;
            foreach (var p in VertexList) { Center += p; }
            if (IsQuad) Center *= (float)(1.0 / 4.0);
            else Center *= (float)(1.0 / 3.0);
            return Center;
        }

        internal Point3 ComputeNormal()
        {
            Point3 v1 = VertexList[1] - VertexList[0];
            Point3 v2 = VertexList[2] - VertexList[0];
            v1.Normalize();
            v2.Normalize();
            var n = Point3.Cross(v1, v2);
            n.Normalize();
            return n;
        }

        internal double ComputeArea()
        {
            //Calc area of the triangle
            Point3 v1 = VertexList[1] - VertexList[0];
            Point3 v2 = VertexList[2] - VertexList[0];
            double area = 0.5 * (Point3.Cross(v1, v2).Length);
            //Calc area of a Quad (2 triangles)
            if (this.IsQuad)
            {
                Point3 v3 = VertexList[3] - VertexList[0];
                area += 0.5 * (Point3.Cross(v1, v3).Length);
            }
            return area;
        }

        public static void ConvertToPrimitive(List<SmoFace> smofaces, out List<float> vertices, out List<int> triangles, out List<int> mats)
        {
            vertices = new List<float>();
            triangles = new List<int>();
            mats = new List<int>();
            int vertexIndex = 0;

            for (int i = 0; i < smofaces.Count; i++)  //smo faces are all triangles
            {
                vertices.Add((float)smofaces[i].VertexList[0].X);
                vertices.Add((float)smofaces[i].VertexList[0].Y);
                vertices.Add((float)smofaces[i].VertexList[0].Z);
                triangles.Add(vertexIndex);
                vertexIndex++;


                vertices.Add((float)smofaces[i].VertexList[1].X);
                vertices.Add((float)smofaces[i].VertexList[1].Y);
                vertices.Add((float)smofaces[i].VertexList[1].Z);
                triangles.Add(vertexIndex);
                vertexIndex++;

                vertices.Add((float)smofaces[i].VertexList[2].X);
                vertices.Add((float)smofaces[i].VertexList[2].Y);
                vertices.Add((float)smofaces[i].VertexList[2].Z);
                triangles.Add(vertexIndex);
                vertexIndex++;

                mats.Add((int)smofaces[i].ViewContentType);
                //face defined, stores
            }
        }


        [JsonConverter(typeof(StringEnumConverter))]
        public enum Orientation
        {
            _UNSET_,
            North, 
            South,
            East,
            West,
            Up,
            Down
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum SmoFaceType
        {
            Interior,
            Exterior,
            Glazing,
            Context_Building,
            Context_Window,
            Equipment,
            Landmark,
            Sidewalk,
            Road,
            ParkingLot,
            Tree,
            Grass,
            Water,
            _UNSET_
        }


    }
}
