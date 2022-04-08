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
            pManager.AddNumberParameter("Ground Level", "GL", "Z coordinate of Ground Level", GH_ParamAccess.item);
            //pManager.AddGenericParameter("Windows", "W", "Windows", GH_ParamAccess.list);

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
            List<SmoSensor> sensors = new List<SmoSensor>();
            List<SmoFace> faces = new List<SmoFace>();
            double glevel = 0;

            DA.GetDataList(0, sensors);
            DA.GetDataList(1, faces);
            if (!DA.GetData(2, ref glevel));


            //calculate min, max mode size
            double avNodeSize = 0;
            double minNodeSize = double.MaxValue;
            double maxNodeSize = double.MinValue;
            BBox worldbounds = new BBox();

            for (int i = 0; i < faces.Count; i++)
            {
                var f = faces[i];
                f.Id = i;
                var size = f.BoundingBox.Size.Length;
                if (minNodeSize > size) minNodeSize = size;
                if (maxNodeSize < size) maxNodeSize = size;
                avNodeSize += size;
                worldbounds.Encapsulate(f.BoundingBox);
            }
            avNodeSize /= faces.Count;
            // make octree for visibility
            float worldSize = (float)worldbounds.Size.Length * 0.8f;
            PointOctree<SmoFace> octree0 = new PointOctree<SmoFace>(worldSize, worldbounds.Center, (float)(avNodeSize));
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

            StringBuilder report = new StringBuilder();
            Stopwatch sp = new Stopwatch();
            sp.Start();

            // -------------------------
            // setup raycasting worklist
            // -------------------------

            List<SmoImage> images = new List<SmoImage>();

            for (int i = 0; i < sensors.Count; i++)
            {
                for (int j = 0; j < sensors[i].ViewDirections.Length; j++)
                    {
                        var image = new SmoImage(sensors[i].Pt, sensors[i].ViewDirections[j], sensors[i].Resolution, sensors[i].HorizontalViewAngle, sensors[i].VerticalViewAngle);
                        images.Add(image);
                    }
                
            }

            report.AppendLine("Setup raycasting worklist: " + sp.ElapsedMilliseconds +"[ms]");
            sp.Restart();

            // -------------------------
            // raycasting process
            // -------------------------

            SmoImage[] imageArray;
            imageArray = images.ToArray();

            //    for (int i = 0; i < imageArray.Length; i++)
            Parallel.For(0, imageArray.Length, i =>
            {
                imageArray[i].ComputeImage(octree0, maxNodeSize);
            }); // Parallel.For
            
            report.AppendLine("Computing view images: " + sp.ElapsedMilliseconds  + "[ms]");
            sp.Restart();




            // -------------------------
            // execute ML model and create result output classes
            // -------------------------
            int imgIndex = 0;
            for (int i = 0; i < sensors.Count; i++)
            {
                List<DirectionResult> nodeResult = new List<DirectionResult>();
                SmoSensorWithResults node = new SmoSensorWithResults();
                node.NodeID = i;
                node.Pt = sensors[i].Pt;
                node.Dirs = sensors[i].ViewDirections;


                for (int j = 0; j < sensors[i].ViewDirections.Length; j++)
                {
                    DirectionResult directionResult = new DirectionResult();
                    directionResult.ID = ("Point" + i.ToString() + ":" + "Dir" + j.ToString());
                    directionResult.Dir = sensors[i].ViewDirections[j];

                    directionResult.ViewPointX = sensors[i].Pt.X;
                    directionResult.ViewPointY = sensors[i].Pt.Y;
                    directionResult.ViewPointZ = sensors[i].Pt.Z;
                    directionResult.ViewVectorX = sensors[i].ViewDirections[j].X;
                    directionResult.ViewVectorY = sensors[i].ViewDirections[j].Y;
                    directionResult.ViewVectorZ = sensors[i].ViewDirections[j].Z;



                    directionResult.Image = imageArray[imgIndex];
                    imgIndex++;


                    //directionResult.Image = new SmoImage(sensors[i].Pt, sensors[i].ViewDirections[j], sensors[i].Resolution, sensors[i].HorizontalViewAngle, sensors[i].VerticalViewAngle);
                    //directionResult.Image.ComputeImage(octree0, maxNodeSize);




                    // compute the ML model inputs from the SmoImage class here
                    directionResult.ComputeFeatures();

                    directionResult.FloorHeights = (directionResult.ViewPointZ - glevel);



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
                        PavementPtsCountRatio = (float)directionResult.PavementPtsCountRatio,
                        GrassPtsCountRatio = (float)directionResult.GrassPtsCountRatio,
                        WaterPtsCountRatio = (float)directionResult.WaterPtsCountRatio,
                        DynamicPtsRatio = (float)directionResult.PeoplePtsCountRatio,
                        SkyPtsCountRatio = (float)directionResult.SkyPtsCountRatio,
                        ElementNumber = (float)directionResult.ElementNumber,
                        FloorHeights = (float)directionResult.FloorHeights,
                        BuildingClosestDist = (float)directionResult.BuildingClosestDist,
                        EquipmentClosestDist = (float)directionResult.EquipmentClosestDist,
                        TreeClosestDist = (float)directionResult.TreeClosestDist,
                        GrassClosestDist = (float)directionResult.GrassClosestDist,
                        WaterClosestDist = (float)directionResult.WaterClosestDist,
                        DynamicClosestDist = (float)directionResult.PeopleClosestDist,
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
                        PavementPtsCountRatio = (float)directionResult.PavementPtsCountRatio,
                        GrassPtsCountRatio = (float)directionResult.GrassPtsCountRatio,
                        WaterPtsCountRatio = (float)directionResult.WaterPtsCountRatio,
                        DynamicPtsRatio = (float)directionResult.PeoplePtsCountRatio,
                        SkyPtsCountRatio = (float)directionResult.SkyPtsCountRatio,
                        ElementNumber = (float)directionResult.ElementNumber,
                        FloorHeights = (float)directionResult.FloorHeights,
                        BuildingClosestDist = (float)directionResult.BuildingClosestDist,
                        EquipmentClosestDist = (float)directionResult.EquipmentClosestDist,
                        TreeClosestDist = (float)directionResult.TreeClosestDist,
                        GrassClosestDist = (float)directionResult.GrassClosestDist,
                        WaterClosestDist = (float)directionResult.WaterClosestDist,
                        DynamicClosestDist = (float)directionResult.PeopleClosestDist,
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
                        PavementPtsCountRatio = (float)directionResult.PavementPtsCountRatio,
                        GrassPtsCountRatio = (float)directionResult.GrassPtsCountRatio,
                        WaterPtsCountRatio = (float)directionResult.WaterPtsCountRatio,
                        DynamicPtsRatio = (float)directionResult.PeoplePtsCountRatio,
                        SkyPtsCountRatio = (float)directionResult.SkyPtsCountRatio,
                        ElementNumber = (float)directionResult.ElementNumber,
                        FloorHeights = (float)directionResult.FloorHeights,
                        BuildingClosestDist = (float)directionResult.BuildingClosestDist,
                        EquipmentClosestDist = (float)directionResult.EquipmentClosestDist,
                        TreeClosestDist = (float)directionResult.TreeClosestDist,
                        GrassClosestDist = (float)directionResult.GrassClosestDist,
                        WaterClosestDist = (float)directionResult.WaterClosestDist,
                        DynamicClosestDist = (float)directionResult.PeopleClosestDist,
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
                        PavementPtsCountRatio = (float)directionResult.PavementPtsCountRatio,
                        GrassPtsCountRatio = (float)directionResult.GrassPtsCountRatio,
                        WaterPtsCountRatio = (float)directionResult.WaterPtsCountRatio,
                        DynamicPtsRatio = (float)directionResult.PeoplePtsCountRatio,
                        SkyPtsCountRatio = (float)directionResult.SkyPtsCountRatio,
                        ElementNumber = (float)directionResult.ElementNumber,
                        FloorHeights = (float)directionResult.FloorHeights,
                        BuildingClosestDist = (float)directionResult.BuildingClosestDist,
                        EquipmentClosestDist = (float)directionResult.EquipmentClosestDist,
                        TreeClosestDist = (float)directionResult.TreeClosestDist,
                        GrassClosestDist = (float)directionResult.GrassClosestDist,
                        WaterClosestDist = (float)directionResult.WaterClosestDist,
                        DynamicClosestDist = (float)directionResult.PeopleClosestDist,
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



                    //Save direction result
                    nodeResult.Add(directionResult);
                }

                node.DirectionsResults = nodeResult;
                //Save node result
                resultNodes.Add(node);
            }
            seemoResult.Results = resultNodes;



            report.AppendLine("Computing predictions: " + sp.ElapsedMilliseconds  + "[ms]");
            sp.Restart();


            // -------------------------
            //save all results to json file
            // -------------------------
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            string dir =(path + @"\Seemo");

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            long time = DateTime.Now.ToFileTime();
            seemoResult.ToFile(dir + @"\Result"+ time+".json");



            report.AppendLine("Saving result: " + sp.ElapsedMilliseconds  + "[ms]");
            sp.Restart();








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
            get { return new Guid("E3E704D9-F1FC-4384-95E5-BA5DC4F1594C"); }
        }
    }
}