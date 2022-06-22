﻿
//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.IO;
//using Grasshopper;
//using Grasshopper.Kernel;
//using Grasshopper.Kernel.Data;
//using Rhino.Geometry;
//using System.Linq;
//using System.Drawing.Imaging;

//namespace SeemoPredictor
//{
//    public class VisualizationComponent : GH_Component
//    {
//        /// <summary>
//        /// Initializes a new instance of the VisualizationComponent class.
//        /// </summary>
//        public VisualizationComponent()
//          : base("Visualizer", "Visualizer",
//              "Wind Rose Visualization of the prediction result",
//              "SeEmo", "4|Visualize")
//        {
//        }

//        /// <summary>
//        /// Registers all the input parameters for this component.
//        /// </summary>
//        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
//        {
//            //  pManager.AddGenericParameter("ViewResult", "ViewResult", "ViewResult", GH_ParamAccess.tree);
//            pManager.AddTextParameter("Path", "Path", "Result file path", GH_ParamAccess.item);
//            pManager.AddNumberParameter("Graph Size", "Size", "Graph Size", GH_ParamAccess.item);
//        }

//        /// <summary>
//        /// Registers all the output parameters for this component.
//        /// </summary>
//        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
//        {
//            pManager.AddMeshParameter("Overall Rating Graph", "Overall Rating Graph", "Overall Rating Graph", GH_ParamAccess.list);
//            pManager.AddMeshParameter("View Content Graph", "View Content Graph", "View Content Graph", GH_ParamAccess.list);
//            pManager.AddMeshParameter("View Access Graph", "View Access Graph", "View Access Graph", GH_ParamAccess.list);
//            pManager.AddMeshParameter("Privacy Graph", "Privacy Graph", "Privacy Graph", GH_ParamAccess.list);
//            pManager.AddMeshParameter("Framework Graph", "Framework Graph", "Framework Graph", GH_ParamAccess.list);
//            pManager.AddMeshParameter("SPVEI Graph", "SPVEI Graph", "S-PVEI Graph", GH_ParamAccess.list);
//            //pManager.AddMeshParameter("I-PVEI Graph", "I-PVEI Graph", "I-PVEI Graph", GH_ParamAccess.list);
//            pManager.AddNumberParameter("OverallRating", "OverallRating", "OverallRating", GH_ParamAccess.item);
//            pManager.AddNumberParameter("View Content", "View Content", "View Content", GH_ParamAccess.item);
//            pManager.AddNumberParameter("View Access", "View Access", "View Access", GH_ParamAccess.item);
//            pManager.AddNumberParameter("Privacy", "Privacy", "Privacy", GH_ParamAccess.item);
//            pManager.AddNumberParameter("Framework", "Framework", "Framework", GH_ParamAccess.item);
//            pManager.AddNumberParameter("S-PVEI", "S-PVEI", "S-PVEI", GH_ParamAccess.item);
//            //pManager.AddNumberParameter("I-PVEI", "I-PVEI", "I-PVEI", GH_ParamAccess.item);

//        }

//        /// <summary>
//        /// This is the method that actually does the work.
//        /// </summary>
//        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
//        protected override void SolveInstance(IGH_DataAccess DA)
//        {
//            string path = "";
//            double scale = 1;
//            if (!DA.GetData(0, ref path)) { return; }
//            DA.GetData(1, ref scale);

//            if ( ! File.Exists(path)) {

//                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Result file does not exsist");
//                return;
//            }


//            SeemoResult result = SeemoResult.FromFile(path);


//            //GH_Structure<Grasshopper.Kernel.Types.IGH_Goo> List1;
//            List<Mesh> overallRatingGraphs = new List<Mesh>();
//            List<Mesh> viewContentGraphs = new List<Mesh>();
//            List<Mesh> viewAccessGraphs = new List<Mesh>();
//            List<Mesh> privacyGraphs = new List<Mesh>();
//            List<Mesh> frameworkGraphs = new List<Mesh>();
//            List<Mesh> SPVEIGraphs = new List<Mesh>();
//            //List<Mesh> IPVEIGraphs = new List<Mesh>();
//            List<double> overalls = new List<double>();
//            List<double> contents = new List<double>();
//            List<double> accesses = new List<double>();
//            List<double> privacys = new List<double>();
//            List<double> frameworks = new List<double>();
//            List<double> SPVEIs = new List<double>();
//            //List<double> IPVEIs = new List<double>();


//            //Wind Rose Mesh Generation

