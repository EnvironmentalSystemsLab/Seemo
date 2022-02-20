using System;
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


            //output objects
            var seemoResult = new SeemoResult();
            List<Node> nodes = new List<Node>();
            List<ResultDataSet> nodeResult = new List<ResultDataSet>();

            List<double> overallRatings = new List<double>();
            List<double> viewContents = new List<double>();
            List<double> viewAccesses = new List<double>();
            List<double> privacys = new List<double>();
            //List<List<Vector3d>> winRayVectorsT = new List<List<Vector3d>>();

            //input objects
            
            List<SeemoRoom> rooms = new List<SeemoRoom>();
            SEnvironment env = new SEnvironment();

            DA.GetDataList(0, rooms);
            DA.GetData(1, ref env);


            for (int i = 0; i < rooms.Count; i++)
            {
                //define and get object
                SeemoRoom sr = rooms[i];
                SeemoOutdoor so = new SeemoOutdoor(sr, env);


                //compute window mesh
                sr.ComputeWindowMesh();


                //compute every view points and directions stored in seemoRoom
                for (int j = 0; j<sr.Pts.Count; j++)
                {
                    //define and get object
                    Node node = new Node();
                    node.RoomID = i;
                    node.Pt = sr.Pts[j];
                    node.Dirs = sr.Vecs;
                    

                    for(int k = 0; k<sr.Vecs.Length; k++)
                    {
                        //define and get object
                        ResultDataSet directionResult = new ResultDataSet();
                        directionResult = sr.ComputeWindowRay(j, k);
                        directionResult.ID = ("Room" + i.ToString() + ":" + "Point" + j.ToString() + ":" + "Dir" + k.ToString());
                        so.ComputeViewMeshray(j, k, directionResult);



                        ModelInput sampleDataOverallRating = new ModelInput()
                        {

                            WindowNumber = (float)directionResult.WindowNumber,
                            WindowAreaSum = (float)directionResult.WindowAreaSum,
                            Z1PtsCountRatio = (float)directionResult.Z1PtsCountRatio,
                            Z2PtCountRatio = (float)directionResult.Z2PtCountRatio,
                            Z3PtsCountRatio = (float)directionResult.Z3PtsCountRatio,
                            Z4PtsCountRatio = (float)directionResult.Z4PtsCountRatio,
                            BuildingPtsCountRatio = (float)directionResult.BuildingPtsCountRatio,
                            EquipmentPtsCountRatio = (float)directionResult.EquipmentPtsCountRatio,
                            TreePtsCountRatio = (float)directionResult.TreePtsCountRatio,
                            PavementPtsCountRatio = (float)directionResult.PavementPtsCountRatio,
                            GrassPtsCountRatio = (float)directionResult.GrassPtsCountRatio,
                            WaterPtsCountRatio = (float)directionResult.WaterPtsCountRatio,
                            DynamicPtsRatio = (float)directionResult.DynamicPtsRatio,
                            SkyPtsCountRatio = (float)directionResult.SkyPtsCountRatio,
                            ElementNumber = (float)directionResult.ElementNumber,
                            FloorHeights = (float)directionResult.FloorHeights,
                            BuildingClosestDist = (float)directionResult.BuildingClosestDist,
                            EquipmentClosestDist = (float)directionResult.EquipmentClosestDist,
                            TreeClosestDist = (float)directionResult.TreeClosestDist,
                            GrassClosestDist = (float)directionResult.GrassClosestDist,
                            WaterClosestDist = (float)directionResult.WaterClosestDist,
                            DynamicClosestDist = (float)directionResult.DynamicClosestDist,
                            SkyCondition = (float)directionResult.SkyCondition,

                        };

                        ModelInputViewContent sampleDataViewContent = new ModelInputViewContent()
                        {

                            WindowNumber = (float)directionResult.WindowNumber,
                            WindowAreaSum = (float)directionResult.WindowAreaSum,
                            Z1PtsCountRatio = (float)directionResult.Z1PtsCountRatio,
                            Z2PtCountRatio = (float)directionResult.Z2PtCountRatio,
                            Z3PtsCountRatio = (float)directionResult.Z3PtsCountRatio,
                            Z4PtsCountRatio = (float)directionResult.Z4PtsCountRatio,
                            BuildingPtsCountRatio = (float)directionResult.BuildingPtsCountRatio,
                            EquipmentPtsCountRatio = (float)directionResult.EquipmentPtsCountRatio,
                            TreePtsCountRatio = (float)directionResult.TreePtsCountRatio,
                            PavementPtsCountRatio = (float)directionResult.PavementPtsCountRatio,
                            GrassPtsCountRatio = (float)directionResult.GrassPtsCountRatio,
                            WaterPtsCountRatio = (float)directionResult.WaterPtsCountRatio,
                            DynamicPtsRatio = (float)directionResult.DynamicPtsRatio,
                            SkyPtsCountRatio = (float)directionResult.SkyPtsCountRatio,
                            ElementNumber = (float)directionResult.ElementNumber,
                            FloorHeights = (float)directionResult.FloorHeights,
                            BuildingClosestDist = (float)directionResult.BuildingClosestDist,
                            EquipmentClosestDist = (float)directionResult.EquipmentClosestDist,
                            TreeClosestDist = (float)directionResult.TreeClosestDist,
                            GrassClosestDist = (float)directionResult.GrassClosestDist,
                            WaterClosestDist = (float)directionResult.WaterClosestDist,
                            DynamicClosestDist = (float)directionResult.DynamicClosestDist,
                            SkyCondition = (float)directionResult.SkyCondition,

                        };
                        ModelInputViewAccess sampleDataViewAccess = new ModelInputViewAccess()
                        {

                            WindowNumber = (float)directionResult.WindowNumber,
                            WindowAreaSum = (float)directionResult.WindowAreaSum,
                            Z1PtsCountRatio = (float)directionResult.Z1PtsCountRatio,
                            Z2PtCountRatio = (float)directionResult.Z2PtCountRatio,
                            Z3PtsCountRatio = (float)directionResult.Z3PtsCountRatio,
                            Z4PtsCountRatio = (float)directionResult.Z4PtsCountRatio,
                            BuildingPtsCountRatio = (float)directionResult.BuildingPtsCountRatio,
                            EquipmentPtsCountRatio = (float)directionResult.EquipmentPtsCountRatio,
                            TreePtsCountRatio = (float)directionResult.TreePtsCountRatio,
                            PavementPtsCountRatio = (float)directionResult.PavementPtsCountRatio,
                            GrassPtsCountRatio = (float)directionResult.GrassPtsCountRatio,
                            WaterPtsCountRatio = (float)directionResult.WaterPtsCountRatio,
                            DynamicPtsRatio = (float)directionResult.DynamicPtsRatio,
                            SkyPtsCountRatio = (float)directionResult.SkyPtsCountRatio,
                            ElementNumber = (float)directionResult.ElementNumber,
                            FloorHeights = (float)directionResult.FloorHeights,
                            BuildingClosestDist = (float)directionResult.BuildingClosestDist,
                            EquipmentClosestDist = (float)directionResult.EquipmentClosestDist,
                            TreeClosestDist = (float)directionResult.TreeClosestDist,
                            GrassClosestDist = (float)directionResult.GrassClosestDist,
                            WaterClosestDist = (float)directionResult.WaterClosestDist,
                            DynamicClosestDist = (float)directionResult.DynamicClosestDist,
                            SkyCondition = (float)directionResult.SkyCondition,

                        };

                        ModelInputPrivacy sampleDataPrivacy = new ModelInputPrivacy()
                        {

                            WindowNumber = (float)directionResult.WindowNumber,
                            WindowAreaSum = (float)directionResult.WindowAreaSum,
                            Z1PtsCountRatio = (float)directionResult.Z1PtsCountRatio,
                            Z2PtCountRatio = (float)directionResult.Z2PtCountRatio,
                            Z3PtsCountRatio = (float)directionResult.Z3PtsCountRatio,
                            Z4PtsCountRatio = (float)directionResult.Z4PtsCountRatio,
                            BuildingPtsCountRatio = (float)directionResult.BuildingPtsCountRatio,
                            EquipmentPtsCountRatio = (float)directionResult.EquipmentPtsCountRatio,
                            TreePtsCountRatio = (float)directionResult.TreePtsCountRatio,
                            PavementPtsCountRatio = (float)directionResult.PavementPtsCountRatio,
                            GrassPtsCountRatio = (float)directionResult.GrassPtsCountRatio,
                            WaterPtsCountRatio = (float)directionResult.WaterPtsCountRatio,
                            DynamicPtsRatio = (float)directionResult.DynamicPtsRatio,
                            SkyPtsCountRatio = (float)directionResult.SkyPtsCountRatio,
                            ElementNumber = (float)directionResult.ElementNumber,
                            FloorHeights = (float)directionResult.FloorHeights,
                            BuildingClosestDist = (float)directionResult.BuildingClosestDist,
                            EquipmentClosestDist = (float)directionResult.EquipmentClosestDist,
                            TreeClosestDist = (float)directionResult.TreeClosestDist,
                            GrassClosestDist = (float)directionResult.GrassClosestDist,
                            WaterClosestDist = (float)directionResult.WaterClosestDist,
                            DynamicClosestDist = (float)directionResult.DynamicClosestDist,
                            SkyCondition = (float)directionResult.SkyCondition,

                        };

                        // Make a single prediction on the sample data and print results
                        var overallRating = ConsumeOverallRating.Predict(sampleDataOverallRating);
                        var viewContent = ConsumeViewContent.Predict(sampleDataViewContent);
                        var viewAccess = ConsumeViewAccess.Predict(sampleDataViewAccess);
                        var privacy = ConsumePrivacy.Predict(sampleDataPrivacy);
                        
                        //max:43259, min: 17892
                        //(directionResult.WindowAreaSum * 5288.02083158) > 17892) && ((directionResult.WindowAreaSum * 5288.02083158) < 43259)
                        if ( directionResult.WindowAreaSum > 0){

                            overallRatings.Add(overallRating.OverallRatingB);
                            viewContents.Add(viewContent.ViewContentB);
                            viewAccesses.Add(viewAccess.ViewAccessB);
                            privacys.Add(privacy.PrivacyB);

                            directionResult.PredictedOverallRating = overallRatings[k];
                            directionResult.PredictedViewContent = viewContents[k];
                            directionResult.PredictedViewAccess = viewAccesses[k];
                            directionResult.PredictedPrivacy = privacys[k];


                            //directionResult.PredictedOverallRating = overallRating.OverallRatingB;
                            //directionResult.PredictedViewContent = viewContent.ViewContentB;
                            //directionResult.PredictedOverallRating = viewAccess.ViewAccessB;
                            //directionResult.PredictedViewContent = privacy.PrivacyB;

                        }
                        else
                        {
                            overallRatings.Add(double.NaN);
                            viewContents.Add(double.NaN);
                            viewAccesses.Add(double.NaN);
                            privacys.Add(double.NaN);

                            directionResult.PredictedOverallRating = double.NaN;
                            directionResult.PredictedViewContent = double.NaN;
                            directionResult.PredictedOverallRating = double.NaN;
                            directionResult.PredictedViewContent = double.NaN;
                        }
                        

                        directionResult.ViewPointX = sr.Pts[j].X;
                        directionResult.ViewPointY = sr.Pts[j].Y;
                        directionResult.ViewPointZ = sr.Pts[j].Z;
                        directionResult.ViewVectorX = sr.Vecs[k].X;
                        directionResult.ViewVectorY = sr.Vecs[k].Y;
                        directionResult.ViewVectorZ = sr.Vecs[k].Z;


                        //Save direction result
                        node.DirectionsResults.Add(directionResult);
                    }
                    //Save node result
                    nodes.Add(node);
                }
            }

            seemoResult.Results = nodes;

            //save all results to json file
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            //writeCSV(path + @"\ResultData.csv", nodeResult.AllData());
            seemoResult.ToFile(path + @"\Result.json");

            DA.SetData(0, path + @"\Result.json");
            DA.SetDataList(1, overallRatings);
            DA.SetDataList(2, viewContents);
            DA.SetDataList(3, viewAccesses);
            DA.SetDataList(4, privacys);


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
}