using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using SeemoPredictor.SeemoGeo;
using System.Threading.Tasks;
using System.Threading;

namespace SeemoPredictor
{
    public class ViewAnalyzerComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ViewAnalyzerComponent_ver2 class.
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

            pManager.AddGenericParameter("Sensors", "S", "View Sensors", GH_ParamAccess.list);
            pManager.AddGenericParameter("Faces", "F", "Seemo Faces", GH_ParamAccess.list);
            //pManager.AddGenericParameter("Windows", "W", "Windows", GH_ParamAccess.list);

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
            pManager.AddGenericParameter("Result", "Res", "Result", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var floorheight = 3;


            //input objects
            List<SmoSensor> sensors = new List<SmoSensor>();
            List<SmoFace> faces = new List<SmoFace>();

            DA.GetDataList(0, sensors);
            DA.GetDataList(1, faces);



            //calculate min, max mode size
            double minNodeSize = double.MaxValue;
            double maxNodeSize = double.MinValue;

            for (int i = 0; i < faces.Count; i++)
            {
                var f = faces[i];
                f.Id = i;
                var size = f.BoundingBox.Size.Length;
                if (minNodeSize > size) minNodeSize = size;
                if (maxNodeSize < size) maxNodeSize = size;
            }

            // make octree
            SmoPointOctree<SmoFace> octree0 = new SmoPointOctree<SmoFace>((float)maxNodeSize, sensors[0].Pt, (float)minNodeSize);
            foreach (SmoFace f in faces)
            {
                octree0.Add(f, f.Center);
            }



            List<double> overallRatings = new List<double>();
            List<double> viewContents = new List<double>();
            List<double> viewAccesses = new List<double>();
            List<double> privacys = new List<double>();



            //output objects
            var seemoResult = new SeemoResult();
            List<SmoSensorWithResults> resultNodes = new List<SmoSensorWithResults>();
            //output test
            List<Point3d> hitsList = new List<Point3d>();
            List<Point3d> raysList = new List<Point3d>();



            


            ////calculating FloorHeights
            //double floorheight;
            //SmoPointOctree<SmoFace> octree1 = new SmoPointOctree<SmoFace>((float)maxNodeSize, input.Pts[0], (float)minNodeSize);
            //foreach (SmoFace f in input.Pavement)
            //    octree1.Add(f, f.Center);
            //foreach (SmoFace f in input.Grass)
            //    octree1.Add(f, f.Center);
            //foreach (SmoFace f in input.Water)
            //    octree1.Add(f, f.Center);




            //compute every view points and directions stored in seemoRoom
            for (int i = 0; i < sensors.Count; i++)
            {
                List<DirectionResult> nodeResult = new List<DirectionResult>();

 
                //define and get object
                SmoSensorWithResults node = new SmoSensorWithResults();
                node.NodeID = i;
                node.Pt = sensors[i].Pt;
                node.Dirs = sensors[i].ViewDirections;


                //SmoPoint3 groundIntersect = new SmoPoint3();
                //var typeG = SmoIntersect.IsObstructed(ref groundIntersect, octree1, node.Pt, new SmoPoint3(0, 0, -1), maxNodeSize);
                //if (typeG == SmoFace.SmoFaceType.Pavement || typeG == SmoFace.SmoFaceType.Grass || typeG == SmoFace.SmoFaceType.Water)
                //{
                //    floorheight = SmoPoint3.Distance(groundIntersect, node.Pt);
                //}
                //else
                //{
                //    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "There is no ground (pavement, grass, or water) vertically under the view point");
                //    floorheight = 0;
                //    return;
                //}





                //generate ray and do machine learning and save data in direction result 
                //original

                for (int j = 0; j < sensors[i].ViewDirections.Length; j++)
                {



                    //generateZoneRay and Define ResultDataSet
                    DirectionResult directionResult = sensors[i].GenerateZoneRay( j);
                    directionResult.ID = ("Point" + i.ToString() + ":" + "Dir" + j.ToString());
                    directionResult.Dir = sensors[i].ViewDirections[j];



                    directionResult.Image = new SmoImage(sensors[i].Pt, sensors[i].ViewDirections[j], sensors[i].Resolution, sensors[i].HorizontalViewAngle, sensors[i].VerticalViewAngle);



                    //Compute octree intersect
                    SmoIntersect.MeshRayResultSave(ref directionResult, octree0, node.Pt, maxNodeSize);


                    //Generate Model input for prediction
                    ModelInput sampleDataOverallRating = new ModelInput()
                    {

                        WindowNumber = (float)directionResult.WindowNumber,
                        WindowAreaSum = (float)directionResult.WindowAreaSum,
                        Z1PtsCountRatio = (float)directionResult.Z1PtsCountRatio,
                        Z2PtCountRatio = (float)directionResult.Z2PtsCountRatio,
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
                        Z2PtCountRatio = (float)directionResult.Z2PtsCountRatio,
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
                        Z2PtCountRatio = (float)directionResult.Z2PtsCountRatio,
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
                        Z2PtCountRatio = (float)directionResult.Z2PtsCountRatio,
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
                    if (directionResult.WindowAreaSum > 0)
                    {

                        overallRatings.Add(overallRating.OverallRatingB);
                        viewContents.Add(viewContent.ViewContentB);
                        viewAccesses.Add(viewAccess.ViewAccessB);
                        privacys.Add(privacy.PrivacyB);

                        directionResult.PredictedOverallRating = overallRatings[overallRatings.Count - 1];
                        directionResult.PredictedViewContent = viewContents[viewContents.Count - 1];
                        directionResult.PredictedViewAccess = viewAccesses[viewAccesses.Count - 1];
                        directionResult.PredictedPrivacy = privacys[privacys.Count - 1];

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


                    directionResult.ViewPointX = sensors[i].Pt.X;
                    directionResult.ViewPointY = sensors[i].Pt.Y;
                    directionResult.ViewPointZ = sensors[i].Pt.Z;
                    directionResult.ViewVectorX = sensors[i].ViewDirections[j].X;
                    directionResult.ViewVectorY = sensors[i].ViewDirections[j].Y;
                    directionResult.ViewVectorZ = sensors[i].ViewDirections[j].Z;


                    ////erase sceneRayVector before exporting JSON
                    //directionResult.sceneRayVectorsZ1.Clear();
                    //directionResult.sceneRayVectorsZ2.Clear();
                    //directionResult.sceneRayVectorsZ3.Clear();
                    //directionResult.sceneRayVectorsZ4.Clear();

                    directionResult.FloorHeights = floorheight;

                    //Save direction result
                    nodeResult.Add(directionResult);
                }

                node.DirectionsResults = nodeResult;
                //Save node result
                resultNodes.Add(node);
            }
            seemoResult.Results = resultNodes;

            //save all results to json file
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            //writeCSV(path + @"\ResultData.csv", nodeResult.AllData());
            seemoResult.ToFile(path + @"\Result.json");

            DA.SetData(0, path + @"\Result.json");
            DA.SetDataList(1, overallRatings);
            DA.SetDataList(2, viewContents);
            DA.SetDataList(3, viewAccesses);
            DA.SetDataList(4, privacys);

            DA.SetData(5, seemoResult);
 
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
            get { return new Guid("E3E704D9-F1FC-4384-95E5-BA5DC4F1594C"); }
        }
    }
}