using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using SeemoPredictor.Geometry;

namespace SeemoPredictor
{
    public class SmoSensor
    {

        public  Point3 Pt { get; set; }
        public Point3[] ViewDirections { get; set; }

        public Point3[] QuadMeshVertices { get; set; } = new Point3[3];

        public int Resolution { get; set; } = 1024;
        public double HorizontalViewAngle { get; set; } = (35.754 * 2);
        public double VerticalViewAngle { get; set; } = (25.641 * 2);



        public SmoSensor()
        {
        }

        public SmoSensor(Point3 _pt, List<Point3> _vecs, int _resolution, double _horizontalSceneAngle, double _verticalSceneAngle)
        {
            Pt = _pt;
            ViewDirections = _vecs.ToArray();
            Resolution = _resolution;
            HorizontalViewAngle = _horizontalSceneAngle;
            VerticalViewAngle = _verticalSceneAngle;
        }

        public SmoSensor(Point3 _pt, List<Point3> _vecs, Point3 Vertex0, Point3 Vertex1, Point3 Vertex2, Point3 Vertex3, int _resolution, double _horizontalSceneAngle, double _verticalSceneAngle)
        {
            Pt = _pt;
            ViewDirections = _vecs.ToArray();
            QuadMeshVertices.Append(Vertex0);
            QuadMeshVertices.Append(Vertex1);
            QuadMeshVertices.Append(Vertex2);
            QuadMeshVertices.Append(Vertex3);
            Resolution = _resolution;
            HorizontalViewAngle = _horizontalSceneAngle;
            VerticalViewAngle = _verticalSceneAngle;
        }


        public SmoImage GenerateImagePlane(Point3 viewDirection)
        {
            //compute features for single view point and single view direction
  
            SmoImage image = new SmoImage(Pt, viewDirection, Resolution, HorizontalViewAngle, VerticalViewAngle);

            return image;
        }


        
    }
}