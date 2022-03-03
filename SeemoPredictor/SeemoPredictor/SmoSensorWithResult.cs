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
        public List<SmoPoint3> RayCastHits { get; set; } = new List<SmoPoint3>();
        [JsonIgnore]
        public List<SmoPoint3> sceneRayVectorsZ1 { get; set; }
        [JsonIgnore]
        public List<SmoPoint3> sceneRayVectorsZ2 { get; set; }
        [JsonIgnore]
        public List<SmoPoint3> sceneRayVectorsZ3 { get; set; }
        [JsonIgnore]
        public List<SmoPoint3> sceneRayVectorsZ4 { get; set; }

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
    }

}
