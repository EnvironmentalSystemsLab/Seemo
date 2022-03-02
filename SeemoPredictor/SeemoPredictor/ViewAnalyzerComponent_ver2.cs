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
    public class ViewAnalyzerComponent_ver2 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ViewAnalyzerComponent_ver2 class.
        /// </summary>
        public ViewAnalyzerComponent_ver2()
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
            
            pManager.AddGenericParameter("SeemoRoom", "SeemoRoom", "SeemoRoom", GH_ParamAccess.item);
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
            //pManager.AddGenericParameter("hits", "hits", "hits", GH_ParamAccess.list);
            //pManager.AddGenericParameter("rays", "rays", "rays", GH_ParamAccess.list);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<double> overallRatings = new List<double>();
            List<double> viewContents = new List<double>();
            List<double> viewAccesses = new List<double>();
            List<double> privacys = new List<double>();



            //output objects
            var seemoResult = new SeemoResult();
            List<Node> nodes = new List<Node>();
            //output test
            List<Point3d> hitsList = new List<Point3d>();
            List<Point3d> raysList = new List<Point3d>();

            //input objects
            SeemoInput room = new SeemoInput();
            SeemoInput env = new SeemoInput();
            

            DA.GetData(0, ref room);
            DA.GetData(1, ref env);

            SeemoInput input = SeemoInput.Merge2Inputs(room, env);
            
            //Generate octree
            List<SmoFace> smofaces = new List<SmoFace>();
            smofaces.AddRange(input.Room);
            smofaces.AddRange(input.Building);
            smofaces.AddRange(input.Equipment);
            smofaces.AddRange(input.Tree);
            smofaces.AddRange(input.Pavement);
            smofaces.AddRange(input.Grass);
            smofaces.AddRange(input.Water);
            smofaces.AddRange(input.Dynamics);
            smofaces.AddRange(input.Sky);

            //create octree
            SmoPointOctree<SmoFace> octree0 = new SmoPointOctree<SmoFace>(1000.0f, input.Pts[0], 0.05f);
            foreach (SmoFace f in smofaces)
            {
                octree0.Add(f, f.Center);
            }


            //calculating FloorHeights
            double floorheight;
            SmoPointOctree<SmoFace> octree1 = new SmoPointOctree<SmoFace>(1000.0f, input.Pts[0], 0.05f);
            foreach (SmoFace f in input.Pavement)
                octree1.Add(f, f.Center);
            foreach (SmoFace f in input.Grass)
                octree1.Add(f, f.Center);
            foreach (SmoFace f in input.Water)
                octree1.Add(f, f.Center);

            //compute every view points and directions stored in seemoRoom
            
            for (int i = 0; i < input.Pts.Count; i++)
            {
                List<ResultDataSet_ver2> nodeResult = new List<ResultDataSet_ver2>();

                //List<List<Vector3d>> winRayVectorsT = new List<List<Vector3d>>();

                //define and get object
                Node node = new Node();
                node.NodeID = i;
                node.Pt = input.Pts[i];
                node.Dirs = input.Vecs;

                
                /*
                SmoPoint3 groundIntersect = new SmoPoint3();
                var typeG = SmoIntersect.IsObstructed(ref groundIntersect, octree1, node.Pt, new SmoPoint3(0, 0, -1), 10.0f);
                if (typeG == SmoFace.SmoFaceType.Pavement || typeG == SmoFace.SmoFaceType.Grass || typeG == SmoFace.SmoFaceType.Water)
                {
                    floorheight = SmoPoint3.Distance(groundIntersect, node.Pt);
                }else{
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "There is no ground (pavement, grass, or water) vertically under the view point");
                    return;
                }
                */
                floorheight = node.Pt.Z;



                //generate ray and do machine learning and save data in direction result 
                //original
                //for (int j = 0; j < input.Vecs.Length; j++)
                {
                    //generateZoneRay and Define ResultDataSet
                    ResultDataSet_ver2 directionResult = input.GenerateZoneRay(i, j);
                    directionResult.ID = ("Point" + i.ToString() + ":" + "Dir" + j.ToString());
                    directionResult.Dir = input.Vecs[j];

                    //test output
                    List<SmoPoint3> hits = new List<SmoPoint3>();

                    //Compute octree intersect
                    SmoIntersect.MeshRayResultSave(ref directionResult, ref hits, octree0, node.Pt);
                    /*
                    //test output
                    List<Point3d> rays = new List<Point3d>();
                    foreach (SmoPoint3 hit in directionResult.sceneRayVectorsZ1)
                    {
                        Point3d hitR = new Point3d(hit.X, hit.Y, hit.Z);
                        rays.Add(hitR);
                    }
                    foreach (SmoPoint3 hit in directionResult.sceneRayVectorsZ2)
                    {
                        Point3d hitR = new Point3d(hit.X, hit.Y, hit.Z);
                        rays.Add(hitR);
                    }
                    foreach (SmoPoint3 hit in directionResult.sceneRayVectorsZ3)
                    {
                        Point3d hitR = new Point3d(hit.X, hit.Y, hit.Z);
                        rays.Add(hitR);
                    }
                    foreach (SmoPoint3 hit in directionResult.sceneRayVectorsZ4)
                    {
                        Point3d hitR = new Point3d(hit.X, hit.Y, hit.Z);
                        rays.Add(hitR);
                    }
                    raysList.AddRange(rays);

                    //test output ray
                    List<Point3d> hitsR = new List<Point3d>();
                    foreach (SmoPoint3 hit in hits)
                    {
                        Point3d hitR = new Point3d(hit.X, hit.Y, hit.Z);
                        hitsR.Add(hitR);
                    }
                    hitsList.AddRange(hitsR);

                    //Run (octree)IsObstructed save data into directionResult 

                    directionResult.FloorHeights = floorheight;

                    */

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


                    directionResult.ViewPointX = input.Pts[i].X;
                    directionResult.ViewPointY = input.Pts[i].Y;
                    directionResult.ViewPointZ = input.Pts[i].Z;
                    directionResult.ViewVectorX = input.Vecs[j].X;
                    directionResult.ViewVectorY = input.Vecs[j].Y;
                    directionResult.ViewVectorZ = input.Vecs[j].Z;


                    //Save direction result
                    nodeResult.Add(directionResult);
                }


                //threading try
                Thread thread1 = new Thread(() =>
                {
                    for (int j = 0; j < input.Vecs.Length / 2; j++)
                    {
                        //generateZoneRay and Define ResultDataSet
                        ResultDataSet_ver2 directionResult = input.GenerateZoneRay(i, j);
                        directionResult.ID = ("Point" + i.ToString() + ":" + "Dir" + j.ToString());
                        directionResult.Dir = input.Vecs[j];

                        //test output
                        List<SmoPoint3> hits = new List<SmoPoint3>();

                        //Compute octree intersect
                        SmoIntersect.MeshRayResultSave(ref directionResult, ref hits, octree0, node.Pt);
                        /*
                        //test output
                        List<Point3d> rays = new List<Point3d>();
                        foreach (SmoPoint3 hit in directionResult.sceneRayVectorsZ1)
                        {
                            Point3d hitR = new Point3d(hit.X, hit.Y, hit.Z);
                            rays.Add(hitR);
                        }
                        foreach (SmoPoint3 hit in directionResult.sceneRayVectorsZ2)
                        {
                            Point3d hitR = new Point3d(hit.X, hit.Y, hit.Z);
                            rays.Add(hitR);
                        }
                        foreach (SmoPoint3 hit in directionResult.sceneRayVectorsZ3)
                        {
                            Point3d hitR = new Point3d(hit.X, hit.Y, hit.Z);
                            rays.Add(hitR);
                        }
                        foreach (SmoPoint3 hit in directionResult.sceneRayVectorsZ4)
                        {
                            Point3d hitR = new Point3d(hit.X, hit.Y, hit.Z);
                            rays.Add(hitR);
                        }
                        raysList.AddRange(rays);

                        //test output ray
                        List<Point3d> hitsR = new List<Point3d>();
                        foreach (SmoPoint3 hit in hits)
                        {
                            Point3d hitR = new Point3d(hit.X, hit.Y, hit.Z);
                            hitsR.Add(hitR);
                        }
                        hitsList.AddRange(hitsR);

                        //Run (octree)IsObstructed save data into directionResult 

                        directionResult.FloorHeights = floorheight;

                        */

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


                        directionResult.ViewPointX = input.Pts[i].X;
                        directionResult.ViewPointY = input.Pts[i].Y;
                        directionResult.ViewPointZ = input.Pts[i].Z;
                        directionResult.ViewVectorX = input.Vecs[j].X;
                        directionResult.ViewVectorY = input.Vecs[j].Y;
                        directionResult.ViewVectorZ = input.Vecs[j].Z;


                        //Save direction result
                        nodeResult.Add(directionResult);
                    }
                });
                Thread thread2 = new Thread(() =>
                {
                    for (int k = (int)Math.Ceiling((double)(input.Vecs.Length) / 2); k < input.Vecs.Length; k++)
                    {
                        //generateZoneRay and Define ResultDataSet
                        ResultDataSet_ver2 directionResult = input.GenerateZoneRay(i, k);
                        directionResult.ID = ("Point" + i.ToString() + ":" + "Dir" + k.ToString());
                        directionResult.Dir = input.Vecs[k];

                        //test output
                        List<SmoPoint3> hits = new List<SmoPoint3>();

                        //Compute octree intersect
                        SmoIntersect.MeshRayResultSave(ref directionResult, ref hits, octree0, node.Pt);
                        /*
                        //test output
                        List<Point3d> rays = new List<Point3d>();
                        foreach (SmoPoint3 hit in directionResult.sceneRayVectorsZ1)
                        {
                            Point3d hitR = new Point3d(hit.X, hit.Y, hit.Z);
                            rays.Add(hitR);
                        }
                        foreach (SmoPoint3 hit in directionResult.sceneRayVectorsZ2)
                        {
                            Point3d hitR = new Point3d(hit.X, hit.Y, hit.Z);
                            rays.Add(hitR);
                        }
                        foreach (SmoPoint3 hit in directionResult.sceneRayVectorsZ3)
                        {
                            Point3d hitR = new Point3d(hit.X, hit.Y, hit.Z);
                            rays.Add(hitR);
                        }
                        foreach (SmoPoint3 hit in directionResult.sceneRayVectorsZ4)
                        {
                            Point3d hitR = new Point3d(hit.X, hit.Y, hit.Z);
                            rays.Add(hitR);
                        }
                        raysList.AddRange(rays);

                        //test output ray
                        List<Point3d> hitsR = new List<Point3d>();
                        foreach (SmoPoint3 hit in hits)
                        {
                            Point3d hitR = new Point3d(hit.X, hit.Y, hit.Z);
                            hitsR.Add(hitR);
                        }
                        hitsList.AddRange(hitsR);

                        //Run (octree)IsObstructed save data into directionResult 

                        directionResult.FloorHeights = floorheight;

                        */

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


                        directionResult.ViewPointX = input.Pts[i].X;
                        directionResult.ViewPointY = input.Pts[i].Y;
                        directionResult.ViewPointZ = input.Pts[i].Z;
                        directionResult.ViewVectorX = input.Vecs[k].X;
                        directionResult.ViewVectorY = input.Vecs[k].Y;
                        directionResult.ViewVectorZ = input.Vecs[k].Z;


                        //Save direction result
                        nodeResult.Add(directionResult);
                    }
                });

                thread1.Start();
                thread2.Start(); 
                node.DirectionsResults = nodeResult;
                //Save node result
                nodes.Add(node);
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

            //DA.SetDataList(5, hitsList);
            //DA.SetDataList(6, raysList);
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