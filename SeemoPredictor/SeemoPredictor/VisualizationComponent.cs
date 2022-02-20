using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;

namespace SeemoPredictor
{
    public class VisualizationComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the VisualizationComponent class.
        /// </summary>
        public VisualizationComponent()
          : base("Visualizer0205", "Visualizer0205",
              "Wind Rose Visualization of the prediction result",
              "SeEmo", "4|Visualize")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            //  pManager.AddGenericParameter("ViewResult", "ViewResult", "ViewResult", GH_ParamAccess.tree);
            pManager.AddTextParameter("Path", "Path", "Result file path", GH_ParamAccess.item);
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

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string path = "";
            if (!DA.GetData(0, ref path)) { return; }

            if ( ! File.Exists(path)) {

                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Result file does not exsist");
                return;
            }


            SeemoResult result = SeemoResult.FromFile(path);


            //GH_Structure<Grasshopper.Kernel.Types.IGH_Goo> List1;
            List<Mesh> overallRatingGraphs = new List<Mesh>();
            List<Mesh> viewContentGraphs = new List<Mesh>();
            List<Mesh> viewAccessGraphs = new List<Mesh>();
            List<Mesh> privacyGraphs = new List<Mesh>();


            //Wind Rose Mesh Generation

            for(int i = 0; i < result.Results.Count; i++)
            {
                // i is node index
                Point3d viewPoint = result.Results[i].Pt;


                for (int j = 0; j<result.Results[i].DirectionsResults.Count; j++)
                {
                    //j is vector index
                    ResultDataSet resultData3 = result.Results[i].DirectionsResults[j];
                    Point3d viewVector = new Point3d(resultData3.ViewVectorX, resultData3.ViewVectorY, resultData3.ViewVectorZ);
                    
                    
                    Transform rocw25 = Transform.Rotation(-0.125 * (Math.PI), viewPoint);
                    Transform roccw25 = Transform.Rotation(0.125 * (Math.PI), viewPoint);
                    Point3d p1 = new Point3d(viewPoint + 0.3 * viewVector);
                    Point3d p2 = p1;
                    p1.Transform(rocw25);
                    p2.Transform(roccw25);

                    //int prediction = (int)Math.Ceiling((r1.PredictedOverallRating + 5) / 10);
                    //1.overall Rating
                    Color overallRatingColor;
                    double overallRatingV = resultData3.PredictedOverallRating;
                    if (resultData3.WindowAreaSum == 0)
                    { overallRatingColor = Color.DarkGray; }
                    else if((overallRatingV >= -5) && (overallRatingV <= 5))
                    {
                        double overallRatingP = ColorGenerator.Remap(overallRatingV, -5, 5, 0, 1);
                        overallRatingColor = ColorGenerator.GetTriColour(overallRatingP, Color.Gray, Color.Yellow, Color.Navy);
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
                    double viewContentV = resultData3.PredictedViewContent;
                    if (resultData3.WindowAreaSum == 0)
                    { viewContentColor = Color.DarkGray; }
                    else if ((viewContentV >= -5) && (viewContentV <= 5))
                    {
                        double viewContentP = ColorGenerator.Remap(viewContentV, -5, 5, 0, 1);
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
                    double viewAccessV = resultData3.PredictedViewAccess;
                    if (resultData3.WindowAreaSum == 0)
                    { viewAccessColor = Color.DarkGray; }
                    else if ((viewAccessV >= -5) && (viewAccessV <= 5))
                    {
                        double viewAccessP = ColorGenerator.Remap(viewAccessV, -5, 5, 0, 1);
                        viewAccessColor = ColorGenerator.GetTriColour(viewAccessP, Color.Gray, Color.Beige, Color.Brown);
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
                    double privacyV = resultData3.PredictedPrivacy;
                    if (resultData3.WindowAreaSum == 0)
                    { privacyColor = Color.DarkGray; }
                    else if ((privacyV >= -5) && (privacyV <= 5))
                    {
                        double privacyP = ColorGenerator.Remap(privacyV, -5, 5, 0, 1);
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
            get { return new Guid("417BE727-FAE1-48A0-9A2A-F44D734A39F5"); }
        }
    }
}