/*using System;
using System.Collections.Generic;
using CsvHelper;
using System.IO;
using System.Globalization;
using Grasshopper;
using Grasshopper.Kernel.Data;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SeemoPredictor
{
    public class ViewAnalyzerComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ViewAnalyzerComponent class.
        /// </summary>
        public ViewAnalyzerComponent()
          : base("ViewAnalyzer", "ViewAnalyzer",
              "ViewAnalyzer",
              "SeEmo", "3|Analyzer")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {

            pManager.AddGenericParameter("Room Sensor", "Room Sensor", "Room Sensor", GH_ParamAccess.list);
            pManager.AddGenericParameter("Environment", "Environment", "Environment", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Result File Path", "Result File Path", "Result File Path", GH_ParamAccess.item);
            pManager.AddNumberParameter("OverallRatings", "OverallRatings", "OverallRatings", GH_ParamAccess.list);
            pManager.AddNumberParameter("ViewContents", "ViewContents", "ViewContents", GH_ParamAccess.list);
            pManager.AddNumberParameter("ViewAccesses", "ViewAccesses", "ViewAccesses", GH_ParamAccess.list);
            pManager.AddNumberParameter("Privacys", "Privacys", "Privacys", GH_ParamAccess.list);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {



            var seemo_result = new SeemoResult();
            List<Node> nodes = new List<Node>();
            List<SeemoResult> nodeResult = new List<SeemoResult>();
            Vector3d[] dirs;


            SeemoRoom rs = new SeemoRoom();
            List<SeemoRoom> rooms = new List<SeemoRoom>();
            SEnvironment env = new SEnvironment();
            
            List<double> overallRatings = new List<double>();
            List<double> viewContents = new List<double>();
            List<double> viewAccesses = new List<double>();
            List<double> privacys = new List<double>();
            List<List<Vector3d>> winRayVectorsT = new List<List<Vector3d>>();

            DA.GetDataList(0, rooms);
            DA.GetData(1, ref env);

            for (int k = 0; k < rooms.Count; k++)
            {
                rs = rooms[k];
                rs.ComputeRoom();


                SeemoOutdoor rm = new SeemoOutdoor(rs, env);
                rm.ComputeView();

                //make a datatree and consuem machine learning

                for (int i = 0; i < rs.viewResultsRm.GetLength(0); i++)
                {
                    GH_Path treePath = new GH_Path(i);
                    
                    for (int j = 0; j < rs.viewResultsRm.GetLength(1); j++)
                    {
                        
                        rs.viewResultsRm[i, j].ResultData.ID = ("Room" + k.ToString() + ":" + "Point" + i.ToString() + ":" + "Dir" + j.ToString());

                        //Using trained model and making prediction

                        ModelInput sampleDataOverallRating = new ModelInput()
                        {

                            WindowNumber = (float)rs.viewResultsRm[i, j].ResultData.WindowNumber,
                            WindowAreaSum = (float)rs.viewResultsRm[i, j].ResultData.WindowAreaSum,
                            Z1PtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.Z1PtsCountRatio,
                            Z2PtCountRatio = (float)rs.viewResultsRm[i, j].ResultData.Z2PtCountRatio,
                            Z3PtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.Z3PtsCountRatio,
                            Z4PtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.Z4PtsCountRatio,
                            BuildingPtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.BuildingPtsCountRatio,
                            EquipmentPtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.EquipmentPtsCountRatio,
                            TreePtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.TreePtsCountRatio,
                            PavementPtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.PavementPtsCountRatio,
                            GrassPtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.GrassPtsCountRatio,
                            WaterPtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.WaterPtsCountRatio,
                            DynamicPtsRatio = (float)rs.viewResultsRm[i, j].ResultData.DynamicPtsRatio,
                            SkyPtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.SkyPtsCountRatio,
                            ElementNumber = (float)rs.viewResultsRm[i, j].ResultData.ElementNumber,
                            FloorHeights = (float)rs.viewResultsRm[i, j].ResultData.FloorHeights,
                            BuildingClosestDist = (float)rs.viewResultsRm[i, j].ResultData.BuildingClosestDist,
                            EquipmentClosestDist = (float)rs.viewResultsRm[i, j].ResultData.EquipmentClosestDist,
                            TreeClosestDist = (float)rs.viewResultsRm[i, j].ResultData.TreeClosestDist,
                            GrassClosestDist = (float)rs.viewResultsRm[i, j].ResultData.GrassClosestDist,
                            WaterClosestDist = (float)rs.viewResultsRm[i, j].ResultData.WaterClosestDist,
                            DynamicClosestDist = (float)rs.viewResultsRm[i, j].ResultData.DynamicClosestDist,
                            SkyCondition = (float)rs.viewResultsRm[i, j].ResultData.SkyCondition,

                        };

                        ModelInputViewContent sampleDataViewContent = new ModelInputViewContent()
                        {

                            WindowNumber = (float)rs.viewResultsRm[i, j].ResultData.WindowNumber,
                            WindowAreaSum = (float)rs.viewResultsRm[i, j].ResultData.WindowAreaSum,
                            Z1PtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.Z1PtsCountRatio,
                            Z2PtCountRatio = (float)rs.viewResultsRm[i, j].ResultData.Z2PtCountRatio,
                            Z3PtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.Z3PtsCountRatio,
                            Z4PtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.Z4PtsCountRatio,
                            BuildingPtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.BuildingPtsCountRatio,
                            EquipmentPtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.EquipmentPtsCountRatio,
                            TreePtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.TreePtsCountRatio,
                            PavementPtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.PavementPtsCountRatio,
                            GrassPtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.GrassPtsCountRatio,
                            WaterPtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.WaterPtsCountRatio,
                            DynamicPtsRatio = (float)rs.viewResultsRm[i, j].ResultData.DynamicPtsRatio,
                            SkyPtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.SkyPtsCountRatio,
                            ElementNumber = (float)rs.viewResultsRm[i, j].ResultData.ElementNumber,
                            FloorHeights = (float)rs.viewResultsRm[i, j].ResultData.FloorHeights,
                            BuildingClosestDist = (float)rs.viewResultsRm[i, j].ResultData.BuildingClosestDist,
                            EquipmentClosestDist = (float)rs.viewResultsRm[i, j].ResultData.EquipmentClosestDist,
                            TreeClosestDist = (float)rs.viewResultsRm[i, j].ResultData.TreeClosestDist,
                            GrassClosestDist = (float)rs.viewResultsRm[i, j].ResultData.GrassClosestDist,
                            WaterClosestDist = (float)rs.viewResultsRm[i, j].ResultData.WaterClosestDist,
                            DynamicClosestDist = (float)rs.viewResultsRm[i, j].ResultData.DynamicClosestDist,
                            SkyCondition = (float)rs.viewResultsRm[i, j].ResultData.SkyCondition,

                        };
                        ModelInputViewAccess sampleDataViewAccess = new ModelInputViewAccess()
                        {

                            WindowNumber = (float)rs.viewResultsRm[i, j].ResultData.WindowNumber,
                            WindowAreaSum = (float)rs.viewResultsRm[i, j].ResultData.WindowAreaSum,
                            Z1PtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.Z1PtsCountRatio,
                            Z2PtCountRatio = (float)rs.viewResultsRm[i, j].ResultData.Z2PtCountRatio,
                            Z3PtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.Z3PtsCountRatio,
                            Z4PtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.Z4PtsCountRatio,
                            BuildingPtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.BuildingPtsCountRatio,
                            EquipmentPtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.EquipmentPtsCountRatio,
                            TreePtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.TreePtsCountRatio,
                            PavementPtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.PavementPtsCountRatio,
                            GrassPtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.GrassPtsCountRatio,
                            WaterPtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.WaterPtsCountRatio,
                            DynamicPtsRatio = (float)rs.viewResultsRm[i, j].ResultData.DynamicPtsRatio,
                            SkyPtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.SkyPtsCountRatio,
                            ElementNumber = (float)rs.viewResultsRm[i, j].ResultData.ElementNumber,
                            FloorHeights = (float)rs.viewResultsRm[i, j].ResultData.FloorHeights,
                            BuildingClosestDist = (float)rs.viewResultsRm[i, j].ResultData.BuildingClosestDist,
                            EquipmentClosestDist = (float)rs.viewResultsRm[i, j].ResultData.EquipmentClosestDist,
                            TreeClosestDist = (float)rs.viewResultsRm[i, j].ResultData.TreeClosestDist,
                            GrassClosestDist = (float)rs.viewResultsRm[i, j].ResultData.GrassClosestDist,
                            WaterClosestDist = (float)rs.viewResultsRm[i, j].ResultData.WaterClosestDist,
                            DynamicClosestDist = (float)rs.viewResultsRm[i, j].ResultData.DynamicClosestDist,
                            SkyCondition = (float)rs.viewResultsRm[i, j].ResultData.SkyCondition,

                        };

                        ModelInputPrivacy sampleDataPrivacy = new ModelInputPrivacy()
                        {

                            WindowNumber = (float)rs.viewResultsRm[i, j].ResultData.WindowNumber,
                            WindowAreaSum = (float)rs.viewResultsRm[i, j].ResultData.WindowAreaSum,
                            Z1PtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.Z1PtsCountRatio,
                            Z2PtCountRatio = (float)rs.viewResultsRm[i, j].ResultData.Z2PtCountRatio,
                            Z3PtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.Z3PtsCountRatio,
                            Z4PtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.Z4PtsCountRatio,
                            BuildingPtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.BuildingPtsCountRatio,
                            EquipmentPtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.EquipmentPtsCountRatio,
                            TreePtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.TreePtsCountRatio,
                            PavementPtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.PavementPtsCountRatio,
                            GrassPtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.GrassPtsCountRatio,
                            WaterPtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.WaterPtsCountRatio,
                            DynamicPtsRatio = (float)rs.viewResultsRm[i, j].ResultData.DynamicPtsRatio,
                            SkyPtsCountRatio = (float)rs.viewResultsRm[i, j].ResultData.SkyPtsCountRatio,
                            ElementNumber = (float)rs.viewResultsRm[i, j].ResultData.ElementNumber,
                            FloorHeights = (float)rs.viewResultsRm[i, j].ResultData.FloorHeights,
                            BuildingClosestDist = (float)rs.viewResultsRm[i, j].ResultData.BuildingClosestDist,
                            EquipmentClosestDist = (float)rs.viewResultsRm[i, j].ResultData.EquipmentClosestDist,
                            TreeClosestDist = (float)rs.viewResultsRm[i, j].ResultData.TreeClosestDist,
                            GrassClosestDist = (float)rs.viewResultsRm[i, j].ResultData.GrassClosestDist,
                            WaterClosestDist = (float)rs.viewResultsRm[i, j].ResultData.WaterClosestDist,
                            DynamicClosestDist = (float)rs.viewResultsRm[i, j].ResultData.DynamicClosestDist,
                            SkyCondition = (float)rs.viewResultsRm[i, j].ResultData.SkyCondition,

                        };

                        // Make a single prediction on the sample data and print results
                        var overallRating = ConsumeOverallRating.Predict(sampleDataOverallRating);
                        var viewContent = ConsumeViewContent.Predict(sampleDataViewContent);
                        var viewAccess = ConsumeViewAccess.Predict(sampleDataViewAccess);
                        var privacy = ConsumePrivacy.Predict(sampleDataPrivacy);
                        
                        //max:43259, min: 17892
                        //(rs.viewResultsRm[i, j].ResultData.WindowAreaSum * 5288.02083158) > 17892) && ((rs.viewResultsRm[i, j].ResultData.WindowAreaSum * 5288.02083158) < 43259)
                        if ( rs.viewResultsRm[i,j].ResultData.WindowAreaSum > 0){

                            overallRatings.Add(overallRating.OverallRatingB);
                            viewContents.Add(viewContent.ViewContentB);
                            viewAccesses.Add(viewAccess.ViewAccessB);
                            privacys.Add(privacy.PrivacyB);

                            rs.viewResultsRm[i, j].ResultData.PredictedOverallRating = overallRating.OverallRatingB;
                            rs.viewResultsRm[i, j].ResultData.PredictedViewContent = viewContent.ViewContentB;
                            rs.viewResultsRm[i, j].ResultData.PredictedOverallRating = viewAccess.ViewAccessB;
                            rs.viewResultsRm[i, j].ResultData.PredictedViewContent = privacy.PrivacyB;

                        }
                        else
                        {
                            overallRatings.Add(double.NaN);
                            viewContents.Add(double.NaN);
                            viewAccesses.Add(double.NaN);
                            privacys.Add(double.NaN);

                            rs.viewResultsRm[i, j].ResultData.PredictedOverallRating = double.NaN;
                            rs.viewResultsRm[i, j].ResultData.PredictedViewContent = double.NaN;
                            rs.viewResultsRm[i, j].ResultData.PredictedOverallRating = double.NaN;
                            rs.viewResultsRm[i, j].ResultData.PredictedViewContent = double.NaN;
                        }
                        

                        rs.viewResultsRm[i, j].ResultData.ViewPointX = rs.Pts[i].X;
                        rs.viewResultsRm[i, j].ResultData.ViewPointY = rs.Pts[i].Y;
                        rs.viewResultsRm[i, j].ResultData.ViewPointZ = rs.Pts[i].Z;
                        rs.viewResultsRm[i, j].ResultData.ViewVectorX = rs.Vecs[j].X;
                        rs.viewResultsRm[i, j].ResultData.ViewVectorY = rs.Vecs[j].Y;
                        rs.viewResultsRm[i, j].ResultData.ViewVectorZ = rs.Vecs[j].Z;

                        nodeResult.Add(rs.viewResultsRm[i, j].ResultData, treePath);
                    }
                }
            }

            


            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            
            //writeCSV(path + @"\ResultData.csv", nodeResult.AllData());

            seemo_result.ToFile(path + @"\Result.json");

            DA.SetData(0, path + @"\Result.json");
            DA.SetDataList(1, overallRatings);
            DA.SetDataList(2, viewContents);
            DA.SetDataList(3, viewAccesses);
            DA.SetDataList(4, privacys);

        }
        /*
        public static void writeCSV<T>(string fp, List<T> records)
        {
            if (records.Count == 0) return;
            using (var writer = new StreamWriter(fp))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(records);
            }

        }
        


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("E81FFE58-83E4-41C3-BE48-F98205A365A3"); }
        }
    }
}*/