//            for (int i = 0; i < result.Results.Count; i++)
//            {
//                // i is node index
//                Point3d viewPoint = new Point3d(result.Results[i].Pt.X, result.Results[i].Pt.Y, result.Results[i].Pt.Z);


//                Transform rocw25 = Transform.Rotation(-0.125 * (Math.PI), viewPoint);
//                Transform roccw25 = Transform.Rotation(0.125 * (Math.PI), viewPoint);

//                //Point3d p1 = new Point3d(viewPoint + scale * 0.3 * viewVector);
//                //Point3d p2 = p1;
//                //p1.Transform(rocw25);
//                //p2.Transform(roccw25);
//                List<Point3d> pointys_overalls = new List<Point3d>();
//                List<Point3d> ints_overalls = new List<Point3d>();
//                List<Point3d> pointys_contents = new List<Point3d>();
//                List<Point3d> ints_contents = new List<Point3d>();
//                List<Point3d> pointys_accesses = new List<Point3d>();
//                List<Point3d> ints_accessess = new List<Point3d>();
//                List<Point3d> pointys_privacys = new List<Point3d>();
//                List<Point3d> ints_privacys = new List<Point3d>();
//                List<Point3d> pointys_frameworks = new List<Point3d>();
//                List<Point3d> ints_frameworks = new List<Point3d>();
//                List<Point3d> pointys_SPVEIs = new List<Point3d>();
//                List<Point3d> ints_SPVEIs = new List<Point3d>();


//                for (int j = 0; j<result.Results[i].DirectionsResults.Count; j++)
//                {
//                    //j is vector index
//                    DirectionResult resultData3 = result.Results[i].DirectionsResults[j];
//                    Point3d viewVector = new Point3d(resultData3.ViewVectorX, resultData3.ViewVectorY, resultData3.ViewVectorZ);

//                    Point3d p1 = new Point3d(viewPoint + scale * 0.3 * viewVector);


//                    //int prediction = (int)Math.Ceiling((r1.PredictedOverallRating + 5) / 10);
//                    //1.overall Rating
//                    double overallRatingV = resultData3.PredictedOverallRating;
//                    double overallRatingP;
                    
//                    if (resultData3.WindowAreaSum <= 0.05)
//                    { overallRatingP = 0; }
//                    else if((overallRatingV >= -5) && (overallRatingV <= 5))
//                    {
//                        overalls.Add(overallRatingV);
//                        overallRatingP = ColorGenerator.Remap(overallRatingV, -5, 5, 0, 1);
//                    }
//                    else
//                    { overallRatingP = 0; }
//                    remap_overalls.Add(overallRatingP);
//                    pointys_overalls.Add(viewPoint + scale * 0.3 * viewVector * (0.5 + (overallRatingP * overallRatingP)));

//                    //2.viewContent
//                    double viewContentV = resultData3.PredictedViewContent;
//                    double viewContentP;

//                    if (resultData3.WindowAreaSum <= 0.05)
//                    { viewContentP = 0; }
//                    else if ((viewContentV >= -5) && (viewContentV <= 5))
//                    {
//                        contents.Add(viewContentV);
//                        viewContentP = ColorGenerator.Remap(viewContentV, -5, 5, 0, 1);
//                    }
//                    else
//                    { viewContentP = 0; }
//                    remap_contents.Add(viewContentP);

//                    //3.viewAccess
//                    double viewAccessV = resultData3.PredictedViewAccess;
//                    double viewAccessP;

//                    if (resultData3.WindowAreaSum <= 0.05)
//                    { viewAccessP = 0; }
//                    else if ((viewAccessV >= -5) && (viewAccessV <= 5))
//                    {
//                        accesses.Add(viewAccessV);
//                        viewAccessP = ColorGenerator.Remap(viewAccessV, -5, 5, 0, 1);
//                    }
//                    else
//                    { viewAccessP = 0; }
//                    remap_accesses.Add(viewAccessP);

//                    //4.Privacy
//                    double privacyV = resultData3.PredictedPrivacy;
//                    double privacyP;
//                    if (resultData3.WindowAreaSum <= 0.05)
//                    { privacyP = 0; }
//                    else if ((privacyV >= -5) && (privacyV <= 5))
//                    {
//                        privacys.Add(privacyV);
//                        privacyP = ColorGenerator.Remap(privacyV, -5, 5, 0, 1);
                        
//                    }
//                    else
//                    { privacyP = 0; }
//                    remap_privacys.Add(privacyP);


