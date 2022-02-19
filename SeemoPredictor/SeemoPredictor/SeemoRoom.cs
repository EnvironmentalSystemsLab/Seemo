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

        List<Brep> Windows;
        public List<Point3d> Pts { get; set; }
        public Vector3d[] Vecs { get; set; }
        public int Resolution { get; set; }
        public double HorizontalSceneAngle { get; set; }
        public double VerticalSceneAngle { get; set; }


        
        //for debugging - switch to private afterwards
        public Mesh zoneMesh;
        //public List<Vector3d> sceneVectors;
        //public List<Point3d> winPts;
        //public List<Vector3d> winRayVectors;

        public ViewResult[,] viewResultsRm { get; set; }


        public SeemoRoom()
        {
        }

        public SeemoRoom(Mesh _room, List<Brep> _windows, List<Point3d> _pts, List<Vector3d> _vecs, int _resolution, double _horizontalSceneAngle, double _verticalSceneAngle)
        {
            Room = _room;
            Windows = _windows;
            Pts = _pts;
            Vecs = _vecs.ToArray();
            Resolution = _resolution;
            HorizontalSceneAngle = _horizontalSceneAngle;
            VerticalSceneAngle = _verticalSceneAngle;

        }



        //Compute room and window analysis
        //SeemoOutdoor rm = new SeemoOutdoor();
        //List<ViewSensor> vs = new List<ViewSensor>();


        public void ComputeRoom()
        {
            //Compute Room and Window Analysis
            Mesh r = Room;
            SeemoRoom vs = new SeemoRoom();
            viewResultsRm = new ViewResult[Pts.Count, Vecs.Length];

            //get room and windows
            ////Windows; Room;
            List<Mesh> windowMeshs = new List<Mesh>();
            Mesh windowMesh = new Mesh();

            //Making Window mesh from window brep and subdivide window mesh depends on widnow area to get window target points

            Mesh w = new Mesh();
            MeshingParameters parameters = new MeshingParameters();


            foreach (Brep wd in Windows)
            {
                var area = AreaMassProperties.Compute(wd).Area;
                int a = (int)Math.Round(area * Resolution);

                parameters.GridMaxCount = a;
                parameters.GridMinCount = a;

                Mesh[] windowBrepToMeshs = Mesh.CreateFromBrep(wd, parameters);

                windowMesh.Append(windowBrepToMeshs[0]);
            }

            //window Breps into Mesh list for window analysis
            MeshingParameters parameters2 = new MeshingParameters();
            foreach (Brep wd in Windows)
            {
                Mesh[] windowBrepToMesh = Mesh.CreateFromBrep(wd, parameters2);
                windowMeshs.Add(windowBrepToMesh[0]);
            }

            Mesh windowAndRoom = new Mesh();
            windowAndRoom.Append(windowMesh);
            windowAndRoom.Append(r);

            int windowThreshold = windowMesh.Faces.Count;


            //compute features for single view point and single view direction
            for (int k = 0; k < Pts.Count; k++)
            {

                Point3d vp = new Point3d(Pts[k]);
                //Space Analyze----------------------------------------------------------------------------------

                for (int j = 0; j < Vecs.Length; j++)
                {

                    Vector3d vd = new Vector3d(Vecs[j]);

                    //Calculated results
                    double was = 0;
                    int wn;
                    
                    
                    viewResultsRm[k, j] = new ViewResult(vp, vd);
                    //
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
                    List<Vector3d> viewAxis = new List<Vector3d> { nvd, vr, vl, vup * 2, vdn * 2, vh };


                    //Making window directional rays------------------------------------------------------------------------------------
                    List<Point3d> winPts = new List<Point3d>(); //not necessary for 1st way, only for 2nd way
                    List<Vector3d> winRayVectors = new List<Vector3d>();


                    //<1st Way to generate rays> pixcel based
                    //making scene rays: make into point 2d cordination plane vdx, _vdx, vdy, _vdy (vectors from (0,0,0))
                    //point[k], viewvector[j], window[i]
                    List<Vector3d> sceneVectors = new List<Vector3d>();
                    //sceneVectors = new List<Vector3d>();


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
                            sceneVectors.Add(sceneVector);

                        }
                    }


                    //select window intersecting rays from sceneMesh
                    int[] winIhit;
                    


                    for (int i = 0; i < sceneVectors.Count; i++)
                    {
                        Vector3d ray = sceneVectors[i];
                        ray.Unitize();
                        Ray3d ray1 = new Ray3d(vp, ray);
                        var dtr = Rhino.Geometry.Intersect.Intersection.MeshRay(windowAndRoom, ray1, out winIhit);
                        if (dtr > 0)
                        {
                            if ((winIhit[0] < windowThreshold))
                            {
                                winRayVectors.Add(sceneVectors[i]);
                            }
                        }
                    }

                    was = winRayVectors.Count / (5288.02083158);

                    //Count how many window are visible
                    //int[] winIhit;
                    List<int> hitWindows = new List<int>();

                    for (int i = 0; i < winRayVectors.Count; i++)
                    {
                        Vector3d ray = winRayVectors[i];
                        ray.Unitize();
                        Ray3d ray1 = new Ray3d(vp, ray);

                        for ( int q = 0; q<windowMeshs.Count; q++)
                        {
                            var dtr = Rhino.Geometry.Intersect.Intersection.MeshRay(windowMeshs[q], ray1);
                            if (dtr > 0)
                            {
                                hitWindows.Add(q);
                            }
                        }
                    }
                    hitWindows = hitWindows.Distinct().ToList();
                    wn = hitWindows.Count;


                    //Zoning Window Area 1.Set up-----------------------------------------------------------------------------------------------
                    Point3d vcp = new Point3d(vp + (nvd * 2));
                    double zLeng = 2 * Math.Tan((Math.PI / 180) * 7.5);

                    Mesh z1 = new Mesh();
                    Mesh z2 = new Mesh();
                    Mesh z3 = new Mesh();
                    Mesh z4 = new Mesh();


                    Point3d[] ptZone = new Point3d[12];

                    ptZone[0] = (vcp + vl * 1.5 * zLeng + vdn * zLeng);
                    ptZone[1] = (vcp + vr * 1.5 * zLeng + vdn * zLeng);
                    ptZone[2] = (vcp + vr * 1.5 * zLeng + vup * zLeng);
                    ptZone[3] = (vcp + vl * 1.5 * zLeng + vup * zLeng);
                    ptZone[4] = (vcp + vl * 4.5 * zLeng + vdn * zLeng);
                    ptZone[5] = (vcp + vr * 4.5 * zLeng + vdn * zLeng);
                    ptZone[6] = (vcp + vr * 4.5 * zLeng + vup * zLeng);
                    ptZone[7] = (vcp + vl * 4.5 * zLeng + vup * zLeng);
                    ptZone[8] = (vcp + vl * 4.5 * zLeng + vdn * 3 * zLeng);
                    ptZone[9] = (vcp + vr * 4.5 * zLeng + vdn * 3 * zLeng);
                    ptZone[10] = (vcp + vr * 4.5 * zLeng + vup * 3 * zLeng);
                    ptZone[11] = (vcp + vl * 4.5 * zLeng + vup * 3 * zLeng);

                    //center zone as z1, z0 used as a patch
                    z1.Vertices.Add(ptZone[0]);
                    z1.Vertices.Add(ptZone[1]);
                    z1.Vertices.Add(ptZone[2]);
                    z1.Vertices.Add(ptZone[3]);
                    z1.Faces.AddFace(0, 1, 2, 3);

                    //middle right and left zone as z2z1.Vertices.Add(ptZone[0]);
                    z2.Vertices.Add(ptZone[4]);
                    z2.Vertices.Add(ptZone[0]);
                    z2.Vertices.Add(ptZone[3]);
                    z2.Vertices.Add(ptZone[7]);
                    z2.Vertices.Add(ptZone[1]);
                    z2.Vertices.Add(ptZone[5]);
                    z2.Vertices.Add(ptZone[6]);
                    z2.Vertices.Add(ptZone[2]);
                    z2.Faces.AddFace(0, 1, 2, 3);
                    z2.Faces.AddFace(4, 5, 6, 7);

                    //upper zone as z3
                    z3.Vertices.Add(ptZone[7]);
                    z3.Vertices.Add(ptZone[6]);
                    z3.Vertices.Add(ptZone[10]);
                    z3.Vertices.Add(ptZone[11]);
                    z3.Faces.AddFace(0, 1, 2, 3);

                    //lower zone as z4
                    z4.Vertices.Add(ptZone[8]);
                    z4.Vertices.Add(ptZone[9]);
                    z4.Vertices.Add(ptZone[5]);
                    z4.Vertices.Add(ptZone[4]);
                    z4.Faces.AddFace(0, 1, 2, 3);

                    //Zoning Window Area 2.Calculate Intersecting point
                    zoneMesh = new Mesh();
                    List<int> zoneFaceCnts = new List<int>();

                    zoneFaceCnts.Add(z1.Faces.Count);
                    zoneFaceCnts.Add(z2.Faces.Count);
                    zoneFaceCnts.Add(z3.Faces.Count);
                    zoneFaceCnts.Add(z4.Faces.Count);

                    zoneMesh.Append(z1);
                    zoneMesh.Append(z2);
                    zoneMesh.Append(z3);
                    zoneMesh.Append(z4);



                    int[] zoneIhit;

                    List<Point3d> z1Pts = new List<Point3d>();
                    List<Point3d> z2Pts = new List<Point3d>();
                    List<Point3d> z3Pts = new List<Point3d>();
                    List<Point3d> z4Pts = new List<Point3d>();
                    List<Point3d> zPts = new List<Point3d>();

                    for (int i = 0; i < winRayVectors.Count; i++)
                    {
                        var dtr = Rhino.Geometry.Intersect.Intersection.MeshRay(zoneMesh, new Ray3d(vp, winRayVectors[i]), out zoneIhit);
                        if (dtr > 0)
                        {
                            Point3d ipt = vp + winRayVectors[i] * dtr;
                            zPts.Add(ipt);
                            if (zoneIhit[0] < zoneFaceCnts[0])
                            {
                                z1Pts.Add(ipt);
                            }
                            else if (zoneIhit[0] < (zoneFaceCnts[0] + zoneFaceCnts[1]))
                            {
                                z2Pts.Add(ipt);
                            }
                            else if (zoneIhit[0] < (zoneFaceCnts[0] + zoneFaceCnts[1] + zoneFaceCnts[2]))
                            {
                                z3Pts.Add(ipt);
                            }
                            else if (zoneIhit[0] < (zoneFaceCnts[0] + zoneFaceCnts[1] + zoneFaceCnts[2] + zoneFaceCnts[3]))
                            {
                                z4Pts.Add(ipt);
                            }
                        }
                    }
                    double zSum = (z1Pts.Count + z2Pts.Count + z3Pts.Count + z4Pts.Count);

                    viewResultsRm[k, j].ResultData = new ResultDataSet(
                        wn,
                        was,
                        (z1Pts.Count / zSum),
                        (z2Pts.Count / zSum),
                        (z3Pts.Count / zSum),
                        (z4Pts.Count / zSum),
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0
                        );

                }
            }
        }
    }
}