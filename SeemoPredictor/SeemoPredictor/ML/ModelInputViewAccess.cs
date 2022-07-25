using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace SeemoPredictor
{
    public class ModelInputViewAccess
    {

        [LoadColumn(0)]
        [ColumnName(@"WindowAreaSum")]
        public float WindowAreaSum { get; set; }

        [LoadColumn(1)]
        [ColumnName(@"Z1PtsCountRatio")]
        public float Z1PtsCountRatio { get; set; }

        [LoadColumn(2)]
        [ColumnName(@"Z2PtsCountRatio")]
        public float Z2PtsCountRatio { get; set; }

        [LoadColumn(3)]
        [ColumnName(@"Z3PtsCountRatio")]
        public float Z3PtsCountRatio { get; set; }

        [LoadColumn(4)]
        [ColumnName(@"Z4PtsCountRatio")]
        public float Z4PtsCountRatio { get; set; }

        [LoadColumn(5)]
        [ColumnName(@"InteriorPtsCountRatio")]
        public float InteriorPtsCountRatio { get; set; }

        [LoadColumn(6)]
        [ColumnName(@"BuildingPtsCountRatio")]
        public float BuildingPtsCountRatio { get; set; }

        [LoadColumn(7)]
        [ColumnName(@"ContextWindowPtsCountRatio")]
        public float ContextWindowPtsCountRatio { get; set; }

        [LoadColumn(8)]
        [ColumnName(@"EquipmentPtsCountRatio")]
        public float EquipmentPtsCountRatio { get; set; }

        [LoadColumn(9)]
        [ColumnName(@"LandmarkPtsCountRatio")]
        public float LandmarkPtsCountRatio { get; set; }

        [LoadColumn(10)]
        [ColumnName(@"SidewalkPtsCountRatio")]
        public float SidewalkPtsCountRatio { get; set; }

        [LoadColumn(11)]
        [ColumnName(@"RoadPtsCountRatio")]
        public float RoadPtsCountRatio { get; set; }

        [LoadColumn(12)]
        [ColumnName(@"ParkingLotPtsCountRatio")]
        public float ParkingLotPtsCountRatio { get; set; }

        [LoadColumn(13)]
        [ColumnName(@"TreePtsCountRatio")]
        public float TreePtsCountRatio { get; set; }

        [LoadColumn(14)]
        [ColumnName(@"GrassPtsCountRatio")]
        public float GrassPtsCountRatio { get; set; }

        [LoadColumn(15)]
        [ColumnName(@"WaterPtsCountRatio")]
        public float WaterPtsCountRatio { get; set; }

        [LoadColumn(16)]
        [ColumnName(@"SkyPtsCountRatio")]
        public float SkyPtsCountRatio { get; set; }

        [LoadColumn(17)]
        [ColumnName(@"ElementNumber")]
        public float ElementNumber { get; set; }

        [LoadColumn(18)]
        [ColumnName(@"FloorHeights")]
        public float FloorHeights { get; set; }

        [LoadColumn(19)]
        [ColumnName(@"InteriorClosestDist")]
        public float InteriorClosestDist { get; set; }

        [LoadColumn(20)]
        [ColumnName(@"BuildingClosestDist")]
        public float BuildingClosestDist { get; set; }

        [LoadColumn(21)]
        [ColumnName(@"ContextWindowClosestDist")]
        public float ContextWindowClosestDist { get; set; }

        [LoadColumn(22)]
        [ColumnName(@"EquipmentClosestDist")]
        public float EquipmentClosestDist { get; set; }

        [LoadColumn(23)]
        [ColumnName(@"LandmarkClosestDist")]
        public float LandmarkClosestDist { get; set; }

        [LoadColumn(24)]
        [ColumnName(@"SidewalkClosestDist")]
        public float SidewalkClosestDist { get; set; }

        [LoadColumn(25)]
        [ColumnName(@"RoadClosestDist")]
        public float RoadClosestDist { get; set; }

        [LoadColumn(26)]
        [ColumnName(@"ParkingLotClosestDist")]
        public float ParkingLotClosestDist { get; set; }

        [LoadColumn(27)]
        [ColumnName(@"TreeClosestDist")]
        public float TreeClosestDist { get; set; }

        [LoadColumn(28)]
        [ColumnName(@"GrassClosestDist")]
        public float GrassClosestDist { get; set; }

        [LoadColumn(29)]
        [ColumnName(@"WaterClosestDist")]
        public float WaterClosestDist { get; set; }


        [LoadColumn(30)]
        [ColumnName("Label")]
        public float ViewAccessS { get; set; }
    }


}
