using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using SeemoPredictor.Geometry;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Text;
using System.IO;

namespace SeemoPredictor.Components
{
    public class PredictorComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the PredictorComponent class.
        /// </summary>
        public PredictorComponent()
          : base("Predictor", "Predictor",
              "Predictor",
              "SeEmo", "3|Analyzer")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Run", "Run", "Run", GH_ParamAccess.item);
            pManager.AddGenericParameter("RT_Result", "RT", "RT_Result", GH_ParamAccess.item);
            pManager.AddNumberParameter("Ground Level", "GL", "Z coordinate of Ground Level", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Report", "Out", "Console log of simulation", GH_ParamAccess.item);
            pManager.AddGenericParameter("Result", "Res", "Result", GH_ParamAccess.item);
            pManager.AddTextParameter("Result File Path", "File", "Result File Path", GH_ParamAccess.item);
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
            //input objects
            var seemoResult = new SeemoResult();
            double glevel = 0;
            Boolean run = false;

            if (!DA.GetData(0, ref run)) return;
            if (run == false) return;

            if (!DA.GetData(1, ref seemoResult)) return;


            List<double> overallRatings = new List<double>();
            List<double> viewContents = new List<double>();
            List<double> viewAccesses = new List<double>();
            List<double> privacys = new List<double>();


            //output objects
            
            List<SmoSensorWithResults> resultNodes = new List<SmoSensorWithResults>();

            StringBuilder report = new StringBuilder();
            Stopwatch sp = new Stopwatch();
            report.AppendLine("Computing view images: " + sp.ElapsedMilliseconds + "[ms]");
            sp.Start();

            

            // -------------------------
            // execute ML model and create result output classes
            // -------------------------
            for (int i = 0; i < seemoResult.Results.Count; i++) //results = list<sensors> = list<node
            {
                List<DirectionResult> nodeResult = new List<DirectionResult>();
                SmoSensorWithResults node = new SmoSensorWithResults();
                

                for (int j = 0; j < seemoResult.Results[i].DirectionsResults.Count; j++)
                {
                    // compute the ML model inputs from the SmoImage class here
                    seemoResult.Results[i].DirectionsResults[j].ComputeFeatures();

                    DirectionResult directionResult = seemoResult.Results[i].DirectionsResults[j];
                    
                    seemoResult.Results[i].DirectionsResults[j].FloorHeights = (directionResult.ViewPointZ - glevel);


                    //Generate Model input for prediction
                    ModelInput sampleDataOverallRating = new ModelInput()
                    {

                        WindowNumber = (float)2.0,
                        WindowAreaSum = (float)directionResult.WindowAreaSum,
                        Z1PtsCountRatio = (float)directionResult.Z1PtsCountRatio,
                        Z2PtCountRatio = (float)directionResult.Z2PtsCountRatio,
                        Z3PtsCountRatio = (float)directionResult.Z3PtsCountRatio,
                        Z4PtsCountRatio = (float)directionResult.Z4PtsCountRatio,
                        BuildingPtsCountRatio = (float)directionResult.BuildingPtsCountRatio,
                        EquipmentPtsCountRatio = (float)directionResult.EquipmentPtsCountRatio,
                        TreePtsCountRatio = (float)directionResult.TreePtsCountRatio,
                        PavementPtsCountRatio = (float)directionResult.RoadPtsCountRatio,
                        GrassPtsCountRatio = (float)directionResult.GrassPtsCountRatio,
                        WaterPtsCountRatio = (float)directionResult.WaterPtsCountRatio,
                        DynamicPtsRatio = (float)directionResult.SidewalkPtsCountRatio,
                        SkyPtsCountRatio = (float)directionResult.SkyPtsCountRatio,
                        ElementNumber = (float)directionResult.ElementNumber,
                        FloorHeights = (float)directionResult.FloorHeights,
                        BuildingClosestDist = (float)directionResult.BuildingClosestDist,
                        EquipmentClosestDist = (float)directionResult.EquipmentClosestDist,
                        TreeClosestDist = (float)directionResult.TreeClosestDist,
                        GrassClosestDist = (float)directionResult.GrassClosestDist,
                        WaterClosestDist = (float)directionResult.WaterClosestDist,
                        DynamicClosestDist = (float)directionResult.SidewalkClosestDist,
                        SkyCondition = (float)directionResult.SkyCondition,

                    };
                    ModelInputViewContent sampleDataViewContent = new ModelInputViewContent()
                    {

                        WindowNumber = (float)2.0,
                        WindowAreaSum = (float)directionResult.WindowAreaSum,
                        Z1PtsCountRatio = (float)directionResult.Z1PtsCountRatio,
                        Z2PtCountRatio = (float)directionResult.Z2PtsCountRatio,
                        Z3PtsCountRatio = (float)directionResult.Z3PtsCountRatio,
                        Z4PtsCountRatio = (float)directionResult.Z4PtsCountRatio,
                        BuildingPtsCountRatio = (float)directionResult.BuildingPtsCountRatio,
                        EquipmentPtsCountRatio = (float)directionResult.EquipmentPtsCountRatio,
                        TreePtsCountRatio = (float)directionResult.TreePtsCountRatio,
                        PavementPtsCountRatio = (float)directionResult.RoadPtsCountRatio,
                        GrassPtsCountRatio = (float)directionResult.GrassPtsCountRatio,
                        WaterPtsCountRatio = (float)directionResult.WaterPtsCountRatio,
                        DynamicPtsRatio = (float)directionResult.SidewalkPtsCountRatio,
                        SkyPtsCountRatio = (float)directionResult.SkyPtsCountRatio,
                        ElementNumber = (float)directionResult.ElementNumber,
                        FloorHeights = (float)directionResult.FloorHeights,
                        BuildingClosestDist = (float)directionResult.BuildingClosestDist,
                        EquipmentClosestDist = (float)directionResult.EquipmentClosestDist,
                        TreeClosestDist = (float)directionResult.TreeClosestDist,
                        GrassClosestDist = (float)directionResult.GrassClosestDist,
                        WaterClosestDist = (float)directionResult.WaterClosestDist,
                        DynamicClosestDist = (float)directionResult.SidewalkClosestDist,
                        SkyCondition = (float)directionResult.SkyCondition,

                    };
                    ModelInputViewAccess sampleDataViewAccess = new ModelInputViewAccess()
                    {

                        WindowNumber = (float)2.0,
                        WindowAreaSum = (float)directionResult.WindowAreaSum,
                        Z1PtsCountRatio = (float)directionResult.Z1PtsCountRatio,
                        Z2PtCountRatio = (float)directionResult.Z2PtsCountRatio,
                        Z3PtsCountRatio = (float)directionResult.Z3PtsCountRatio,
                        Z4PtsCountRatio = (float)directionResult.Z4PtsCountRatio,
                        BuildingPtsCountRatio = (float)directionResult.BuildingPtsCountRatio,
                        EquipmentPtsCountRatio = (float)directionResult.EquipmentPtsCountRatio,
                        TreePtsCountRatio = (float)directionResult.TreePtsCountRatio,
                        PavementPtsCountRatio = (float)directionResult.RoadPtsCountRatio,
                        GrassPtsCountRatio = (float)directionResult.GrassPtsCountRatio,
                        WaterPtsCountRatio = (float)directionResult.WaterPtsCountRatio,
                        DynamicPtsRatio = (float)directionResult.SidewalkPtsCountRatio,
                        SkyPtsCountRatio = (float)directionResult.SkyPtsCountRatio,
                        ElementNumber = (float)directionResult.ElementNumber,
                        FloorHeights = (float)directionResult.FloorHeights,
                        BuildingClosestDist = (float)directionResult.BuildingClosestDist,
                        EquipmentClosestDist = (float)directionResult.EquipmentClosestDist,
                        TreeClosestDist = (float)directionResult.TreeClosestDist,
                        GrassClosestDist = (float)directionResult.GrassClosestDist,
                        WaterClosestDist = (float)directionResult.WaterClosestDist,
                        DynamicClosestDist = (float)directionResult.SidewalkClosestDist,
                        SkyCondition = (float)directionResult.SkyCondition,

                    };
                    ModelInputPrivacy sampleDataPrivacy = new ModelInputPrivacy()
                    {

                        WindowNumber = (float)2.0,
                        WindowAreaSum = (float)directionResult.WindowAreaSum,
                        Z1PtsCountRatio = (float)directionResult.Z1PtsCountRatio,
                        Z2PtCountRatio = (float)directionResult.Z2PtsCountRatio,
                        Z3PtsCountRatio = (float)directionResult.Z3PtsCountRatio,
                        Z4PtsCountRatio = (float)directionResult.Z4PtsCountRatio,
                        BuildingPtsCountRatio = (float)directionResult.BuildingPtsCountRatio,
                        EquipmentPtsCountRatio = (float)directionResult.EquipmentPtsCountRatio,
                        TreePtsCountRatio = (float)directionResult.TreePtsCountRatio,
                        PavementPtsCountRatio = (float)directionResult.RoadPtsCountRatio,
                        GrassPtsCountRatio = (float)directionResult.GrassPtsCountRatio,
                        WaterPtsCountRatio = (float)directionResult.WaterPtsCountRatio,
                        DynamicPtsRatio = (float)directionResult.SidewalkPtsCountRatio,
                        SkyPtsCountRatio = (float)directionResult.SkyPtsCountRatio,
                        ElementNumber = (float)directionResult.ElementNumber,
                        FloorHeights = (float)directionResult.FloorHeights,
                        BuildingClosestDist = (float)directionResult.BuildingClosestDist,
                        EquipmentClosestDist = (float)directionResult.EquipmentClosestDist,
                        TreeClosestDist = (float)directionResult.TreeClosestDist,
                        GrassClosestDist = (float)directionResult.GrassClosestDist,
                        WaterClosestDist = (float)directionResult.WaterClosestDist,
                        DynamicClosestDist = (float)directionResult.SidewalkClosestDist,
                        SkyCondition = (float)directionResult.SkyCondition,

                    };



                    //max:43259, min: 17892
                    //(directionResult.WindowAreaSum * 5288.02083158) > 17892) && ((directionResult.WindowAreaSum * 5288.02083158) < 43259)
                    if (directionResult.WindowAreaSum > 0)
                    {
                        // Make a single prediction on the sample data and print results
                        var overallRating = ConsumeOverallRating.Predict(sampleDataOverallRating);
                        var viewContent = ConsumeViewContent.Predict(sampleDataViewContent);
                        var viewAccess = ConsumeViewAccess.Predict(sampleDataViewAccess);
                        var privacy = ConsumePrivacy.Predict(sampleDataPrivacy);

                        overallRatings.Add(overallRating.OverallRatingB);
                        viewContents.Add(viewContent.ViewContentB);
                        viewAccesses.Add(viewAccess.ViewAccessB);
                        privacys.Add(privacy.PrivacyB);

                        seemoResult.Results[i].DirectionsResults[j].PredictedOverallRating = overallRatings[overallRatings.Count - 1];
                        seemoResult.Results[i].DirectionsResults[j].PredictedViewContent = viewContents[viewContents.Count - 1];
                        seemoResult.Results[i].DirectionsResults[j].PredictedViewAccess = viewAccesses[viewAccesses.Count - 1];
                        seemoResult.Results[i].DirectionsResults[j].PredictedPrivacy = privacys[privacys.Count - 1];

                    }
                    else
                    {
                        overallRatings.Add(double.NaN);
                        viewContents.Add(double.NaN);
                        viewAccesses.Add(double.NaN);
                        privacys.Add(double.NaN);

                        seemoResult.Results[i].DirectionsResults[j].PredictedOverallRating = double.NaN;
                        seemoResult.Results[i].DirectionsResults[j].PredictedViewContent = double.NaN;
                        seemoResult.Results[i].DirectionsResults[j].PredictedOverallRating = double.NaN;
                        seemoResult.Results[i].DirectionsResults[j].PredictedViewContent = double.NaN;
                    }
                }
            }
            

            report.AppendLine("Computing predictions: " + sp.ElapsedMilliseconds + "[ms]");
            sp.Restart();


            // -------------------------
            //save all results to json file
            // -------------------------
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            string dir = (path + @"\Seemo");

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            long time = DateTime.Now.ToFileTime();
            seemoResult.ToFile(dir + @"\Result" + time + ".json");


            report.AppendLine("Saving result: " + sp.ElapsedMilliseconds + "[ms]");
            sp.Restart();

            //Debug.report.WriteLine("Saving result: " + sp.ElapsedMilliseconds + "[ms]");


            DA.SetData(0, report.ToString());
            DA.SetData(1, seemoResult);
            DA.SetData(2, dir + @"\Result" + time + ".json");


            DA.SetDataList(3, overallRatings);
            DA.SetDataList(4, viewContents);
            DA.SetDataList(5, viewAccesses);
            DA.SetDataList(6, privacys);

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
            get { return new Guid("7B6BB83C-0A13-44DC-B465-96FD58E8A420"); }
        }
    }
}