//                    //5.Framework
//                    double frameworkV = resultData3.ViewContentFramework;
//                    double frameworkP;
//                    if (resultData3.WindowAreaSum <= 0.05)
//                    { frameworkP = 0; }
//                    else if ((frameworkV >= 0) && (frameworkV <= 1))
//                    {
//                        frameworks.Add(frameworkV);
//                        frameworkP = ColorGenerator.Remap(frameworkV, 0, 1, 0, 1);
                        
//                    }
//                    else
//                    { frameworkP = 0; }
//                    remap_frameworks.Add(frameworkP);


//                    //6.IPVEI
//                    double SPVEIV = resultData3.SPVEI;
//                    double SPVEIVP;
//                    if (resultData3.WindowAreaSum <= 0.05)
//                    { SPVEIVP = 0; }
//                    else if(SPVEIV == double.NaN)
//                    {
//                        { SPVEIVP = 0; }
//                    }
//                    else //if ((SPVEIV >= 0) && (SPVEIV <= 1))
//                    {
//                        if(SPVEIV <= 0.001) { SPVEIV = 0.001111; }
//                        if(SPVEIV >= 0.1) { SPVEIV = 0.09999; }
//                        SPVEIs.Add(SPVEIV);
//                        SPVEIVP = ((Math.Log10(SPVEIV) / 2.0f) + 1.5f);
                                                
//                    }
//                    remap_SPVEIs.Add(SPVEIVP);

//                }

//                //calculate intersecting points and draw tiangles and color the triangles
//                for (int j = 0; j < result.Results[i].DirectionsResults.Count; j++)
//                {

//                    //int prediction = (int)Math.Ceiling((r1.PredictedOverallRating + 5) / 10);
//                    //1.overall Rating
//                    double l1 = (pointys_overalls[(j) % result.Results[i].DirectionsResults.Count] - viewPoint).Length;
//                    double l2 = (pointys_overalls[(j + 1) % result.Results[i].DirectionsResults.Count] - viewPoint).Length;

//                    ints_overalls.Add((l1 * pointys_overalls[(j + 1) % result.Results[i].DirectionsResults.Count] + l2 * pointys_overalls[(j) % result.Results[i].DirectionsResults.Count]) / (l1 + l2));
//                }

//                for (int j = 0; j < result.Results[i].DirectionsResults.Count; j++)
//                {
//                    Color overallRatingColor;
//                    if (remap_overalls[j] == 0)
//                    { overallRatingColor = Color.Black; }
//                    else
//                    {
//                        overallRatingColor = ColorGenerator.GetTriColour(remap_overalls[j], Color.HotPink, Color.YellowGreen, Color.Cyan);
//                    }


//                    Mesh overallRatingPetal = new Mesh();
//                    overallRatingPetal.Vertices.Add(viewPoint);
//                    overallRatingPetal.Vertices.Add(ints_overalls[(( j - 1 ) + result.Results[i].DirectionsResults.Count) % result.Results[i].DirectionsResults.Count]);
//                    overallRatingPetal.Vertices.Add(ints_overalls[((j) + result.Results[i].DirectionsResults.Count) % result.Results[i].DirectionsResults.Count]);

//                    overallRatingPetal.Faces.AddFace(0, 1, 2);

//                    overallRatingPetal.VertexColors.SetColor(0, overallRatingColor);
//                    overallRatingPetal.VertexColors.SetColor(1, overallRatingColor);
//                    overallRatingPetal.VertexColors.SetColor(2, overallRatingColor);

//                    overallRatingPetal.Normals.ComputeNormals();
//                    //petal.FaceNormals.ComputeFaceNormals();


//                    overallRatingGraphs.Add(overallRatingPetal);

//                    //2.viewContent
//                    //Color viewContentColor;
//                    //double viewContentV = resultData3.PredictedViewContent;
//                    //if (resultData3.WindowAreaSum <= 0.05)
//                    //{ viewContentColor = Color.Black; }
//                    //else if ((viewContentV >= -5) && (viewContentV <= 5))
//                    //{
//                    //    contents.Add(viewContentV);
//                    //    double viewContentP = ColorGenerator.Remap(viewContentV, -5, 5, 0, 1);

//                    //    Color lightIndianRed = Color.FromArgb(255, 205 + 20, 92 + 20, 92 + 20);
//                    //    viewContentColor = ColorGenerator.GetTriColour(viewContentP, lightIndianRed, Color.Beige, Color.LightSeaGreen);

//                    //}
//                    //else
//                    //{ viewContentColor = Color.Black; }

//                    //Mesh viewContentPetal = new Mesh();
//                    //viewContentPetal.Vertices.Add(viewPoint);
//                    //viewContentPetal.Vertices.Add(p1);
//                    //viewContentPetal.Vertices.Add(p2);

