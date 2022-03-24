using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;

namespace SeemoPredictor.Components
{
    public class CompareriteComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Comparison class.
        /// </summary>
        public CompareriteComponent()
          : base("Comparerite", "Comparerite",
              "Compare two result files",
              "SeEmo", "4|Visualize")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            //  pManager.AddGenericParameter("ViewResult", "ViewResult", "ViewResult", GH_ParamAccess.tree);
            pManager.AddTextParameter("Optimized", "Optimized", "Optimized result file path", GH_ParamAccess.item);
            pManager.AddTextParameter("Original", "Original", "Original result file path", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Overall Rating Graph", "Overall Rating Graph", "Overall Rating Graph", GH_ParamAccess.list);
            pManager.AddMeshParameter("View Content Graph", "View Content Graph", "View Content Graph", GH_ParamAccess.list);
            pManager.AddMeshParameter("View Access Graph", "View Access Graph", "View Access Graph", GH_ParamAccess.list);
            pManager.AddMeshParameter("Privacy Graph", "Privacy Graph", "Privacy Graph", GH_ParamAccess.list);
            pManager.AddNumberParameter("OverallRating", "OverallRating", "OverallRating", GH_ParamAccess.item);
            pManager.AddNumberParameter("View Content", "View Content", "View Content", GH_ParamAccess.item);
            pManager.AddNumberParameter("View Access", "View Access", "View Access", GH_ParamAccess.item);
            pManager.AddNumberParameter("Privacy", "Privacy", "Privacy", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string path1 = "";
            if (!DA.GetData(0, ref path1)) { return; }

            if (!File.Exists(path1))
            {

                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Result file does not exsist");
                return;
            }


            SeemoResult resultOpt = SeemoResult.FromFile(path1);


            string path2 = "";
            if (!DA.GetData(1, ref path2)) { return; }

            if (!File.Exists(path2))
            {

                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Result file does not exsist");
                return;
            }


            SeemoResult resultOri = SeemoResult.FromFile(path2);





            //GH_Structure<Grasshopper.Kernel.Types.IGH_Goo> List1;
            List<Mesh> overallRatingGraphs = new List<Mesh>();
            List<Mesh> viewContentGraphs = new List<Mesh>();
            List<Mesh> viewAccessGraphs = new List<Mesh>();
            List<Mesh> privacyGraphs = new List<Mesh>();
            List<double> overalls = new List<double>();
            List<double> contents = new List<double>();
            List<double> accesses = new List<double>();
            List<double> privacys = new List<double>();


            //Wind Rose Mesh Generation

            for (int i = 0; i < resultOpt.Results.Count; i++)
            {
                // i is node index
                Point3d viewPoint = new Point3d(resultOpt.Results[i].Pt.X, resultOpt.Results[i].Pt.Y, resultOpt.Results[i].Pt.Z);


                for (int j = 0; j < resultOpt.Results[i].DirectionsResults.Count; j++)
                {
                    //j is vector index
                    DirectionResult resultData3Opt = resultOpt.Results[i].DirectionsResults[j];
                    DirectionResult resultData3Ori = resultOri.Results[i].DirectionsResults[j];
                    Point3d viewVector = new Point3d(resultData3Opt.ViewVectorX, resultData3Opt.ViewVectorY, resultData3Opt.ViewVectorZ);


                    Transform rocw25 = Transform.Rotation(-0.125 * (Math.PI), viewPoint);
                    Transform roccw25 = Transform.Rotation(0.125 * (Math.PI), viewPoint);
                    Point3d p1 = new Point3d(viewPoint + 0.3 * viewVector);
                    Point3d p2 = p1;
                    p1.Transform(rocw25);
                    p2.Transform(roccw25);

                    //int prediction = (int)Math.Ceiling((r1.PredictedOverallRating + 5) / 10);
                    //1.overall Rating
                    Color overallRatingColor;
                    double overallRatingV = (resultData3Opt.PredictedOverallRating - resultData3Ori.PredictedOverallRating);

                    if (resultData3Opt.WindowAreaSum <= 0.05)
                    { overallRatingColor = Color.DarkGray; }
                    else if ((overallRatingV >= -10) && (overallRatingV <= 10))
                    {
                        overalls.Add(overallRatingV);
                        double overallRatingP = ColorGenerator.Remap(overallRatingV, -10, 10, 0, 1);
                        overallRatingColor = ColorGenerator.GetTriColour(overallRatingP, Color.Red, Color.BlueViolet, Color.Blue);
                    }
                    else
                    { overallRatingColor = Color.DarkGray; }

                    Mesh overallRatingPetal = new Mesh();
                    overallRatingPetal.Vertices.Add(viewPoint);
                    overallRatingPetal.Vertices.Add(p1);
                    overallRatingPetal.Vertices.Add(p2);

                    overallRatingPetal.Faces.AddFace(0, 1, 2);

                    overallRatingPetal.VertexColors.SetColor(0, overallRatingColor);
                    overallRatingPetal.VertexColors.SetColor(1, overallRatingColor);
                    overallRatingPetal.VertexColors.SetColor(2, overallRatingColor);

                    overallRatingPetal.Normals.ComputeNormals();
                    //petal.FaceNormals.ComputeFaceNormals();


                    overallRatingGraphs.Add(overallRatingPetal);

                    //2.viewContent
                    Color viewContentColor;
                    double viewContentV = (resultData3Opt.PredictedViewContent - resultData3Ori.PredictedViewContent);
                    if (resultData3Opt.WindowAreaSum <= 0.05)
                    { viewContentColor = Color.DarkGray; }
                    else if ((viewContentV >= -10) && (viewContentV <= 10))
                    {
                        contents.Add(viewContentV);
                        double viewContentP = ColorGenerator.Remap(viewContentV, -10, 10, 0, 1);
                        viewContentColor = ColorGenerator.GetTriColour(viewContentP, Color.Red, Color.Orange, Color.Green);
                    }
                    else
                    { viewContentColor = Color.DarkGray; }

                    Mesh viewContentPetal = new Mesh();
                    viewContentPetal.Vertices.Add(viewPoint);
                    viewContentPetal.Vertices.Add(p1);
                    viewContentPetal.Vertices.Add(p2);

                    viewContentPetal.Faces.AddFace(0, 1, 2);

                    viewContentPetal.VertexColors.SetColor(0, viewContentColor);
                    viewContentPetal.VertexColors.SetColor(1, viewContentColor);
                    viewContentPetal.VertexColors.SetColor(2, viewContentColor);

                    viewContentPetal.Normals.ComputeNormals();
                    //petal.FaceNormals.ComputeFaceNormals();

                    viewContentGraphs.Add(viewContentPetal);


                    //3.viewAccess
                    Color viewAccessColor;
                    double viewAccessV = (resultData3Opt.PredictedViewAccess - resultData3Ori.PredictedViewAccess);
                    if (resultData3Opt.WindowAreaSum <= 0.05)
                    { viewAccessColor = Color.DarkGray; }
                    else if ((viewAccessV >= -10) && (viewAccessV <= 10))
                    {
                        accesses.Add(viewAccessV);
                        double viewAccessP = ColorGenerator.Remap(viewAccessV, -10, 10, 0, 1);
                        viewAccessColor = ColorGenerator.GetTriColour(viewAccessP, Color.DarkKhaki, Color.Khaki, Color.Green);
                    }
                    else
                    { viewAccessColor = Color.DarkGray; }

                    Mesh viewAccessPetal = new Mesh();
                    viewAccessPetal.Vertices.Add(viewPoint);
                    viewAccessPetal.Vertices.Add(p1);
                    viewAccessPetal.Vertices.Add(p2);

                    viewAccessPetal.Faces.AddFace(0, 1, 2);

                    viewAccessPetal.VertexColors.SetColor(0, viewAccessColor);
                    viewAccessPetal.VertexColors.SetColor(1, viewAccessColor);
                    viewAccessPetal.VertexColors.SetColor(2, viewAccessColor);

                    viewAccessPetal.Normals.ComputeNormals();
                    //petal.FaceNormals.ComputeFaceNormals();

                    viewAccessGraphs.Add(viewAccessPetal);

                    //4.Privacy
                    Color privacyColor;
                    double privacyV = (resultData3Opt.PredictedPrivacy - resultData3Ori.PredictedPrivacy);
                    if (resultData3Opt.WindowAreaSum <= 0.05)
                    { privacyColor = Color.DarkGray; }
                    else if ((privacyV >= -10) && (privacyV <= 10))
                    {
                        privacys.Add(privacyV);
                        double privacyP = ColorGenerator.Remap(privacyV, -10, 10, 0, 1);
                        privacyColor = ColorGenerator.GetTriColour(privacyP, Color.Navy, Color.Purple, Color.Magenta);
                    }
                    else
                    { privacyColor = Color.DarkGray; }

                    Mesh privacyPetal = new Mesh();
                    privacyPetal.Vertices.Add(viewPoint);
                    privacyPetal.Vertices.Add(p1);
                    privacyPetal.Vertices.Add(p2);

                    privacyPetal.Faces.AddFace(0, 1, 2);

                    privacyPetal.VertexColors.SetColor(0, privacyColor);
                    privacyPetal.VertexColors.SetColor(1, privacyColor);
                    privacyPetal.VertexColors.SetColor(2, privacyColor);

                    privacyPetal.Normals.ComputeNormals();
                    //petal.FaceNormals.ComputeFaceNormals();

                    privacyGraphs.Add(privacyPetal);



                }


            }


            DA.SetDataList(0, overallRatingGraphs);
            DA.SetDataList(1, viewContentGraphs);
            DA.SetDataList(2, viewAccessGraphs);
            DA.SetDataList(3, privacyGraphs);

            double o = overalls.Average();
            double c = contents.Average();
            double a = accesses.Average();
            double p = privacys.Average();
            DA.SetData(4, o);
            DA.SetData(5, c);
            DA.SetData(6, a);
            DA.SetData(7, p);

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
            get { return new Guid("F587FDE1-ACB2-4201-8A08-1F39240C5DED"); }
        }
    }
}