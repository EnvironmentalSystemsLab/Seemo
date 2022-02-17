using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace SeemoPredictor
{
    public class ModelInput
    {

        [LoadColumn(0)]
        public float WindowNumber { get; set; }

        [LoadColumn(1)]
        public float WindowAreaSum { get; set; }

        [LoadColumn(30)]
        public float Z1PtsCountRatio { get; set; }

        [LoadColumn(31)]
        public float Z2PtCountRatio { get; set; }

        [LoadColumn(33)]
        public float Z3PtsCountRatio { get; set; }

        [LoadColumn(34)]
        public float Z4PtsCountRatio { get; set; }

        [LoadColumn(44)]
        public float BuildingPtsCountRatio { get; set; }

        [LoadColumn(45)]
        public float EquipmentPtsCountRatio { get; set; }

        [LoadColumn(46)]
        public float TreePtsCountRatio { get; set; }

        [LoadColumn(47)]
        public float PavementPtsCountRatio { get; set; }

        [LoadColumn(48)]
        public float GrassPtsCountRatio { get; set; }

        [LoadColumn(49)]
        public float WaterPtsCountRatio { get; set; }

        [LoadColumn(50)]
        public float DynamicPtsRatio { get; set; }

        [LoadColumn(52)]
        public float SkyPtsCountRatio { get; set; }

        [LoadColumn(59)]
        public float ElementNumber { get; set; }

        [LoadColumn(60)]
        public float FloorHeights { get; set; }

        [LoadColumn(61)]
        public float BuildingClosestDist { get; set; }

        [LoadColumn(62)]
        public float EquipmentClosestDist { get; set; }

        [LoadColumn(63)]
        public float TreeClosestDist { get; set; }

        [LoadColumn(64)]
        public float GrassClosestDist { get; set; }

        [LoadColumn(65)]
        public float WaterClosestDist { get; set; }

        [LoadColumn(67)]
        public float DynamicClosestDist { get; set; }

        [LoadColumn(69)]
        public float SkyCondition { get; set; }
        /*
        [LoadColumn(70)]
        [ColumnName("ViewContentLabel")]
        public float ViewContentS { get; set; }

        [LoadColumn(71)]
        //[ColumnName("Label")]
        public float ViewAccessS { get; set; }

        [LoadColumn(72)]
        //[ColumnName("Label")]
        public float PrivacyS { get; set; }
        */
        [LoadColumn(73)]
        [ColumnName("Label")]
        public float OverallRatingS { get; set; }


    }


}
