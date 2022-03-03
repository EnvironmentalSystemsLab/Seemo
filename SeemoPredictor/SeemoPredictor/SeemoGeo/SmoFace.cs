
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;


namespace SeemoPredictor.SeemoGeo
{

    public class SmoFace

    {
        public int Id { get; set; }
        public SmoPoint3[] VertexList { get; set; }
        public bool IsQuad { get; set; }
        public SmoPoint3 Normal { get; set; }
        public SmoPoint3 Center { get; set; }
        public SmoFaceType ViewContentType { get; set; }




        public SmoBBox BoundingBox { get; set; }





        public SmoFace() { }
        public SmoFace(SmoPoint3[] points)
        {
            if (!(points.Length > 2 && points.Length < 5)) return;
            VertexList = points;
            if (VertexList.Length == 4) IsQuad = true;
            Normal = ComputeNormal();
        }

        internal SmoBBox GetGroundingBox(){
            return new SmoBBox(VertexList);
        }

        internal int ComputeAngleToNorth()
        {
            SmoPoint3 vec = new SmoPoint3(this.Normal.X, this.Normal.Y, 0);
            vec.Normalize();
            vec = SmoPoint3.Rotate(vec, new SmoPoint3(0, 0, 1), (float)-Math.PI);

            double angl = Math.Atan2(vec.X, vec.Y);
            int ori = (int)Math.Round(angl * 180 / Math.PI + 180);
            if (ori > 359) ori = 0;
            return ori;
        }

        internal SmoPoint3 ComputeCenter()
        {
            Center = SmoPoint3.Zero;
            foreach (var p in VertexList) { Center += p; }
            if (IsQuad) Center *= (float)(1.0 / 4.0);
            else Center *= (float)(1.0 / 3.0);
            return Center;
        }

        internal SmoPoint3 ComputeNormal()
        {
            SmoPoint3 v1 = VertexList[1] - VertexList[0];
            SmoPoint3 v2 = VertexList[2] - VertexList[0];
            v1.Normalize();
            v2.Normalize();
            var n = SmoPoint3.Cross(v1, v2);
            n.Normalize();
            return n;
        }

        internal double ComputeArea()
        {
            //Calc area of the triangle
            SmoPoint3 v1 = VertexList[1] - VertexList[0];
            SmoPoint3 v2 = VertexList[2] - VertexList[0];
            double area = 0.5 * (SmoPoint3.Cross(v1, v2).Length);
            //Calc area of a Quad (2 triangles)
            if (this.IsQuad)
            {
                SmoPoint3 v3 = VertexList[3] - VertexList[0];
                area += 0.5 * (SmoPoint3.Cross(v1, v3).Length);
            }
            return area;
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
            Building,
            Equipment,
            Tree,
            Pavement,
            Grass,
            Water,
            Dynamic,
            _UNSET_
        }


    }
}