//                    //viewContentPetal.Faces.AddFace(0, 1, 2);

//                    //viewContentPetal.VertexColors.SetColor(0, viewContentColor);
//                    //viewContentPetal.VertexColors.SetColor(1, viewContentColor);
//                    //viewContentPetal.VertexColors.SetColor(2, viewContentColor);

//                    //viewContentPetal.Normals.ComputeNormals();
//                    ////petal.FaceNormals.ComputeFaceNormals();

//                    //viewContentGraphs.Add(viewContentPetal);


//                    ////3.viewAccess
//                    //Color viewAccessColor;
//                    //double viewAccessV = resultData3.PredictedViewAccess;
//                    //if (resultData3.WindowAreaSum <= 0.05)
//                    //{ viewAccessColor = Color.Black; }
//                    //else if ((viewAccessV >= -5) && (viewAccessV <= 5))
//                    //{
//                    //    accesses.Add(viewAccessV);
//                    //    double viewAccessP = ColorGenerator.Remap(viewAccessV, -5, 5, 0, 1);
//                    //    viewAccessColor = ColorGenerator.GetTriColour(viewAccessP, Color.Red, Color.Yellow, Color.Blue);
//                    //}
//                    //else
//                    //{ viewAccessColor = Color.Black; }

//                    //Mesh viewAccessPetal = new Mesh();
//                    //viewAccessPetal.Vertices.Add(viewPoint);
//                    //viewAccessPetal.Vertices.Add(p1);
//                    //viewAccessPetal.Vertices.Add(p2);

//                    //viewAccessPetal.Faces.AddFace(0, 1, 2);

//                    //viewAccessPetal.VertexColors.SetColor(0, viewAccessColor);
//                    //viewAccessPetal.VertexColors.SetColor(1, viewAccessColor);
//                    //viewAccessPetal.VertexColors.SetColor(2, viewAccessColor);

//                    //viewAccessPetal.Normals.ComputeNormals();
//                    ////petal.FaceNormals.ComputeFaceNormals();

//                    //viewAccessGraphs.Add(viewAccessPetal);

//                    ////4.Privacy
//                    //Color privacyColor;
//                    //double privacyV = resultData3.PredictedPrivacy;
//                    //if (resultData3.WindowAreaSum <= 0.05)
//                    //{ privacyColor = Color.Black; }
//                    //else if ((privacyV >= -5) && (privacyV <= 5))
//                    //{
//                    //    privacys.Add(privacyV);
//                    //    double privacyP = ColorGenerator.Remap(privacyV, -5, 5, 0, 1);
//                    //    privacyColor = ColorGenerator.GetTriColour(privacyP, Color.Orange, Color.LimeGreen, Color.DarkCyan);
//                    //}
//                    //else
//                    //{ privacyColor = Color.Black; }

//                    //Mesh privacyPetal = new Mesh();
//                    //privacyPetal.Vertices.Add(viewPoint);
//                    //privacyPetal.Vertices.Add(p1);
//                    //privacyPetal.Vertices.Add(p2);

//                    //privacyPetal.Faces.AddFace(0, 1, 2);

//                    //privacyPetal.VertexColors.SetColor(0, privacyColor);
//                    //privacyPetal.VertexColors.SetColor(1, privacyColor);
//                    //privacyPetal.VertexColors.SetColor(2, privacyColor);

//                    //privacyPetal.Normals.ComputeNormals();
//                    ////petal.FaceNormals.ComputeFaceNormals();

//                    //privacyGraphs.Add(privacyPetal);

//                    ////5.Framework
//                    //Color frameworkColor;
//                    //double frameworkV = resultData3.ViewContentFramework;
//                    //if (resultData3.WindowAreaSum <= 0.05)
//                    //{ frameworkColor = Color.Black; }
//                    //else if ((frameworkV >= 0) && (frameworkV <= 1))
//                    //{
//                    //    frameworks.Add(frameworkV);
//                    //    double frameworkP = ColorGenerator.Remap(frameworkV, 0, 1, 0, 1);
//                    //    Color P1 = Color.FromArgb(255, 255, 151, 82);
//                    //    Color P2 = Color.FromArgb(255, 237, 244, 160);
//                    //    Color P3 = Color.FromArgb(255, 152, 200, 117);
//                    //    frameworkColor = ColorGenerator.GetTriColour(frameworkP, P1, P2, P3);
//                    //}
//                    //else
//                    //{ frameworkColor = Color.Black; }

//                    //Mesh frameworkPetal = new Mesh();
//                    //frameworkPetal.Vertices.Add(viewPoint);
//                    //frameworkPetal.Vertices.Add(p1);
//                    //frameworkPetal.Vertices.Add(p2);

