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


        public DirectionResult GenerateZoneRay(int vectorIndex)
        {
            //compute features for single view point and single view direction
            SmoPoint3 vp = Pt;
            SmoPoint3 vd = ViewDirections[vectorIndex];


            //Define Left, right, up, down vectors to measure room dimension
            SmoPoint3 nvd = vd;
            nvd.Normalize();

            //vr, vl is outer product of vd and zaxis
            double vrx = nvd.Y;
            double vry = -nvd.X;
            SmoPoint3 vr = new SmoPoint3(vrx, vry, 0);
            vr.Normalize();
            SmoPoint3 vl = new SmoPoint3(-vr.X, -vr.Y, -vr.Z);
            SmoPoint3 vup = new SmoPoint3(0, 0, 1);
            SmoPoint3 vdn = new SmoPoint3(0, 0, -1);
            SmoPoint3 vf = nvd;
            SmoPoint3 vh = SmoPoint3.Cross(vr, nvd);

            //visualize view vectors

            //Making window directional rays------------------------------------------------------------------------------------
            List<SmoPoint3> RayVectors1 = new List<SmoPoint3>();
            List<SmoPoint3> RayVectors2 = new List<SmoPoint3>();
            List<SmoPoint3> RayVectors3 = new List<SmoPoint3>();
            List<SmoPoint3> RayVectors4 = new List<SmoPoint3>();


            //<1st Way to generate rays> pixel based
            //making scene rays: make into point 2d cordination plane vdx, _vdx, vdy, _vdy (vectors from (0,0,0))
            //point[k], viewvector[j], window[i]


            SmoPoint3 vdx = vd;
            vdx = SmoPoint3.Rotate(vdx, -vh, (float)((HorizontalViewAngle * Math.PI / 180) / 2));
            SmoPoint3 _vdx = vd;
            _vdx = SmoPoint3.Rotate(_vdx, -vh, -(float)((HorizontalViewAngle * Math.PI / 180) / 2));
            SmoPoint3 vdy = vd;
            vdy = SmoPoint3.Rotate(vdy, vr, (float)((VerticalViewAngle * Math.PI / 180) / 2));
            SmoPoint3 _vdy = vd;
            _vdy = SmoPoint3.Rotate(_vdy, vr, -(float)((VerticalViewAngle * Math.PI / 180) / 2));

            double mMax = (Resolution * HorizontalViewAngle * Math.PI / 180);
            double nMax = (Resolution * VerticalViewAngle * Math.PI / 180);
            for (int m = 0; m < mMax; m++)
            {
                for (int n = 0; n < nMax; n++)
                {
                    SmoPoint3 sceneVector = (float)(m / Math.Floor(mMax)) * _vdx + (float)(1 - (m / Math.Floor(mMax))) * vdx
                        + (float)(n / Math.Floor(nMax)) * _vdy + (float)(1 - (n / Math.Floor(nMax))) * vdy - vd * ((vd.X * vdx.X + vd.Y * vdx.Y + vd.Z * vdx.Z));

                    if (n > (nMax - (Math.Truncate((nMax + 1) / 3))))
                    {
                        RayVectors3.Add(sceneVector);
                    }
                    else if (n >= Math.Truncate((nMax + 1) / 3))
                    {
                        if ((m < Math.Truncate((mMax + 1) / 3)) || (m > (mMax - (Math.Truncate((mMax + 1) / 3)))))
                        {
                            RayVectors2.Add(sceneVector);
                        }
                        else
                        {
                            RayVectors1.Add(sceneVector);
                        }
                    }
                    else
                    {
                        RayVectors4.Add(sceneVector);
                    }
                }
            }


            DirectionResult resultData1 = new DirectionResult();

            resultData1.sceneRayVectorsZ1 = RayVectors1;
            resultData1.sceneRayVectorsZ2 = RayVectors2;
            resultData1.sceneRayVectorsZ3 = RayVectors3;
            resultData1.sceneRayVectorsZ4 = RayVectors4;

            resultData1.Dir = vd;
            resultData1.ViewPointX = vp.X;
            resultData1.ViewPointY = vp.Y;
            resultData1.ViewPointZ = vp.Z;
            resultData1.ViewVectorX = vd.X;
            resultData1.ViewVectorY = vd.Y;
            resultData1.ViewVectorZ = vd.Z;

            return resultData1;
        }
    }
}