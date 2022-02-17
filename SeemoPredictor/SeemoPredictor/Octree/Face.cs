
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
//using RCEnergyLib.Geometry;

namespace SeemoPredictor.Octree
    //different folder. seemo geo
{
    public class Face
        //seemoFace (later)
    {
        public int Id { get; set; }
        public Point3[] VertexList { get; set; }
        public bool IsQuad { get; set; }
        public Point3 Normal { get; set; }
        public Point3 Center { get; set; }
        //public double Area { get; set; }
        //public double AngleToNorth { get; set; }
        //public double AltitudeAngle { get; set; }
        //public Orientation Orientation { get; set; } = Orientation._UNSET_;
        public BBox BoundingBox { get; set; }

        public string Material { get; set; }
        //Enum?


       //public ShadowResult Shadow = null;



        public Face() { }
        public Face(Point3[] points )
        {
            if (!(points.Length > 2 && points.Length < 5)) return;
            VertexList = points;
            if (VertexList.Length == 4) IsQuad = true;
            Normal = ComputeNormal();
            //Area = ComputeArea();
            //maybe later useful

            //SetOrientation();
            //AngleToNorth = ComputeAngleToNorth();
        }

        internal BBox GetBoundingBox() {
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

        //internal void SetOrientation()
        //{
        //    // compute altitude angle

        //    double dot = Point3.Dot(Normal, Point3.ZAxis);
        //    double ang = Math.Acos(dot) * 180.0 / Math.PI;

        //    AltitudeAngle = ang;

        //    // set orientation

        //    Point3 vec = this.Normal;
        //    vec.Normalize();

        //    Point3 north = Point3.YAxis;
        //    double dotN = Point3.Dot(vec, north);
        //    double angleN = Math.Acos(dotN) * 180.0 / Math.PI;

        //    Point3 south = -Point3.YAxis;
        //    double dotS = Point3.Dot(vec, south);
        //    double angleS = Math.Acos(dotS) * 180.0 / Math.PI;

        //    Point3 east = Point3.XAxis;
        //    double dotE = Point3.Dot(vec, east);
        //    double angleE = Math.Acos(dotE) * 180.0 / Math.PI;

        //    Point3 west = -Point3.XAxis;
        //    double dotW = Point3.Dot(vec, west);
        //    double angleW = Math.Acos(dotW) * 180.0 / Math.PI;

        //    if (angleN <= 45) { Orientation = Orientation.North; return; }
        //    else if (angleS <= 45) { Orientation = Orientation.South; return; }
        //    else if (angleE <= 45) { Orientation = Orientation.East; return; }
        //    else if (angleW <= 45) { Orientation = Orientation.West; return; }

        //    Point3 up = Point3.ZAxis;
        //    double dotUp = Point3.Dot(vec, up);
        //    double angleUp = Math.Acos(dotUp) * 180.0 / Math.PI;
        //    if (angleUp <= 45) { Orientation = Orientation.Up; return; }
        //    else { Orientation = Orientation.Down; return; }


        //}

        internal Point3 ComputeCenter() {
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
  

}
