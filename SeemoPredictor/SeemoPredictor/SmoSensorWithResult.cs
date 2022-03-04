using Newtonsoft.Json;
using SeemoPredictor.SeemoGeo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeemoPredictor
{

    public class SmoSensorWithResults
    {
        public int NodeID { get; set; }

        public SmoPoint3 Pt { get; set; }

        public SmoPoint3[] Dirs { get; set; }

        public List<DirectionResult> DirectionsResults { get; set; } = new List<DirectionResult>();
    }

    public class DirectionResult
    {
        public SmoPoint3 Dir { get; set; }

        [JsonIgnore]
        public SmoImage Image { get; set; }




        [JsonIgnore]
        public List<SmoPoint3> RayCastHits { get; set; } = new List<SmoPoint3>();
        /*
        [JsonIgnore]
        public List<SmoPoint3> sceneRayVectorsZ1 { get; set; }
        [JsonIgnore]
        public List<SmoPoint3> sceneRayVectorsZ2 { get; set; }
        [JsonIgnore]
        public List<SmoPoint3> sceneRayVectorsZ3 { get; set; }
        [JsonIgnore]
        public List<SmoPoint3> sceneRayVectorsZ4 { get; set; }

        */


        public string ID { get; set; }
        public double ViewPointX { get; set; }
        public double ViewPointY { get; set; }
        public double ViewPointZ { get; set; }
        public double ViewVectorX { get; set; }
        public double ViewVectorY { get; set; }
        public double ViewVectorZ { get; set; }
        public double WindowNumber { get; set; } = 0;
        public double WindowAreaSum { get; set; } = 0;
        public double Z1PtsCountRatio { get; set; } = 0;
        public double Z2PtsCountRatio { get; set; } = 0;
        public double Z3PtsCountRatio { get; set; } = 0;
        public double Z4PtsCountRatio { get; set; } = 0;
        public double BuildingPtsCountRatio { get; set; } = 0;
        public double EquipmentPtsCountRatio { get; set; } = 0;
        public double TreePtsCountRatio { get; set; } = 0;
        public double PavementPtsCountRatio { get; set; } = 0;
        public double GrassPtsCountRatio { get; set; } = 0;
        public double WaterPtsCountRatio { get; set; } = 0;
        public double DynamicPtsRatio { get; set; } = 0;
        public double SkyPtsCountRatio { get; set; } = 0;
        public double ElementNumber { get; set; } = 0;
        public double FloorHeights { get; set; } = 0;
        public double BuildingClosestDist { get; set; } = 0;
        public double EquipmentClosestDist { get; set; } = 0;
        public double TreeClosestDist { get; set; } = 0;
        public double GrassClosestDist { get; set; } = 0;
        public double WaterClosestDist { get; set; } = 0;
        public double DynamicClosestDist { get; set; } = 0;
        public double SkyCondition { get; set; } = 0;


        public double PredictedOverallRating { get; set; } = 0;
        public double PredictedViewContent { get; set; } = 0;
        public double PredictedViewAccess { get; set; } = 0;
        public double PredictedPrivacy { get; set; } = 0;


        public DirectionResult()
        {

        }


        public DirectionResult(double _WindowNumber, double _WindowAreaSum,
            double _Z1PtsCountRatio, double _Z2PtCountRatio,
            double _Z3PtsCountRatio, double _Z4PtsCountRatio,
            double _BuildingPtsCountRatio, double _EquipmentPtsCountRatio, double _TreePtsCountRatio, double _PavementPtsCountRatio, double _GrassPtsCountRatio, double _WaterPtsCountRatio, double _DynamicPtsRatio,
            double _SkyPtsCountRatio,
            double _ElementNumber, double _FloorHeights, double _BuildingClosestDist, double _EquipmentClosestDist, double _TreeClosestDist,
            double _GrassClosestDist, double _WaterClosestDist,
            double _DynamicClosestDist
            )
        {
            WindowNumber = _WindowNumber;
            WindowAreaSum = _WindowAreaSum;

            Z1PtsCountRatio = _Z1PtsCountRatio;
            Z2PtsCountRatio = _Z2PtCountRatio;
            Z3PtsCountRatio = _Z3PtsCountRatio;
            Z4PtsCountRatio = _Z4PtsCountRatio;

            BuildingPtsCountRatio = _BuildingPtsCountRatio;
            EquipmentPtsCountRatio = _EquipmentPtsCountRatio;
            TreePtsCountRatio = _TreePtsCountRatio;
            PavementPtsCountRatio = _PavementPtsCountRatio;
            GrassPtsCountRatio = _GrassPtsCountRatio;
            WaterPtsCountRatio = _WaterPtsCountRatio;
            DynamicPtsRatio = _DynamicPtsRatio;
            SkyPtsCountRatio = _SkyPtsCountRatio;
            ElementNumber = _ElementNumber;
            FloorHeights = _FloorHeights;
            BuildingClosestDist = _BuildingClosestDist;
            EquipmentClosestDist = _EquipmentClosestDist;
            TreeClosestDist = _TreeClosestDist;
            GrassClosestDist = _GrassClosestDist;
            WaterClosestDist = _WaterClosestDist;
            DynamicClosestDist = _DynamicClosestDist;

        }



        public void ComputeFeatures()
        {
            int a = (int)(Image.yres / 3);
            int b = (int)((Image.yres - 1) - (Image.yres / 3));
            int c = (int)(Image.xres / 3);
            int d = (int)((Image.xres - 1) - (Image.xres / 3));

            int rayTotal = Image.xres * Image.yres;
            int z4hit = Image.xres * a;
            int z3hit = Image.xres * a;
            int z1hit = (Image.yres - 2 * a)*(Image.xres - 2 * c);
            int z2hit = rayTotal - z1hit - z3hit - z4hit;
            
            
            List<double> buildingDists = new List<double>();
            List<double> equipmentDists = new List<double>();
            List<double> treeDists = new List<double>();
            List<double> pavementDists = new List<double>();
            List<double> grassDists = new List<double>();
            List<double> waterDists = new List<double>();
            List<double> dynamicDists = new List<double>();
            List<double> skyDists = new List<double>();

            


            for (int x = 0; x < Image.LabelMap.Length; x++)
            {
                for(int y = 0; y < Image.LabelMap[x].Length; y++)
                {
                    SmoFace.SmoFaceType type = Image.LabelMap[x][y];
                    double dist = Image.DepthMap[x][y];

                    switch (type)
                    {
                        case SmoFace.SmoFaceType.Interior:
                            // calculating zoneCount
                            if (y < a) z4hit--;
                            else if(y > b) z3hit--;
                            else if(x < c || x > d) z2hit--;
                            else z1hit--;
                            
                            break;
                        case SmoFace.SmoFaceType.Building:
                            buildingDists.Add(dist);
                            break;
                        case SmoFace.SmoFaceType.Equipment:
                            equipmentDists.Add(dist);
                            break;
                        case SmoFace.SmoFaceType.Tree:
                            treeDists.Add(dist);
                            break;
                        case SmoFace.SmoFaceType.Pavement:
                            pavementDists.Add(dist);
                            break;
                        case SmoFace.SmoFaceType.Grass:
                            grassDists.Add(dist);
                            break;
                        case SmoFace.SmoFaceType.Water:
                            waterDists.Add(dist);
                            break;
                        case SmoFace.SmoFaceType.Dynamic:
                            dynamicDists.Add(dist);
                            break;
                    }




                }
            }


            //calculate ml inputs
             double zhitSum = z1hit + z2hit + z3hit + z4hit;

            double r = 0.05275781019; // come from survey window area with ray counts
            this.WindowAreaSum = (zhitSum / (rayTotal*r));

            this.Z1PtsCountRatio = (double)z1hit / zhitSum;
            this.Z2PtsCountRatio = (double)z2hit / zhitSum;
            this.Z3PtsCountRatio = (double)z3hit / zhitSum;
            this.Z4PtsCountRatio = (double)z4hit / zhitSum;

            //***********Without WindowMesh How can I count window number???
            ////Previous Method: Count how many window are visible
            ////int[] winIhit;
            //List<int> hitWindows = new List<int>();

            //for (int i = 0; i < winRayVectors0.Count; i++)
            //{
            //    Vector3d ray = winRayVectors0[i];
            //    ray.Unitize();
            //    Ray3d ray1 = new Ray3d(vp, ray);

            //    for (int q = 0; q < windowMeshs.Count; q++)
            //    {
            //        var dtr = Rhino.Geometry.Intersect.Intersection.MeshRay(windowMeshs[q], ray1);
            //        if (dtr > 0)
            //        {
            //            hitWindows.Add(q);
            //        }
            //    }
            //}
            //hitWindows = hitWindows.Distinct().ToList();
            //wn = hitWindows.Count;
            this.WindowNumber = 2;
            this.SkyCondition = 1;

            this.BuildingPtsCountRatio = ((double)(buildingDists.Count)) / zhitSum;
            this.EquipmentPtsCountRatio = ((double)(equipmentDists.Count)) / zhitSum;
            this.TreePtsCountRatio = ((double)(treeDists.Count)) / zhitSum;
            this.PavementPtsCountRatio = ((double)(pavementDists.Count)) / zhitSum;
            this.GrassPtsCountRatio = ((double)(grassDists.Count)) / zhitSum;
            this.WaterPtsCountRatio = ((double)(waterDists.Count)) / zhitSum;
            this.DynamicPtsRatio = ((double)(dynamicDists.Count)) / zhitSum;
            this.SkyPtsCountRatio = (1 - this.BuildingPtsCountRatio - this.EquipmentPtsCountRatio - this.TreePtsCountRatio - this.PavementPtsCountRatio - this.GrassPtsCountRatio - this.WaterPtsCountRatio - this.DynamicPtsRatio);

            //calculate object's closest distance and type
            double ComputeClosestDist(List<double> dists)
            {
                double closeDistance = 0;
                if (dists.Count == 0)
                {
                    return 50000;
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

            this.BuildingClosestDist = ComputeClosestDist(buildingDists);
            this.EquipmentClosestDist = ComputeClosestDist(equipmentDists);
            this.TreeClosestDist = ComputeClosestDist(treeDists);
            this.GrassClosestDist = ComputeClosestDist(grassDists);
            this.WaterClosestDist = ComputeClosestDist(waterDists);
            this.DynamicClosestDist = ComputeClosestDist(dynamicDists);


            //calculate visible element types count
            List<int> elementPts = new List<int> { buildingDists.Count, equipmentDists.Count, treeDists.Count, pavementDists.Count, grassDists.Count, waterDists.Count, dynamicDists.Count, skyDists.Count };

            int elementNumber = 0;
            for (int i = 0; i < elementPts.Count; i++)
            {
                if (elementPts[i] > 10)
                {
                    elementNumber++;
                }
            }

            this.ElementNumber = elementNumber;

            this.SkyCondition = 1;
            this.FloorHeights = this.ViewPointZ;

        }

    }

}
