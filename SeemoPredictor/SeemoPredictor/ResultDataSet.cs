using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rhino.Geometry;

namespace SeemoPredictor
{

    public class SeemoResult
    {
        string TimeStamp { get; set; }
        string SeemoVersion { get; set; }
       
        public List<Node> Results { get; set; } = new List<Node>();

        public string ToJSON() {
            return JsonConvert.SerializeObject(this);
        }

        static public SeemoResult FromJSON(string txt)
        {
            return JsonConvert.DeserializeObject<SeemoResult>(txt);
        }

        public void ToFile (string path)
        {
            File.WriteAllText(path, this.ToJSON());
        }

       static public SeemoResult FromFile(string path)
        {
            var txt = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<SeemoResult>(txt);
        }

        public override string ToString()
        {
            return "Resutls for " + Results.Count + " nodes.";
        }
    }


    public class Node
    {
        public Point3d Pt { get; set; }
        public Vector3d[] Dirs { get; set; }

        public List<ResultDataSet> DirectionsResults { get; set; }
    }

    public class ResultDataSet
    {
        public Vector3d Dir { get; set; }

        public string ID { get; set; } = "Room1:Point2:Dir1";
        public double ViewPointX { get; set; }
        public double ViewPointY { get; set; }
        public double ViewPointZ { get; set; }
        public double ViewVectorX { get; set; }
        public double ViewVectorY { get; set; }
        public double ViewVectorZ { get; set; }
        public double WindowNumber { get; set; } = 0;
        public double WindowAreaSum { get; set; } = 0;
        public double Z1PtsCountRatio { get; set; } = 0;
        public double Z2PtCountRatio { get; set; } = 0;
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





        public ResultDataSet()
        {

        }
        

        public ResultDataSet(double _WindowNumber, double _WindowAreaSum, 
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
            Z2PtCountRatio = _Z2PtCountRatio;
           
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












    [Obsolete]
    public class ViewDir_TD
    {
        public Vector3d Dir;
        public List<ResultDataSet> Results;

    }
    [Obsolete]
    public class ViewPoint_TD
    {
        public Point3d Pt;
        public List<ViewDir_TD> Directions;

    }
    [Obsolete]
    public class Room_TD
    {

        public List<Mesh> Walls;
        public List<ViewPoint_TD> Points;

        public List<ResultDataSet> GetAllResults()
        {

            var dirs = Points.SelectMany(x => x.Directions);
            var res = dirs.SelectMany(x => x.Results);
            return res.ToList();
        }


    }
}

