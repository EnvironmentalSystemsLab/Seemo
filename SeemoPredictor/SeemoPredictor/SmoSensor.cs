using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using SeemoPredictor.SeemoGeo;

namespace SeemoPredictor
{
    public class SmoSensor
    {

        public  SmoPoint3 Pt { get; set; }
        public SmoPoint3[] ViewDirections { get; set; }

        public int Resolution { get; set; } = 1024;
        public double HorizontalViewAngle { get; set; } = (35.754 * 2);
        public double VerticalViewAngle { get; set; } = (25.641 * 2);



        public SmoSensor()
        {
        }

        public SmoSensor(SmoPoint3  _pt, List<SmoPoint3> _vecs, int _resolution, double _horizontalSceneAngle, double _verticalSceneAngle)
        {
            Pt = _pt;
            ViewDirections = _vecs.ToArray();
            Resolution = _resolution;
            HorizontalViewAngle = _horizontalSceneAngle;
            VerticalViewAngle = _verticalSceneAngle;
        }



        public SmoImage GenerateImagePlane(SmoPoint3 viewDirection)
        {
            //compute features for single view point and single view direction
            SmoPoint3 vp = Pt;
            SmoPoint3 vd = viewDirection;

            SmoImage image = new SmoImage(Pt, viewDirection, Resolution, HorizontalViewAngle, VerticalViewAngle);

            return image;
        }

    }
}