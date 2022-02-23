using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace SeemoPredictor
{
    public class SeemoRoom
    {

        public Mesh Room { get; set; }

        public List<Point3d> Pts { get; set; } 
        public Vector3d[] Vecs { get; set; }
        public int Resolution { get; set; }
        public double HorizontalSceneAngle { get; set; }
        public double VerticalSceneAngle { get; set; }
        public int roomThreshold { get; set; }



        //for debugging - switch to private afterwards
        //public Mesh zoneMesh;
        //public List<Vector3d> sceneVectors;
        //public List<Point3d> winPts;
        //public List<Vector3d> winRayVectors;

        //public ViewResult[,] viewResultsRm { get; set; }


        public SeemoRoom()
        {
        }

        public SeemoRoom(Mesh _room, List<Point3d> _pts, List<Vector3d> _vecs, int _resolution, double _horizontalSceneAngle, double _verticalSceneAngle)
        {
            Room = _room;
            Pts = _pts;
            Vecs = _vecs.ToArray();
            Resolution = _resolution;
            HorizontalSceneAngle = _horizontalSceneAngle;
            VerticalSceneAngle = _verticalSceneAngle;
            roomThreshold = _room.Faces.Count;

        }



        public ResultDataSet GenerateZoneRay(int pointIndex, int vectorIndex)
        {
            //compute features for single view point and single view direction
            Point3d vp = Pts[pointIndex];
            Vector3d vd = Vecs[vectorIndex];

            //Calculated results
            double was = 0;
            int wn;
                    
                    
            //Define Left, right, up, down vectors to measure room dimension
            Vector3d nvd = vd;
            nvd.Unitize();

            //vr, vl is outer product of vd and zaxis
            double vrx = nvd.Y;
            double vry = -nvd.X;
            Vector3d vr = new Vector3d(vrx, vry, 0);
            vr.Unitize();
            Vector3d vl = new Vector3d(-vr);
            Vector3d vup = new Vector3d(0, 0, 1);
            Vector3d vdn = new Vector3d(0, 0, -1);
            Vector3d vf = nvd;
            Vector3d vh = Vector3d.CrossProduct(vr, nvd);

            //visualize view vectors
            
            //Making window directional rays------------------------------------------------------------------------------------
            List<Vector3d> RayVectors1 = new List<Vector3d>();
            List<Vector3d> RayVectors2 = new List<Vector3d>();
            List<Vector3d> RayVectors3 = new List<Vector3d>();
            List<Vector3d> RayVectors4 = new List<Vector3d>();


            //<1st Way to generate rays> pixcel based
            //making scene rays: make into point 2d cordination plane vdx, _vdx, vdy, _vdy (vectors from (0,0,0))
            //point[k], viewvector[j], window[i]


            Vector3d vdx = vd;
            vdx.Rotate((HorizontalSceneAngle * Math.PI / 180) / 2, -vh);
            Vector3d _vdx = vd;
            _vdx.Rotate(-(HorizontalSceneAngle * Math.PI / 180) / 2, -vh);
            Vector3d vdy = vd;
            vdy.Rotate((VerticalSceneAngle * Math.PI / 180) / 2, vr);
            Vector3d _vdy = vd;
            _vdy.Rotate(-(VerticalSceneAngle * Math.PI / 180) / 2, vr);

            double mMax = (Resolution * HorizontalSceneAngle * Math.PI / 180);
            double nMax = (Resolution * VerticalSceneAngle * Math.PI / 180);
            for (int m = 0; m < mMax; m++)
            {
                for (int n = 0; n < nMax; n++)
                {
                    Vector3d sceneVector = (m / Math.Floor(mMax)) * _vdx + (1 - (m / Math.Floor(mMax))) * vdx
                        + (n / Math.Floor(nMax)) * _vdy + (1 - (n / Math.Floor(nMax))) * vdy - vd * ((vd.X * vdx.X + vd.Y * vdx.Y + vd.Z * vdx.Z) );
                    
                    if(n < (nMax / 3))
                    {
                        RayVectors3.Add(sceneVector);
                    }
                    else if((n >= (nMax /3)) && (n < (nMax * 2 / 3)))
                    {
                        if((m >= (mMax / 3)) && (m < (mMax * 2 / 3)))
                        {
                            RayVectors1.Add(sceneVector);
                        }
                        else
                        {
                            RayVectors2.Add(sceneVector);
                        }
                    }
                    else
                    {
                        RayVectors4.Add(sceneVector);
                    }
                }
            }


            ResultDataSet resultData1 = new ResultDataSet();

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