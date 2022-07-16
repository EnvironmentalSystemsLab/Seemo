using Newtonsoft.Json;
using SeemoPredictor.Geometry;
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

        public Point3 Pt { get; set; }

        public Point3[] Dirs { get; set; }

        public List<DirectionResult> DirectionsResults { get; set; } = new List<DirectionResult>();
    }

    public class DirectionResult
    {
        public Point3 Dir { get; set; }

        [JsonIgnore]
        public SmoImage Image { get; set; }

/*
        [JsonIgnore]
        public List<SmoPoint3> RayCastHits { get; set; } = new List<SmoPoint3>();
        
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
        public double WindowNumber { get; set; } = 2;
        public double WindowAreaRatio { get; set; } = 0;

        public double Z1PtsCountRatio { get; set; } = 0;
        public double Z2PtsCountRatio { get; set; } = 0;
        public double Z3PtsCountRatio { get; set; } = 0;
        public double Z4PtsCountRatio { get; set; } = 0;

        public double InteriorPtsCountRatio { get; set; } = 0;
        public double BuildingPtsCountRatio { get; set; } = 0;  //Building includes exterior + context.building
        public double ContextWindowPtsCountRatio { get; set; } = 0;
        public double EquipmentPtsCountRatio { get; set; } = 0;
        public double LandmarkPtsCountRatio { get; set; } = 0;
        public double SidewalkPtsCountRatio { get; set; } = 0;
        public double RoadPtsCountRatio { get; set; } = 0;
        public double ParkingLotPtsCountRatio { get; set; } = 0;
        public double TreePtsCountRatio { get; set; } = 0;
        public double GrassPtsCountRatio { get; set; } = 0;
        public double WaterPtsCountRatio { get; set; } = 0;
       
        public double SkyPtsCountRatio { get; set; } = 0;
        public double ElementNumber { get; set; } = 0;
        public double FloorHeights { get; set; } = 0;

        public double InteriorClosestDist { get; set; } = 0;
        public double BuildingClosestDist { get; set; } = 0;
        public double ContextWindowClosestDist { get; set; } = 0;
        public double EquipmentClosestDist { get; set; } = 0;
        public double LandmarkClosestDist { get; set; } = 0;
        public double SidewalkClosestDist { get; set; } = 0;
        public double RoadClosestDist { get; set; } = 0;
        public double ParkingLotClosestDist { get; set; } = 0;
        public double TreeClosestDist { get; set; } = 0;
        public double GrassClosestDist { get; set; } = 0;
        public double WaterClosestDist { get; set; } = 0;

        public double SkyCondition { get; set; } = 0;

        
        public double ViewContentFramework { get; set; } = 0;
        public double SPVEI { get; set; } = 0;
        //public double IPVEI { get; set; } = 0;

        public double PredictedOverallRating { get; set; } = 0;
        public double PredictedViewContent { get; set; } = 0;
        public double PredictedViewAccess { get; set; } = 0;
        public double PredictedPrivacy { get; set; } = 0;


        public DirectionResult()
        {

        }


        public DirectionResult(double _WindowAreaSum,
            double _Z1PtsCountRatio, double _Z2PtsCountRatio,
            double _Z3PtsCountRatio, double _Z4PtsCountRatio,
            double _InteriorPtsCountRatio, double _BuildingPtsCountRatio, double _ContextWindowPtsCountRatio, double _EquipmentPtsCountRatio, double _LandmarkPtsCountRatio, 
            double _SidewalkPtsCountRatio, double _RoadPtsCountRatio, double _ParkingLotPtsCountRatio, 
            double _TreePtsCountRatio, double _GrassPtsCountRatio, double _WaterPtsCountRatio, 
            
            double _SkyPtsCountRatio,
            double _ElementNumber, double _FloorHeights,

            double _InteriorClosestDist, double _BuildingClosestDist, double _ContextWindowClosestDist, double _EquipmentClosestDist, double _LandmarkClosestDist,
            double _SidewalkClosestDist, double _RoadClosestDist, double _ParkingLotClosestDist,
            double _TreeClosestDist, double _GrassClosestDist, double _WaterClosestDist
            
            )
        {
            WindowNumber = 2;
            WindowAreaRatio = _WindowAreaSum;

            Z1PtsCountRatio = _Z1PtsCountRatio;
            Z2PtsCountRatio = _Z2PtsCountRatio;

            Z3PtsCountRatio = _Z3PtsCountRatio;
            Z4PtsCountRatio = _Z4PtsCountRatio;

            InteriorPtsCountRatio = _InteriorPtsCountRatio;
            BuildingPtsCountRatio = _BuildingPtsCountRatio;
            ContextWindowPtsCountRatio = _ContextWindowPtsCountRatio;
            EquipmentPtsCountRatio = _EquipmentPtsCountRatio;
            LandmarkPtsCountRatio = _LandmarkPtsCountRatio;
            
            SidewalkPtsCountRatio = _SidewalkPtsCountRatio;
            RoadPtsCountRatio = _RoadPtsCountRatio;
            ParkingLotPtsCountRatio = _ParkingLotPtsCountRatio;

            TreePtsCountRatio = _TreePtsCountRatio;
            GrassPtsCountRatio = _GrassPtsCountRatio;
            WaterPtsCountRatio = _WaterPtsCountRatio;

            SkyPtsCountRatio = _SkyPtsCountRatio;

            ElementNumber = _ElementNumber;
            FloorHeights = _FloorHeights;

            InteriorClosestDist = _InteriorClosestDist;
            BuildingClosestDist = _BuildingClosestDist;
            ContextWindowClosestDist = _ContextWindowClosestDist;
            EquipmentClosestDist = _EquipmentClosestDist;
            LandmarkClosestDist = _LandmarkClosestDist;

            SidewalkClosestDist = _SidewalkClosestDist;
            RoadClosestDist = _RoadClosestDist;
            ParkingLotClosestDist = _ParkingLotClosestDist;

            TreeClosestDist = _TreeClosestDist;
            GrassClosestDist = _GrassClosestDist;
            WaterClosestDist = _WaterClosestDist;

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
            int z1hit = (Image.yres - 2 * a) * (Image.xres - 2 * c);
            int z2hit = rayTotal - z1hit - z3hit - z4hit;


            List<double> interiorDists = new List<double>();
            List<double> buildingDists = new List<double>();
            List<double> contextWindowDists = new List<double>();
            List<double> equipmentDists = new List<double>();
            List<double> landmarkDists = new List<double>();

            List<double> sidewalkDists = new List<double>();
            List<double> roadDists = new List<double>();
            List<double> parkingLotDists = new List<double>();

            List<double> treeDists = new List<double>();
            List<double> grassDists = new List<double>();
            List<double> waterDists = new List<double>();

            List<double> skyDists = new List<double>();

            for (int x = 0; x < Image.LabelMap.Length; x++)
            {
                for (int y = 0; y < Image.LabelMap[x].Length; y++)
                {
                    SmoFace.SmoFaceType type = Image.LabelMap[x][y];
                    double dist = Image.DepthMap[x][y];
                    double windowDist = Image.WindowDepthMap[x][y]; //애네가 000
                    Point3 windowN = Image.WindowNormals[x][y]; //애네가 000
                    Point3 ray = Image.ImageRays[x][y];

                    //if(windowDist>0.01 && windowDist < 100)
                    //{
                    //    this.WindowAreaSum = this.WindowAreaSum + Math.Pow(Math.Tan(Image.angleStep * Math.PI / 180) * windowDist, 2);
                    //}
                    
                    switch (type)
                    {
                        case SmoFace.SmoFaceType.Interior:
                            // calculating zoneCount
                            interiorDists.Add(dist);
                            if (y < a) z4hit--;
                            else if (y > b) z3hit--;
                            else if (x < c || x > d) z2hit--;
                            else z1hit--;

                            break;
                        case SmoFace.SmoFaceType.Exterior:
                            buildingDists.Add(dist);
                            break;
                        case SmoFace.SmoFaceType.Context_Building:
                            buildingDists.Add(dist);
                            break;
                        case SmoFace.SmoFaceType.Context_Window:
                            contextWindowDists.Add(dist);
                            break;
                        case SmoFace.SmoFaceType.Equipment:
                            equipmentDists.Add(dist);
                            break;
                        case SmoFace.SmoFaceType.Landmark:
                            landmarkDists.Add(dist);
                            break;

                        case SmoFace.SmoFaceType.Sidewalk:
                            sidewalkDists.Add(dist);
                            break;
                        case SmoFace.SmoFaceType.Road:
                            roadDists.Add(dist);
                            break;
                        case SmoFace.SmoFaceType.ParkingLot:
                            parkingLotDists.Add(dist);
                            break;

                        case SmoFace.SmoFaceType.Tree:
                            treeDists.Add(dist);
                            break;
                        case SmoFace.SmoFaceType.Grass:
                            grassDists.Add(dist);
                            break;
                        case SmoFace.SmoFaceType.Water:
                            waterDists.Add(dist);
                            break;
                        
                    }

                    
                    double CalculatePVEI()
                    {
                        double rayPVEI = 0;
                        // calculate PVEI
                        Point3 windowNtemp = windowN.Normalized;
                        windowNtemp = new Point3(windowNtemp.X, windowNtemp.Y, 0);
                        windowNtemp = Point3.Rotate(windowNtemp, Point3.ZAxis, (float)Math.PI / 2);
                        Point3 raytemp = new Point3(ray.X, ray.Y, 0);

                        rayPVEI = dist * dist * windowDist * windowDist / (dist - windowDist) / (dist - windowDist); //later multiply tan(angleStep)^4 then, it's Aobserver * Aprojected / (do - dprojected)^2
                        rayPVEI = rayPVEI * (Point3.Cross(windowNtemp, raytemp).Length) / windowNtemp.Length / raytemp.Length;// multiply sin(alpha)
                        rayPVEI = rayPVEI * (raytemp.Length / ray.Length); //multiply cosin

                        if (y < a) rayPVEI = rayPVEI * 2.0f / 13.0f;
                        else if (y > b) rayPVEI = rayPVEI * 2.0f / 13.0f;
                        else if (x < c || x > d) rayPVEI = rayPVEI * 3.0f / 13.0f;
                        else rayPVEI = rayPVEI * 6.0f / 13.0f;

                        double pixelLength = Math.Tan(Image.angleStep / 180 * Math.PI);
                        rayPVEI = rayPVEI * Math.Pow(pixelLength, 4); //it should be 4 but.. to small
                        return rayPVEI;
                    }

                    if (type == SmoFace.SmoFaceType.Context_Window || type == SmoFace.SmoFaceType.Sidewalk || type == SmoFace.SmoFaceType.Road || type == SmoFace.SmoFaceType.ParkingLot)
                    {
                        if(dist>1 && dist < 200)
                        {
                            this.SPVEI = this.SPVEI + ( 1 / dist );
                        }
                    }
                }
            }

            this.SPVEI = SPVEI * 10 / (Image.xres * Image.yres);
            
            //calculate ml inputs
            double zhitSum = z1hit + z2hit + z3hit + z4hit;

            //double r = Image.xres/1440; // when resolution is 1440
            //this.WindowAreaRatio = (zhitSum / (rayTotal/r));

            double r = 0.05275781019; // come from survey window area with ray counts 
            this.WindowAreaRatio = (zhitSum / (rayTotal * r));

            this.Z1PtsCountRatio = (double)z1hit / zhitSum;
            this.Z2PtsCountRatio = (double)z2hit / zhitSum;
            this.Z3PtsCountRatio = (double)z3hit / zhitSum;
            this.Z4PtsCountRatio = (double)z4hit / zhitSum;
            
            this.SkyCondition = 1;

            //this.InteriorPtsCountRatio = ((double)(interiorDists.Count)) / zhitSum;
            this.BuildingPtsCountRatio = ((double)(buildingDists.Count)) / zhitSum;
            this.ContextWindowPtsCountRatio = ((double)(contextWindowDists.Count)) / zhitSum;
            this.EquipmentPtsCountRatio = ((double)(equipmentDists.Count)) / zhitSum;
            this.LandmarkPtsCountRatio = ((double)(landmarkDists.Count)) / zhitSum;

            this.SidewalkPtsCountRatio = ((double)(sidewalkDists.Count)) / zhitSum;
            this.RoadPtsCountRatio = ((double)(roadDists.Count)) / zhitSum;
            this.ParkingLotPtsCountRatio = ((double)(parkingLotDists.Count)) / zhitSum;

            this.TreePtsCountRatio = ((double)(treeDists.Count)) / zhitSum;
            this.GrassPtsCountRatio = ((double)(grassDists.Count)) / zhitSum;
            this.WaterPtsCountRatio = ((double)(waterDists.Count)) / zhitSum;

            this.SkyPtsCountRatio = (1 - this.BuildingPtsCountRatio - this.ContextWindowPtsCountRatio - this.EquipmentPtsCountRatio - this.LandmarkPtsCountRatio 
                - this.SidewalkPtsCountRatio - this.RoadPtsCountRatio - this.ParkingLotPtsCountRatio
                - this.TreePtsCountRatio - this.GrassPtsCountRatio - this.WaterPtsCountRatio
                );

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

            this.InteriorClosestDist = ComputeClosestDist(interiorDists);
            this.BuildingClosestDist = ComputeClosestDist(buildingDists);
            this.ContextWindowClosestDist = ComputeClosestDist(contextWindowDists);
            this.EquipmentClosestDist = ComputeClosestDist(equipmentDists);
            this.LandmarkClosestDist = ComputeClosestDist(landmarkDists);

            this.SidewalkClosestDist = ComputeClosestDist(sidewalkDists);
            this.RoadClosestDist = ComputeClosestDist(roadDists);
            this.ParkingLotClosestDist = ComputeClosestDist(parkingLotDists);

            this.TreeClosestDist = ComputeClosestDist(treeDists);
            this.GrassClosestDist = ComputeClosestDist(grassDists);
            this.WaterClosestDist = ComputeClosestDist(waterDists);

            //calculate visible element types count
            List<int> elementPts = new List<int> { buildingDists.Count, contextWindowDists.Count, equipmentDists.Count, landmarkDists.Count, 
                sidewalkDists.Count, roadDists.Count, parkingLotDists.Count, 
                treeDists.Count, grassDists.Count, waterDists.Count, 
                skyDists.Count };

            int elementNumber = 0;
            for (int i = 0; i < elementPts.Count; i++)
            {
                if (elementPts[i] > 10)
                {
                    elementNumber++;
                }
            }

            this.ElementNumber = elementNumber;
            if(SkyPtsCountRatio > 0) { this.SkyCondition = 1; } else
            {
                this.SkyCondition = 0;
            }
            this.FloorHeights = this.ViewPointZ;

            //calculate viewContentFramework
            double Lsky = 0;
            double Llandscape = 0;
            double Lground = 0;
            double Lnautre = 0;

            List<double> LandscapeDists = new List<double>();
            LandscapeDists.AddRange(buildingDists);
            LandscapeDists.AddRange(contextWindowDists);
            LandscapeDists.AddRange(equipmentDists);
            LandscapeDists.AddRange(LandscapeDists);
            LandscapeDists.Sort();

            List<double> DynamicDists = new List<double>(); // also represents ground
            DynamicDists.AddRange(sidewalkDists);
            DynamicDists.AddRange(roadDists);
            DynamicDists.AddRange(parkingLotDists);
            DynamicDists.Sort();

            double NatureRatio = 0;
            NatureRatio = TreePtsCountRatio + GrassPtsCountRatio + WaterPtsCountRatio;

            if(SkyPtsCountRatio > 0) { Lsky = 0.25; }
            
            if(LandscapeDists.Count!= 0)
            {
                double dist = LandscapeDists.Average();
                if(dist > 50) { Llandscape = 1 * 0.25; }
                else if( dist > 20) { Llandscape = 0.75 * 0.25; }
                else if( dist > 6) { Llandscape = 0.5 * 0.25; }
                else { Llandscape = 0; }
            }
            else
            {
                Llandscape = 0;
            }

            if (DynamicDists.Count != 0)
            {
                
                if (DynamicDists[DynamicDists.Count-1] > 6) { Lground = 1 * 0.25; }
                else if (DynamicDists[0] <= 6 ) { Lground = 0 * 0.25; }
                else { Lground = 0.5 * 0.25; }
            }
            else
            {
                Lground = 0;
            }

            if (NatureRatio > 0)
            {
                if(NatureRatio > 0.5) { Lnautre = 1 * 0.25; }
                else if(NatureRatio > 0.25) { Lnautre = 0.75 * 0.25; }
                else { Lnautre = 0.5 * 0.25; }
            }
            else
            {
                Lnautre = 0;
            }

            this.ViewContentFramework = Lsky + Llandscape + Lground + Lnautre;

        }

    }

}
