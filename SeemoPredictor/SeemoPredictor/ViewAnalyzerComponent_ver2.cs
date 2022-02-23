using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using SeemoPredictor.SeemoGeo;

namespace SeemoPredictor
{
    public class ViewAnalyzerComponent_ver2 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ViewAnalyzerComponent_ver2 class.
        /// </summary>
        public ViewAnalyzerComponent_ver2()
          : base("ViewAnalyzerComponent_ver2", "Nickname",
              "Description",
              "Category", "Subcategory")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            
            pManager.AddGenericParameter("RoomSensor", "RoomSensor", "RoomSensor", GH_ParamAccess.list);
            pManager.AddGenericParameter("SmoFace", "SmoFace", "SmoFace", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            /*
            List<SeemoRoom> rooms = new List<SeemoRoom>();
            DA.GetDataList(0, smolists);

            List<SmoFace> smolists = new List<SmoFace>();
            DA.GetDataList(0, smolists);

            //List<GH_Goo> smoGoos = new List<GH_Goo>();
            var octree = new SmoPointOctree<SmoFace>(10.0f, new SmoPoint3(0,0,0) , 0.1f);

            for(int i = 0; i<smolists.Count; i++)
            {
                
                octree.Add(smolists[i], smolists[i].Center);
            }


            */



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

            //Merge all meshes into one mesh (0: building, 1: equipment, 2: tree, 3: pavement, 4: grass, 5: water, 6: dynamic, 7: sky, 8: room) 
            /*
            faceCnts.Add(buildingMesh.Faces.Count);
            faceCnts.Add(equipmentMesh.Faces.Count);
            faceCnts.Add(treeMesh.Faces.Count);
            faceCnts.Add(pavementMesh.Faces.Count);
            faceCnts.Add(grassMesh.Faces.Count);
            faceCnts.Add(waterMesh.Faces.Count);
            faceCnts.Add(dynamicMesh.Faces.Count);
            faceCnts.Add(msky.Faces.Count);


            SEnvironment env = new SEnvironment(sceneMesh, faceCnts, pavements, grass);
            */
            List<int> allFaceCnts = env.FaceCnt;
            Mesh allMergedMesh = env.SceneMesh;

            allMergedMesh.Append(room)


            for (int i = 0; i < rooms.Count; i++)
            {
                //define and get object
                SeemoRoom sr = rooms[i];
                SeemoOutdoor so = new SeemoOutdoor(sr, env);


                //compute window mesh
                sr.ComputeWindowMesh();


                //compute every view points and directions stored in seemoRoom
                for (int j = 0; j < sr.Pts.Count; j++)
                {
                    //define and get object
                    Node node = new Node();
                    node.RoomID = i;
                    node.Pt = sr.Pts[j];
                    node.Dirs = sr.Vecs;


                    for (int k = 0; k < sr.Vecs.Length; k++)
                    {
                        //define and get object
                        ResultDataSet directionResult = sr.ComputeWindowRay(j, k);
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



        internal static bool IsObstructed(SmoPointOctree<SmoFace> octree, SmoFace f, SmoPoint3 rayDir, double maxNodeSize)
        {
            var ray = new SmoRay(f.Center + (f.Normal * 0.01f), rayDir);
            var testGeo = octree.GetNearby(ray, (float)maxNodeSize);

            foreach (var g in testGeo)
            {

                SmoPoint3 ipt1;
                SmoPoint3 ipt2;
                bool i1 = false;
                bool i2 = false;

                i1 = SmoIntersect.RayTriangle_MollerTrumbore(ray, g.VertexList[0], g.VertexList[1], g.VertexList[2], out ipt1);

                if (i1) { return i1; }

                if (g.IsQuad)
                {
                    i2 = SmoIntersect.RayTriangle_MollerTrumbore(ray, g.VertexList[0], g.VertexList[2], g.VertexList[3], out ipt2);
                    if (i2) { return i2; }
                }
            }
            return false;
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