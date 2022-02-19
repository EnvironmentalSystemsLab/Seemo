using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;


namespace SeemoPredictor
{
    public class SeemoOutdoor
    {


        public SeemoOutdoor() { }

        public SeemoRoom roomsensor { get; set; }
        public SEnvironment env { get; set; }



        //not sure..
        public SeemoOutdoor(SeemoRoom _roomsensor, SEnvironment _env)
        {
            roomsensor = _roomsensor;
            env = _env;
        }

        //viewResultsRm[k,j].WindowRayVectors = winRayVectors;
        //viewResultsRm[k, j].WindowRays = winRays;


        public void ComputeView()
        {
            for (int k = 0; k < roomsensor.Pts.Count; k++)
            {
                Point3d vp = roomsensor.Pts[k];
                for (int j = 0; j < roomsensor.Vecs.Length; j++)
                {
                    Vector3d vd = roomsensor.Vecs[j];
                    //List<Ray3d> winRays = roomsensor.viewResultsRm[k, j].WindowRays;
                    List<Vector3d> winRayVectors = new List<Vector3d>();
                    Mesh sceneMesh = env.SceneMesh;
                    List<int> faceCnts = env.FaceCnt;
                    List<Mesh> pavements = env.Pavements;
                    List<Mesh> grass = env.Grass;

                    //calculate distance from veiw point to projected viewpoint on pavement and grass and get shortest distance
                    double floorHeights = 100000;
                    double distToPavement = 50000;
                    double distToGrass = 50000;
                    //-----------------------------------------------------------

                    if (pavements != null)
                    {
                        for (int i = 0; i < pavements.Count; i++)
                        {
                            Plane pavementPlane = new Rhino.Geometry.Plane(pavements[i].Vertices[0], pavements[i].Vertices[1], pavements[i].Vertices[2]);
                            if (Math.Abs(pavementPlane.DistanceTo(vp)) < distToPavement)
                            {
                                distToPavement = Math.Abs(pavementPlane.DistanceTo(vp));
                            }
                        }
                    }

                    if (grass != null)
                    {
                        for (int i = 0; i < grass.Count; i++)
                        {
                            Plane grassPlane = new Rhino.Geometry.Plane(grass[0].Vertices[0], grass[0].Vertices[1], grass[0].Vertices[2]);
                            if (Math.Abs(grassPlane.DistanceTo(vp)) < distToGrass)
                            {
                                distToGrass = Math.Abs(grassPlane.DistanceTo(vp));
                            }
                        }
                    }

                    if ((pavements != null) && (grass != null))
                    {
                        floorHeights = Math.Min(distToGrass, distToPavement);
                    }
                    else if ((pavements != null) && (grass == null))
                    {
                        floorHeights = distToPavement;
                    }
                    else if ((pavements == null) && (grass != null))
                    {
                        floorHeights = distToGrass;
                    }
                    else
                    {
                        floorHeights = Plane.WorldXY.DistanceTo(vp);
                    }

                    //Environment Intersecting Points Lists
                    List<Point3d> buildingPts = new List<Point3d>();
                    List<Point3d> equipmentPts = new List<Point3d>();
                    List<Point3d> treePts = new List<Point3d>();
                    List<Point3d> pavementPts = new List<Point3d>();
                    List<Point3d> grassPts = new List<Point3d>();
                    List<Point3d> waterPts = new List<Point3d>();
                    List<Point3d> dynamicPts = new List<Point3d>();
                    List<Point3d> skyPts = new List<Point3d>();

                    List<double> buildingDists = new List<double>();
                    List<double> equipmentDists = new List<double>();
                    List<double> treeDists = new List<double>();
                    List<double> pavementDists = new List<double>();
                    List<double> grassDists = new List<double>();
                    List<double> waterDists = new List<double>();
                    List<double> dynamicDists = new List<double>();
                    List<double> skyDists = new List<double>();


                    int[] faceIhit;
                    for (int i = 0; i < winRayVectors.Count; i++)
                    {
                        var dtr = Rhino.Geometry.Intersect.Intersection.MeshRay(sceneMesh, new Ray3d(vp, winRayVectors[i]), out faceIhit);
                        winRayVectors[i].Unitize();
                        Point3d ipt = vp + winRayVectors[i] * dtr;

                        if (dtr < 0.0)
                        {
                            return;
                        }
                        else if (faceIhit[0] < faceCnts[0])
                        {
                            buildingPts.Add(ipt);
                            buildingDists.Add(dtr);
                        }
                        else if (faceIhit[0] < (faceCnts[0] + faceCnts[1]))
                        {
                            equipmentPts.Add(ipt);
                            equipmentDists.Add(dtr);
                        }
                        else if (faceIhit[0] < (faceCnts[0] + faceCnts[1] + faceCnts[2]))
                        {
                            treePts.Add(ipt);
                            treeDists.Add(dtr);
                        }
                        else if (faceIhit[0] < (faceCnts[0] + faceCnts[1] + faceCnts[2] + faceCnts[3]))
                        {
                            pavementPts.Add(ipt);
                            pavementDists.Add(dtr);
                        }
                        else if (faceIhit[0] < (faceCnts[0] + faceCnts[1] + faceCnts[2] + faceCnts[3] + faceCnts[4]))
                        {
                            grassPts.Add(ipt);
                            grassDists.Add(dtr);
                        }
                        else if (faceIhit[0] < (faceCnts[0] + faceCnts[1] + faceCnts[2] + faceCnts[3] + faceCnts[4] + faceCnts[5]))
                        {
                            waterPts.Add(ipt);
                            waterDists.Add(dtr);
                        }
                        else if (faceIhit[0] < (faceCnts[0] + faceCnts[1] + faceCnts[2] + faceCnts[3] + faceCnts[4] + faceCnts[5] + faceCnts[6]))
                        {
                            dynamicPts.Add(ipt);
                            dynamicDists.Add(dtr);
                        }
                        else if (faceIhit[0] < (faceCnts[0] + faceCnts[1] + faceCnts[2] + faceCnts[3] + faceCnts[4] + faceCnts[5] + faceCnts[6] + faceCnts[7]))
                        {
                            skyPts.Add(ipt);
                            skyDists.Add(dtr);
                        }
                    }

                    List<int> elementPts = new List<int> { buildingPts.Count, equipmentPts.Count, treePts.Count, pavementPts.Count, grassPts.Count, waterPts.Count, dynamicPts.Count, skyPts.Count };
                    double total = (elementPts[0] + elementPts[1] + elementPts[2] + elementPts[3] + elementPts[4] + elementPts[5] + elementPts[6] + elementPts[7]);

                    int elementNumber = 0;
                    for (int i = 0; i < elementPts.Count; i++)
                    {
                        if (elementPts[i] > 10)
                        {
                            elementNumber++;
                        }
                    }

                    //calculate object's closest distance and type
                    double ComputeClosestDist(List<double> dists)
                    {
                        double closeDistance = 0;
                        if (dists.Count == 0)
                        {
                            return 50000; //50000에서 바꿈 //double.NaN
                        }
                        else
                        {
                            dists.Sort();
                            for (int i = 0; i < dists.Count * 0.3 + 0.001; i++)
                            {
                                closeDistance = closeDistance + dists[i];
                            }
                            closeDistance = closeDistance / (Math.Ceiling(dists.Count * 0.3));
                            return closeDistance;
                        }
                    }

                    double buildingClosestDist = ComputeClosestDist(buildingDists);
                    double equipmentClosestDist = ComputeClosestDist(equipmentDists);
                    double treeClosestDist = ComputeClosestDist(treeDists);
                    double pavementClosestDist = ComputeClosestDist(pavementDists);
                    double grassClosestDist = ComputeClosestDist(grassDists);
                    double waterClosestDist = ComputeClosestDist(waterDists);
                    double dynamicClosestDist = ComputeClosestDist(dynamicDists);
                    double skyClosestDist = ComputeClosestDist(skyDists);

                    string closestObjectType = null;
                    double closestObjectDist;

                    //exclude pavement and grass
                    Dictionary<string, double> objectDists = new Dictionary<string, double>()
                    {
                        { "buildingClosestDist", buildingClosestDist }, { "equipmentClosestDist", equipmentClosestDist }, { "treeClosestDist", treeClosestDist }, { "dynamicClosestDist", dynamicClosestDist }, { "skyClosestDist", skyClosestDist }
                    };

                    closestObjectType = objectDists.OrderBy(m => m.Value).FirstOrDefault().Key;
                    closestObjectDist = objectDists.OrderBy(m => m.Value).FirstOrDefault().Value;

                    if (closestObjectDist == 50000)
                    {
                        closestObjectDist = 0;
                    }

                    int buildingIsClosest = 0;
                    int equipmentIsClosest = 0;
                    int treeIsClosest = 0;
                    int dynamicIsClosest = 0;
                    int skyIsClosest = 0;

                    if (closestObjectType == "buildingClosestDist")
                    {
                        buildingIsClosest = 1;
                    }
                    else if (closestObjectType == "equipmentClosestDist")
                    {
                        equipmentIsClosest = 1;
                    }
                    else if (closestObjectType == "treeClosestDist")
                    {
                        treeIsClosest = 1;
                    }
                    else if (closestObjectType == "dynamicClosestDist")
                    {
                        dynamicIsClosest = 1;
                    }
                    else if (closestObjectType == "skyClosestDist")
                    {
                        skyIsClosest = 1;
                    }

                    roomsensor.viewResultsRm[k, j].ResultData.BuildingPtsCountRatio = (buildingPts.Count / total);
                    roomsensor.viewResultsRm[k, j].ResultData.EquipmentPtsCountRatio = (equipmentPts.Count / total);
                    roomsensor.viewResultsRm[k, j].ResultData.TreePtsCountRatio = (treePts.Count / total);
                    roomsensor.viewResultsRm[k, j].ResultData.PavementPtsCountRatio = (pavementPts.Count / total);
                    roomsensor.viewResultsRm[k, j].ResultData.GrassPtsCountRatio = (grassPts.Count / total);
                    roomsensor.viewResultsRm[k, j].ResultData.WaterPtsCountRatio = (waterPts.Count / total);
                    roomsensor.viewResultsRm[k, j].ResultData.DynamicPtsRatio = (dynamicPts.Count / total);
                    roomsensor.viewResultsRm[k, j].ResultData.SkyPtsCountRatio = (skyPts.Count / total);
                    roomsensor.viewResultsRm[k, j].ResultData.ElementNumber = elementNumber;
                    roomsensor.viewResultsRm[k, j].ResultData.FloorHeights = floorHeights;
                    roomsensor.viewResultsRm[k, j].ResultData.BuildingClosestDist = buildingClosestDist;
                    roomsensor.viewResultsRm[k, j].ResultData.EquipmentClosestDist = equipmentClosestDist;
                    roomsensor.viewResultsRm[k, j].ResultData.TreeClosestDist = treeClosestDist;
                    roomsensor.viewResultsRm[k, j].ResultData.GrassClosestDist = grassClosestDist;
                    roomsensor.viewResultsRm[k, j].ResultData.WaterClosestDist = waterClosestDist;
                    roomsensor.viewResultsRm[k, j].ResultData.DynamicClosestDist = dynamicClosestDist;


                }
            }
        }

    }
}
