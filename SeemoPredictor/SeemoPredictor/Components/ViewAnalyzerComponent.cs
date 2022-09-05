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

            //raytracing with cpu + predictor
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {

            pManager.AddBooleanParameter("Run", "Run", "Run", GH_ParamAccess.item);
            pManager.AddGenericParameter("Sensors", "S", "View Sensors", GH_ParamAccess.list);
            pManager.AddGenericParameter("Faces", "F", "Seemo Faces", GH_ParamAccess.list);
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
            pManager.AddNumberParameter("Frameworks", "Frameworks", "Frameworks View Content", GH_ParamAccess.list);
            pManager.AddNumberParameter("SPVEI", "SPVEI", "Seemo Potential Visual Exposure Index", GH_ParamAccess.list);

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
            Boolean run = false;

            if (!DA.GetData(0, ref run))  return; 
            if (run == false)  return; 

            DA.GetDataList(1, sensors);
            DA.GetDataList(2, faces);
            if (!DA.GetData(3, ref glevel))  return; 


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
            PointOctree<SmoFace> octreeEnv = new PointOctree<SmoFace>(worldSize, worldbounds.Center, (float)(avNodeSize)); //includes interior + environment
            
            foreach (SmoFace f in faces)
            {
                octreeEnv.Add(f, f.Center);
            }


            List<double> overallRatings = new List<double>();
            List<double> viewContents = new List<double>();
            List<double> viewAccesses = new List<double>();
            List<double> privacys = new List<double>();
            List<double> frameworks = new List<double>();
            List<double> SPVEIs = new List<double>();


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
            List<SmoImage> splitImages = new List<SmoImage>();
            //

            bool singleDirection = false; //every sensors have the same viewdirections.
            if (sensors[0].ViewDirections.Length == 1 && sensors[0].ViewDirections[0].Z == 0)
            {
                singleDirection = true;
            }

            List<SmoImage> sphericalImages = new List<SmoImage>();

            for (int i = 0; i < sensors.Count; i++)
            {
                if(singleDirection)
                {
                    for (int j = 0; j < sensors[i].ViewDirections.Length; j++)
                    {
                        var image = new SmoImage(sensors[i].Pt, sensors[i].ViewDirections[j], sensors[i].Resolution, sensors[i].HorizontalViewAngle, sensors[i].VerticalViewAngle);
                        images.Add(image);
                    }
                }
                else
                {
                //calculate spherical image
                var sphereImage = new SmoImage(sensors[i].Pt, new Point3(0,1,0), sensors[i].Resolution, 360, sensors[i].VerticalViewAngle);
                sphericalImages.Add(sphereImage);
                }
                
            }

            report.AppendLine("Setup raycasting worklist: " + sp.ElapsedMilliseconds +"[ms]");
            sp.Restart();

            // -------------------------
            // raycasting process
            // -------------------------

            SmoImage[] imageArray;
            if (singleDirection)  
            {
                imageArray = images.ToArray();

                //    for (int i = 0; i < imageArray.Length; i++)
                Parallel.For(0, imageArray.Length, i =>
                {
                    imageArray[i].ComputeImage(octreeEnv, maxNodeSize);
                }); // Parallel.For
            }
            else
            {
                SmoImage[] sphericalImageArray = sphericalImages.ToArray();

                //    for (int i = 0; i < imageArray.Length; i++)
                Parallel.For(0, sphericalImageArray.Length, i =>
                {
                    sphericalImageArray[i].ComputeImage(octreeEnv, maxNodeSize);
                }); // Parallel.For

                //spliting images
                for (int i = 0; i < sensors.Count; i++)
                {
                    for (int j = 0; j < sensors[i].ViewDirections.Length; j++)
                    {
                        Point3 p = sensors[i].ViewDirections[j];
                        //divide image for each direction
                        var splitImage = SmoImage.FrameImages(sphericalImageArray[i], sensors[i].ViewDirections[j], sensors[i].HorizontalViewAngle, sensors[i].VerticalViewAngle);
                        splitImages.Add(splitImage);
                    }
                    
                }

                imageArray = splitImages.ToArray();

            }

            
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



                    // compute the ML model inputs from the SmoImage class here
                    directionResult.ComputeFeatures();

                    directionResult.FloorHeights = (directionResult.ViewPointZ - glevel);


                    //Generate Model input for prediction
                    ModelInput sampleDataOverallRating = new ModelInput()
                    {

                        WindowAreaSum = (float)directionResult.WindowAreaRatio,
                        Z1PtsCountRatio = (float)directionResult.Z1PtsCountRatio,
                        Z2PtsCountRatio = (float)directionResult.Z2PtsCountRatio,
                        Z3PtsCountRatio = (float)directionResult.Z3PtsCountRatio,
                        Z4PtsCountRatio = (float)directionResult.Z4PtsCountRatio,
                        InteriorPtsCountRatio = (float)directionResult.InteriorPtsCountRatio,
                        BuildingPtsCountRatio = (float)directionResult.BuildingPtsCountRatio,
                        ContextWindowPtsCountRatio = (float)directionResult.ContextWindowPtsCountRatio,
                        EquipmentPtsCountRatio = (float)directionResult.EquipmentPtsCountRatio,
                        LandmarkPtsCountRatio = (float)directionResult.LandmarkPtsCountRatio,
                        SidewalkPtsCountRatio = (float)directionResult.SidewalkPtsCountRatio,
                        RoadPtsCountRatio = (float)directionResult.RoadPtsCountRatio,
                        ParkingLotPtsCountRatio = (float)directionResult.ParkingLotPtsCountRatio,
                        TreePtsCountRatio = (float)directionResult.TreePtsCountRatio,
                        GrassPtsCountRatio = (float)directionResult.GrassPtsCountRatio,
                        WaterPtsCountRatio = (float)directionResult.WaterPtsCountRatio,
                        SkyPtsCountRatio = (float)directionResult.SkyPtsCountRatio,

                        ElementNumber = (float)directionResult.ElementNumber,
                        FloorHeights = (float)directionResult.FloorHeights,
                        InteriorClosestDist = (float)directionResult.InteriorClosestDist,
                        BuildingClosestDist = (float)directionResult.BuildingClosestDist,
                        ContextWindowClosestDist = (float)directionResult.ContextWindowClosestDist,
                        EquipmentClosestDist = (float)directionResult.EquipmentClosestDist,
                        LandmarkClosestDist = (float)directionResult.LandmarkClosestDist,
                        SidewalkClosestDist = (float)directionResult.SidewalkClosestDist,
                        RoadClosestDist = (float)directionResult.RoadClosestDist,
                        ParkingLotClosestDist = (float)directionResult.ParkingLotClosestDist,

                        TreeClosestDist = (float)directionResult.TreeClosestDist,
                        GrassClosestDist = (float)directionResult.GrassClosestDist,
                        WaterClosestDist = (float)directionResult.WaterClosestDist

                    };
                    ModelInputViewContent sampleDataViewContent = new ModelInputViewContent()
                    {

                        WindowAreaSum = (float)directionResult.WindowAreaRatio,
                        Z1PtsCountRatio = (float)directionResult.Z1PtsCountRatio,
                        Z2PtsCountRatio = (float)directionResult.Z2PtsCountRatio,
                        Z3PtsCountRatio = (float)directionResult.Z3PtsCountRatio,
                        Z4PtsCountRatio = (float)directionResult.Z4PtsCountRatio,
                        InteriorPtsCountRatio = (float)directionResult.InteriorPtsCountRatio,
                        BuildingPtsCountRatio = (float)directionResult.BuildingPtsCountRatio,
                        ContextWindowPtsCountRatio = (float)directionResult.ContextWindowPtsCountRatio,
                        EquipmentPtsCountRatio = (float)directionResult.EquipmentPtsCountRatio,
                        LandmarkPtsCountRatio = (float)directionResult.LandmarkPtsCountRatio,
                        SidewalkPtsCountRatio = (float)directionResult.SidewalkPtsCountRatio,
                        RoadPtsCountRatio = (float)directionResult.RoadPtsCountRatio,
                        ParkingLotPtsCountRatio = (float)directionResult.ParkingLotPtsCountRatio,
                        TreePtsCountRatio = (float)directionResult.TreePtsCountRatio,
                        GrassPtsCountRatio = (float)directionResult.GrassPtsCountRatio,
                        WaterPtsCountRatio = (float)directionResult.WaterPtsCountRatio,
                        SkyPtsCountRatio = (float)directionResult.SkyPtsCountRatio,

                        ElementNumber = (float)directionResult.ElementNumber,
                        FloorHeights = (float)directionResult.FloorHeights,
                        InteriorClosestDist = (float)directionResult.InteriorClosestDist,
                        BuildingClosestDist = (float)directionResult.BuildingClosestDist,
                        ContextWindowClosestDist = (float)directionResult.ContextWindowClosestDist,
                        EquipmentClosestDist = (float)directionResult.EquipmentClosestDist,
                        LandmarkClosestDist = (float)directionResult.LandmarkClosestDist,
                        SidewalkClosestDist = (float)directionResult.SidewalkClosestDist,
                        RoadClosestDist = (float)directionResult.RoadClosestDist,
                        ParkingLotClosestDist = (float)directionResult.ParkingLotClosestDist,

                        TreeClosestDist = (float)directionResult.TreeClosestDist,
                        GrassClosestDist = (float)directionResult.GrassClosestDist,
                        WaterClosestDist = (float)directionResult.WaterClosestDist

                    };
                    ModelInputViewAccess sampleDataViewAccess = new ModelInputViewAccess()
                    {

                        WindowAreaSum = (float)directionResult.WindowAreaRatio,
                        Z1PtsCountRatio = (float)directionResult.Z1PtsCountRatio,
                        Z2PtsCountRatio = (float)directionResult.Z2PtsCountRatio,
                        Z3PtsCountRatio = (float)directionResult.Z3PtsCountRatio,
                        Z4PtsCountRatio = (float)directionResult.Z4PtsCountRatio,
                        InteriorPtsCountRatio = (float)directionResult.InteriorPtsCountRatio,
                        BuildingPtsCountRatio = (float)directionResult.BuildingPtsCountRatio,
                        ContextWindowPtsCountRatio = (float)directionResult.ContextWindowPtsCountRatio,
                        EquipmentPtsCountRatio = (float)directionResult.EquipmentPtsCountRatio,
                        LandmarkPtsCountRatio = (float)directionResult.LandmarkPtsCountRatio,
                        SidewalkPtsCountRatio = (float)directionResult.SidewalkPtsCountRatio,
                        RoadPtsCountRatio = (float)directionResult.RoadPtsCountRatio,
                        ParkingLotPtsCountRatio = (float)directionResult.ParkingLotPtsCountRatio,
                        TreePtsCountRatio = (float)directionResult.TreePtsCountRatio,
                        GrassPtsCountRatio = (float)directionResult.GrassPtsCountRatio,
                        WaterPtsCountRatio = (float)directionResult.WaterPtsCountRatio,
                        SkyPtsCountRatio = (float)directionResult.SkyPtsCountRatio,

                        ElementNumber = (float)directionResult.ElementNumber,
                        FloorHeights = (float)directionResult.FloorHeights,
                        InteriorClosestDist = (float)directionResult.InteriorClosestDist,
                        BuildingClosestDist = (float)directionResult.BuildingClosestDist,
                        ContextWindowClosestDist = (float)directionResult.ContextWindowClosestDist,
                        EquipmentClosestDist = (float)directionResult.EquipmentClosestDist,
                        LandmarkClosestDist = (float)directionResult.LandmarkClosestDist,
                        SidewalkClosestDist = (float)directionResult.SidewalkClosestDist,
                        RoadClosestDist = (float)directionResult.RoadClosestDist,
                        ParkingLotClosestDist = (float)directionResult.ParkingLotClosestDist,

                        TreeClosestDist = (float)directionResult.TreeClosestDist,
                        GrassClosestDist = (float)directionResult.GrassClosestDist,
                        WaterClosestDist = (float)directionResult.WaterClosestDist

                    };
                    ModelInputPrivacy sampleDataPrivacy = new ModelInputPrivacy()
                    {

                        WindowAreaSum = (float)directionResult.WindowAreaRatio,
                        Z1PtsCountRatio = (float)directionResult.Z1PtsCountRatio,
                        Z2PtsCountRatio = (float)directionResult.Z2PtsCountRatio,
                        Z3PtsCountRatio = (float)directionResult.Z3PtsCountRatio,
                        Z4PtsCountRatio = (float)directionResult.Z4PtsCountRatio,
                        InteriorPtsCountRatio = (float)directionResult.InteriorPtsCountRatio,
                        BuildingPtsCountRatio = (float)directionResult.BuildingPtsCountRatio,
                        ContextWindowPtsCountRatio = (float)directionResult.ContextWindowPtsCountRatio,
                        EquipmentPtsCountRatio = (float)directionResult.EquipmentPtsCountRatio,
                        LandmarkPtsCountRatio = (float)directionResult.LandmarkPtsCountRatio,
                        SidewalkPtsCountRatio = (float)directionResult.SidewalkPtsCountRatio,
                        RoadPtsCountRatio = (float)directionResult.RoadPtsCountRatio,
                        ParkingLotPtsCountRatio = (float)directionResult.ParkingLotPtsCountRatio,
                        TreePtsCountRatio = (float)directionResult.TreePtsCountRatio,
                        GrassPtsCountRatio = (float)directionResult.GrassPtsCountRatio,
                        WaterPtsCountRatio = (float)directionResult.WaterPtsCountRatio,
                        SkyPtsCountRatio = (float)directionResult.SkyPtsCountRatio,

                        ElementNumber = (float)directionResult.ElementNumber,
                        FloorHeights = (float)directionResult.FloorHeights,
                        InteriorClosestDist = (float)directionResult.InteriorClosestDist,
                        BuildingClosestDist = (float)directionResult.BuildingClosestDist,
                        ContextWindowClosestDist = (float)directionResult.ContextWindowClosestDist,
                        EquipmentClosestDist = (float)directionResult.EquipmentClosestDist,
                        LandmarkClosestDist = (float)directionResult.LandmarkClosestDist,
                        SidewalkClosestDist = (float)directionResult.SidewalkClosestDist,
                        RoadClosestDist = (float)directionResult.RoadClosestDist,
                        ParkingLotClosestDist = (float)directionResult.ParkingLotClosestDist,

                        TreeClosestDist = (float)directionResult.TreeClosestDist,
                        GrassClosestDist = (float)directionResult.GrassClosestDist,
                        WaterClosestDist = (float)directionResult.WaterClosestDist

                    };

                    

                    //max:43259, min: 17892
                    //(directionResult.WindowAreaSum * 5288.02083158) > 17892) && ((directionResult.WindowAreaSum * 5288.02083158) < 43259)
                    if (directionResult.WindowAreaRatio > 0)
                    {
                        // Make a single prediction on the sample data and print results
                        var overallRating = ConsumeOverallRating.Predict(sampleDataOverallRating);
                        var viewContent = ConsumeViewContent.Predict(sampleDataViewContent);
                        var viewAccess = ConsumeViewAccess.Predict(sampleDataViewAccess);
                        var privacy = ConsumePrivacy.Predict(sampleDataPrivacy);
                        var framework = directionResult.ViewContentFramework;
                        var SPVEI = directionResult.SPVEI;
                        
                        overallRatings.Add(overallRating.OverallRatingB);
                        viewContents.Add(viewContent.ViewContentB);
                        viewAccesses.Add(viewAccess.ViewAccessB);
                        privacys.Add(privacy.PrivacyB);
                        frameworks.Add(framework);
                        SPVEIs.Add(SPVEI);
                        
                        directionResult.PredictedOverallRating = overallRatings[overallRatings.Count - 1];
                        directionResult.PredictedViewContent = viewContents[viewContents.Count - 1];
                        directionResult.PredictedViewAccess = viewAccesses[viewAccesses.Count - 1];
                        directionResult.PredictedPrivacy = privacys[privacys.Count - 1];

                    }
                    else //when there is no view, score is NaN
                    {
                        overallRatings.Add(double.NaN);
                        viewContents.Add(double.NaN);
                        viewAccesses.Add(double.NaN);
                        privacys.Add(double.NaN);
                        frameworks.Add(double.NaN);
                        SPVEIs.Add(double.NaN);
                        
                        directionResult.PredictedOverallRating = double.NaN;
                        directionResult.PredictedViewContent = double.NaN;
                        directionResult.PredictedOverallRating = double.NaN;
                        directionResult.PredictedViewContent = double.NaN;
                        directionResult.ViewContentFramework = double.NaN;
                        directionResult.SPVEI = double.NaN;
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
            seemoResult.ToFile(dir + @"\Result" + time + ".json");


            report.AppendLine("Saving result: " + sp.ElapsedMilliseconds  + "[ms]");
            sp.Restart();

            //Debug.report.WriteLine("Saving result: " + sp.ElapsedMilliseconds + "[ms]");



            DA.SetData(0, report.ToString());
            DA.SetData(1, seemoResult);
            DA.SetData(2, dir + @"\Result" + time + ".json");


            DA.SetDataList(3, overallRatings);
            DA.SetDataList(4, viewContents);
            DA.SetDataList(5, viewAccesses);
            DA.SetDataList(6, privacys);
            DA.SetDataList(7, frameworks);
            DA.SetDataList(8, SPVEIs);


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
                return Properties.Resources.viewAnalyzer;
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