//                    //frameworkPetal.Faces.AddFace(0, 1, 2);

//                    //frameworkPetal.VertexColors.SetColor(0, frameworkColor);
//                    //frameworkPetal.VertexColors.SetColor(1, frameworkColor);
//                    //frameworkPetal.VertexColors.SetColor(2, frameworkColor);

//                    //frameworkPetal.Normals.ComputeNormals();
//                    ////petal.FaceNormals.ComputeFaceNormals();

//                    //frameworkGraphs.Add(frameworkPetal);

//                    ////6.IPVEI
//                    //Color SPVEIColor;
//                    //double SPVEIV = resultData3.SPVEI;
//                    //if (resultData3.WindowAreaSum <= 0.05)
//                    //{ SPVEIColor = Color.Black; }
//                    //else if (SPVEIV == double.NaN)
//                    //{
//                    //    { SPVEIColor = Color.Black; }
//                    //}
//                    //else //if ((SPVEIV >= 0) && (SPVEIV <= 1))
//                    //{
//                    //    if (SPVEIV <= 0.001) { SPVEIV = 0.001111; }
//                    //    if (SPVEIV >= 0.1) { SPVEIV = 0.09999; }
//                    //    SPVEIs.Add(SPVEIV);
//                    //    double remap = ((Math.Log10(SPVEIV) / 2.0f) + 1.5f);
//                    //    double SPVEIP = ColorGenerator.Remap(remap, 0, 1, 0, 1);
//                    //    Color P1 = Color.FromArgb(255, 255, 198, 253);
//                    //    Color P2 = Color.FromArgb(255, 227, 183, 224);
//                    //    Color P3 = Color.FromArgb(255, 152, 184, 255);
//                    //    SPVEIColor = ColorGenerator.GetTriColour(SPVEIP, P3, P2, P1);

//                    //}

//                    //Mesh SPVEIPetal = new Mesh();
//                    //SPVEIPetal.Vertices.Add(viewPoint);
//                    //SPVEIPetal.Vertices.Add(p1);
//                    //SPVEIPetal.Vertices.Add(p2);

//                    //SPVEIPetal.Faces.AddFace(0, 1, 2);

//                    //SPVEIPetal.VertexColors.SetColor(0, SPVEIColor);
//                    //SPVEIPetal.VertexColors.SetColor(1, SPVEIColor);
//                    //SPVEIPetal.VertexColors.SetColor(2, SPVEIColor);

//                    //SPVEIPetal.Normals.ComputeNormals();
//                    ////petal.FaceNormals.ComputeFaceNormals();

//                    //SPVEIGraphs.Add(SPVEIPetal);

//                }

//            }

            
//            DA.SetDataList(0, overallRatingGraphs);
//            DA.SetDataList(1, overallRatingGraphs);
//            DA.SetDataList(2, overallRatingGraphs);
//            DA.SetDataList(3, overallRatingGraphs);
//            DA.SetDataList(4, overallRatingGraphs);
//            DA.SetDataList(5, overallRatingGraphs);
//            //DA.SetDataList(1, viewContentGraphs);
//            //DA.SetDataList(2, viewAccessGraphs);
//            //DA.SetDataList(3, privacyGraphs);
//            //DA.SetDataList(4, frameworkGraphs);
//            //DA.SetDataList(5, SPVEIGraphs);
//            ////DA.SetDataList(5, IPVEIGraphs);

//            double o = overalls.Average();
//            double c = contents.Average();
//            double a = accesses.Average();
//            double p = privacys.Average();
//            double f = frameworks.Average();
//            double v = SPVEIs.Average();
//            //double v = IPVEIs.Average();
//            DA.SetData(6, o);
//            DA.SetData(7, c);
//            DA.SetData(8, a);
//            DA.SetData(9, p);
//            DA.SetData(10, f);
//            DA.SetData(11, v);

//        }

//        /// <summary>
//        /// Provides an Icon for the component.
//        /// </summary>
//        protected override System.Drawing.Bitmap Icon
//        {
//            get
//            {
//                //You can add image files to your project resources and access them like this:
//                // return Resources.IconForThisComponent;
//                return Properties.Resources.visualizer;
//            }
//        }

//        /// <summary>
//        /// Gets the unique ID for this component. Do not change this ID after release.
//        /// </summary>
//        public override Guid ComponentGuid
//        {
//            get { return new Guid("417BE727-FAE1-48A0-9A2A-F44D734A39F5"); }
//        }
//    }
//}