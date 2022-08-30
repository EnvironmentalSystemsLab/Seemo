using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using System.Linq;
using System.Drawing.Imaging;

namespace SeemoPredictor
{
    public class VisualizationComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the VisualizationComponent class.
        /// </summary>
        public VisualizationComponent()
          : base("Visualizer", "Visualizer",
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
            pManager.AddNumberParameter("Graph Size", "Size", "Graph Size", GH_ParamAccess.item);
            pManager.AddNumberParameter("Rec Size", "Rec Size", "Rec Size", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Overall Rating Graph", "Overall Rating Graph", "Overall Rating Graph", GH_ParamAccess.list);
            pManager.AddMeshParameter("View Content Pie Graph", "View Content Pie Graph", "View Content Pie Graph", GH_ParamAccess.list);
            pManager.AddMeshParameter("View Content Pixel Graph", "View Content Pixel Graph", "View Content Pixel Graph", GH_ParamAccess.list);
            pManager.AddMeshParameter("View Access Graph", "View Access Graph", "View Access Graph", GH_ParamAccess.list);
            pManager.AddMeshParameter("Privacy Graph", "Privacy Graph", "Privacy Graph", GH_ParamAccess.list);
            pManager.AddMeshParameter("Framework Graph", "Framework Graph", "Framework Graph", GH_ParamAccess.list);
            pManager.AddMeshParameter("S-PVEI Pie Graph", "S-PVEI Pie Graph", "S-PVEI Pie Graph", GH_ParamAccess.list);
            pManager.AddMeshParameter("S-PVEI Pixel Graph", "S-PVEI Pixel Graph", "S-PVEI Pixel Graph", GH_ParamAccess.list);
            pManager.AddNumberParameter("OverallRating", "OverallRating", "OverallRating", GH_ParamAccess.item);
            pManager.AddNumberParameter("View Content", "View Content", "View Content", GH_ParamAccess.item);
            pManager.AddNumberParameter("View Content Pixel", "View Content Pixel", "View Content Pixel", GH_ParamAccess.item);
            pManager.AddNumberParameter("View Access", "View Access", "View Access", GH_ParamAccess.item);
            pManager.AddNumberParameter("Privacy", "Privacy", "Privacy", GH_ParamAccess.item);
            pManager.AddNumberParameter("Framework", "Framework", "Framework", GH_ParamAccess.item);
            pManager.AddNumberParameter("S-PVEI", "S-PVEI", "S-PVEI", GH_ParamAccess.item);
            pManager.AddNumberParameter("S-PVEI Pixel", "S-PVEI Pixel", "S-PVEI Pixel", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string path = "";
            double scale = 1;
            if (!DA.GetData(0, ref path)) { return; }
            DA.GetData(1, ref scale);
            double scale2 = 1;
            DA.GetData(2, ref scale2);

            if (!File.Exists(path))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Result file does not exsist");
                return;
            }

            SeemoResult result = SeemoResult.FromFile(path);


            List<Mesh> overallRatingGraphs = new List<Mesh>();
            List<Mesh> viewContentGraphs = new List<Mesh>();
            List<Mesh> viewContentPixelGraphs = new List<Mesh>();
            List<Mesh> viewAccessGraphs = new List<Mesh>();
            List<Mesh> privacyGraphs = new List<Mesh>();
            List<Mesh> frameworkGraphs = new List<Mesh>();
            List<Mesh> SPVEIGraphs = new List<Mesh>();
            List<Mesh> SPVEIPixelGraphs = new List<Mesh>();
            List<double> overalls = new List<double>();
            List<double> contents = new List<double>();
            List<double> contentPixels = new List<double>();
            List<double> accesses = new List<double>();
            List<double> privacys = new List<double>();
            List<double> frameworks = new List<double>();
            List<double> SPVEIs = new List<double>();
            List<double> SPVEIPixels = new List<double>();

            bool isMeshSensor = false;
            if(result.Results[0].Vert0 != result.Results[0].Vert1)
            {
                isMeshSensor = true;
            }


            //Wind Rose Mesh Generation

            for (int i = 0; i < result.Results.Count; i++)
            {
                // i is node index
                Point3d viewPoint = new Point3d(result.Results[i].Pt.X, result.Results[i].Pt.Y, result.Results[i].Pt.Z);

                
                //3.ViewContentPixel
                double viewContentPixel = -5;
                double windowAreaRatioViewContent = 0;

                //8.SPVEIPixel
                double SPVEIVPixel = -1;
                double windowAreaRatioSPVEIV = 0;

                for (int j = 0; j < result.Results[i].DirectionsResults.Count; j++)
                {
                    //j is vector index
                    DirectionResult resultData3 = result.Results[i].DirectionsResults[j];
                    Point3d viewVector = new Point3d(resultData3.ViewVectorX, resultData3.ViewVectorY, resultData3.ViewVectorZ);

                    Transform rocw25 = Transform.Rotation(-0.125 * (Math.PI), viewPoint);
                    Transform roccw25 = Transform.Rotation(0.125 * (Math.PI), viewPoint);
                    Point3d p1 = new Point3d(viewPoint + (scale * 0.3 * viewVector / (Math.Sin(0.125 * (Math.PI)) + Math.Cos(0.125 * (Math.PI)))));
                    Point3d p2 = p1;
                    Point3d p3 = new Point3d(viewPoint + scale * 0.3 * viewVector);
                    p1.Transform(rocw25);
                    p2.Transform(roccw25);

                    //1.overall Rating
                    Color overallRatingColor;
                    double overallRatingV = resultData3.PredictedOverallRating;

                    if (resultData3.WindowAreaRatio <= 0.0)
                    {   
                        overallRatingColor = Color.Black;
                        overalls.Add(-5);
                    }
                    else if ((overallRatingV >= -5) && (overallRatingV <= 5))
                    {
                        overalls.Add(overallRatingV);
                        double overallRatingP = ColorGenerator.Remap(overallRatingV, -5, 5, 0, 1);
                        Color P1 = Color.FromArgb(255, 215, 0, 56);
                        Color P2 = Color.FromArgb(255, 255, 255, 210);
                        Color P3 = Color.FromArgb(255, 0, 139, 225);
                        overallRatingColor = ColorGenerator.GetTriColour(overallRatingP, P1, P2, P3);
                    }
                    else
                    {   
                        overallRatingColor = Color.Black;
                        overalls.Add(-5);
                    }


                    Mesh overallRatingPetal = new Mesh();
                    overallRatingPetal.Vertices.Add(viewPoint);
                    overallRatingPetal.Vertices.Add(p1);
                    overallRatingPetal.Vertices.Add(p2);

                    overallRatingPetal.Faces.AddFace(0, 1, 2);

                    overallRatingPetal.VertexColors.SetColor(0, overallRatingColor);
                    overallRatingPetal.VertexColors.SetColor(1, overallRatingColor);
                    overallRatingPetal.VertexColors.SetColor(2, overallRatingColor);

                    overallRatingPetal.Normals.ComputeNormals();

                    overallRatingGraphs.Add(overallRatingPetal);


                    //2.viewContent
                    Color viewContentColor;
                    double viewContentV = resultData3.PredictedViewContent;
                    if (resultData3.WindowAreaRatio <= 0.0)
                    { 
                        viewContentColor = Color.Black;
                        contents.Add(-5);
                    }
                    else if ((viewContentV >= -5) && (viewContentV <= 5))
                    {
                        contents.Add(viewContentV);
                        var remap = ColorGenerator.Remap(-viewContentV, -5, 5, 0, 1);  //original ColorGenerator.Remap(val, min, max, 0, 1)
                        viewContentColor = ColorGenerator.Turbo.ReturnTurboColor(remap);
                    }
                    else
                    { 
                        viewContentColor = Color.Black;
                        contents.Add(-5);
                    }

                    Mesh viewContentPetal = new Mesh();
                    viewContentPetal.Vertices.Add(viewPoint);
                    viewContentPetal.Vertices.Add(p1);
                    viewContentPetal.Vertices.Add(p2);

                    viewContentPetal.Faces.AddFace(0, 1, 2);

                    viewContentPetal.VertexColors.SetColor(0, viewContentColor);
                    viewContentPetal.VertexColors.SetColor(1, viewContentColor);
                    viewContentPetal.VertexColors.SetColor(2, viewContentColor);

                    viewContentPetal.Normals.ComputeNormals();
                    viewContentGraphs.Add(viewContentPetal);

                    //3.ViewContent Pixel
                    double viewContentPixeltemp = resultData3.PredictedViewContent;
                    if (viewContentPixeltemp > viewContentPixel)
                    {
                        viewContentPixel = viewContentPixeltemp;
                        windowAreaRatioViewContent = resultData3.WindowAreaRatio;
                    }
                    
                    //4.viewAccess
                    Color viewAccessColor;
                    double viewAccessV = resultData3.PredictedViewAccess;
                    if (resultData3.WindowAreaRatio <= 0.0)
                    { 
                        viewAccessColor = Color.Black;
                        accesses.Add(-5);
                    }
                    else if ((viewAccessV >= -5) && (viewAccessV <= 5))
                    {
                        accesses.Add(viewAccessV);
                        double viewAccessP = ColorGenerator.Remap(viewAccessV, -5, 5, 0, 1);
                        viewAccessColor = ColorGenerator.GetTriColour(viewAccessP, Color.Red, Color.Yellow, Color.Blue);
                    }
                    else
                    { 
                        viewAccessColor = Color.Black;
                        accesses.Add(-5);
                    }

                    Mesh viewAccessPetal = new Mesh();
                    viewAccessPetal.Vertices.Add(viewPoint);
                    viewAccessPetal.Vertices.Add(p1);
                    viewAccessPetal.Vertices.Add(p2);

                    viewAccessPetal.Faces.AddFace(0, 1, 2);

                    viewAccessPetal.VertexColors.SetColor(0, viewAccessColor);
                    viewAccessPetal.VertexColors.SetColor(1, viewAccessColor);
                    viewAccessPetal.VertexColors.SetColor(2, viewAccessColor);

                    viewAccessPetal.Normals.ComputeNormals();
                    viewAccessGraphs.Add(viewAccessPetal);

                    //5.Privacy
                    Color privacyColor;
                    double privacyV = resultData3.PredictedPrivacy;
                    if (resultData3.WindowAreaRatio <= 0.0)
                    { 
                        privacyColor = Color.Black;
                        privacys.Add(-5);
                    }
                    else if ((privacyV >= -5) && (privacyV <= 5))
                    {
                        privacys.Add(privacyV);
                        var privacyP = ColorGenerator.Remap(privacyV, -5, 5, 0, 1);  //original ColorGenerator.Remap(val, min, max, 0, 1)
                        privacyColor = ColorGenerator.Inferno.ReturnInfernoColor(privacyP);
                    }
                    else
                    { 
                        privacyColor = Color.Black;
                        privacys.Add(-5);
                    }

                    Mesh privacyPetal = new Mesh();
                    privacyPetal.Vertices.Add(viewPoint);
                    privacyPetal.Vertices.Add(p1);
                    privacyPetal.Vertices.Add(p2);

                    privacyPetal.Faces.AddFace(0, 1, 2);

                    privacyPetal.VertexColors.SetColor(0, privacyColor);
                    privacyPetal.VertexColors.SetColor(1, privacyColor);
                    privacyPetal.VertexColors.SetColor(2, privacyColor);

                    privacyPetal.Normals.ComputeNormals();

                    privacyGraphs.Add(privacyPetal);

                    //6.Framework
                    Color frameworkColor;
                    double frameworkV = resultData3.ViewContentFramework;
                    if (resultData3.WindowAreaRatio <= 0.0 || viewContentColor == Color.Black )
                    { 
                        frameworkColor = Color.Black;
                        frameworks.Add(0);
                    }
                    else if ((frameworkV >= 0) && (frameworkV <= 1))
                    {
                        frameworks.Add(frameworkV);
                        double frameworkP = ColorGenerator.Remap(frameworkV, 0, 1, 0, 1);
                        Color P1 = Color.FromArgb(255, 255, 151, 82);
                        Color P2 = Color.FromArgb(255, 237, 244, 160);
                        Color P3 = Color.FromArgb(255, 152, 200, 117);
                        frameworkColor = ColorGenerator.GetTriColour(frameworkP, P1, P2, P3);
                    }
                    else
                    { 
                        frameworkColor = Color.Black;
                        frameworks.Add(0);
                    }

                    Mesh frameworkPetal = new Mesh();
                    frameworkPetal.Vertices.Add(viewPoint);
                    frameworkPetal.Vertices.Add(p1);
                    frameworkPetal.Vertices.Add(p2);

                    frameworkPetal.Faces.AddFace(0, 1, 2);

                    frameworkPetal.VertexColors.SetColor(0, frameworkColor);
                    frameworkPetal.VertexColors.SetColor(1, frameworkColor);
                    frameworkPetal.VertexColors.SetColor(2, frameworkColor);

                    frameworkPetal.Normals.ComputeNormals();

                    frameworkGraphs.Add(frameworkPetal);


                    //7.SPVEI
                    Color SPVEIColor;
                    double SPVEIV = resultData3.SPVEI;
                    if (resultData3.WindowAreaRatio <= 0.0)
                    { 
                        SPVEIColor = Color.Black;
                        SPVEIs.Add(0);
                    }
                    else if ((SPVEIV >= 0) && (SPVEIV <= 10))
                    {
                        if (SPVEIV <= 0.001) { SPVEIV = 0.001111; }
                        if (SPVEIV >= 0.1) { SPVEIV = 0.09999; }
                        SPVEIs.Add(SPVEIV);
                        double remap2 = ((Math.Log10(SPVEIV) / 2.0f) + 1.5f);

                        var remap = ColorGenerator.Remap(remap2, 0, 1, 0, 1);  //original ColorGenerator.Remap(val, min, max, 0, 1)
                        SPVEIColor = ColorGenerator.Inferno.ReturnInfernoColor(remap);

                    }else
                    {
                        SPVEIColor = Color.Black;
                        SPVEIs.Add(0);
                    }

                    Mesh SPVEIPetal = new Mesh();
                    SPVEIPetal.Vertices.Add(viewPoint);
                    SPVEIPetal.Vertices.Add(p1);
                    SPVEIPetal.Vertices.Add(p2);

                    SPVEIPetal.Faces.AddFace(0, 1, 2);

                    SPVEIPetal.VertexColors.SetColor(0, SPVEIColor);
                    SPVEIPetal.VertexColors.SetColor(1, SPVEIColor);
                    SPVEIPetal.VertexColors.SetColor(2, SPVEIColor);

                    SPVEIPetal.Normals.ComputeNormals();

                    SPVEIGraphs.Add(SPVEIPetal);


                    //8.SPVEI Pixel
                    double SPVEIVPixeltemp = resultData3.SPVEI;
                    if (SPVEIVPixeltemp > SPVEIVPixel)
                    {
                        SPVEIVPixel = SPVEIVPixeltemp;
                    }
                    if (resultData3.WindowAreaRatio > windowAreaRatioSPVEIV)
                    {
                        windowAreaRatioSPVEIV = resultData3.WindowAreaRatio;
                    }
                }

                Point3d pUpR = new Point3d(1, 1, 0);
                Point3d pUpL = new Point3d(-1, 1, 0);

                //3.ViewContent Pixel
                Color viewContentPixelColor;
                if (windowAreaRatioViewContent <= 0.0)
                { 
                    viewContentPixelColor = Color.Black;
                    contentPixels.Add(-5);
                }
                else if (viewContentPixel == double.NaN)
                {
                    viewContentPixelColor = Color.Black;
                    contentPixels.Add(-5);
                }
                else //if ((SPVEIV >= 0) && (SPVEIV <= 1))
                {
                    if (viewContentPixel <= -5) { viewContentPixel = -5; }
                    if (viewContentPixel >= 5) { viewContentPixel = 5; }
                    contentPixels.Add(viewContentPixel);

                    var remap = ColorGenerator.Remap(-viewContentPixel, -5, 5, 0, 1);  //original ColorGenerator.Remap(val, min, max, 0, 1)
                    viewContentPixelColor = ColorGenerator.Turbo.ReturnTurboColor(remap);
                }

                Mesh viewContentPixelPetal = new Mesh();

                if (isMeshSensor)
                {
                    
                    Point3d vert0 = new Point3d(result.Results[i].Vert0.X, result.Results[i].Vert0.Y, result.Results[i].Vert0.Z);
                    Point3d vert1 = new Point3d(result.Results[i].Vert1.X, result.Results[i].Vert1.Y, result.Results[i].Vert1.Z);
                    Point3d vert2 = new Point3d(result.Results[i].Vert2.X, result.Results[i].Vert2.Y, result.Results[i].Vert2.Z);
                    Point3d vert3 = new Point3d(result.Results[i].Vert3.X, result.Results[i].Vert3.Y, result.Results[i].Vert3.Z);
                    
                    viewContentPixelPetal.Vertices.Add(vert0);
                    viewContentPixelPetal.Vertices.Add(vert1);
                    viewContentPixelPetal.Vertices.Add(vert2);
                    viewContentPixelPetal.Vertices.Add(vert3);
                }
                else
                {
                    viewContentPixelPetal.Vertices.Add(viewPoint + pUpR * scale2 * (0.3));
                    viewContentPixelPetal.Vertices.Add(viewPoint + pUpL * scale2 * (0.3));
                    viewContentPixelPetal.Vertices.Add(viewPoint + pUpR * scale2 * (-0.3));
                    viewContentPixelPetal.Vertices.Add(viewPoint + pUpL * scale2 * (-0.3));
                }
               

                viewContentPixelPetal.Faces.AddFace(0, 1, 2, 3);

                viewContentPixelPetal.VertexColors.SetColor(0, viewContentPixelColor);
                viewContentPixelPetal.VertexColors.SetColor(1, viewContentPixelColor);
                viewContentPixelPetal.VertexColors.SetColor(2, viewContentPixelColor);
                viewContentPixelPetal.VertexColors.SetColor(3, viewContentPixelColor);

                viewContentPixelPetal.Normals.ComputeNormals();
                viewContentPixelGraphs.Add(viewContentPixelPetal);

                //8.SPVEI Pixel
                Color SPVEIPixelColor;
                
                if (windowAreaRatioSPVEIV <= 0.0)
                { 
                    SPVEIPixelColor = Color.Black;
                    SPVEIPixels.Add(0);
                }
                else if ((SPVEIVPixel >= 0) && (SPVEIVPixel <= 10))
                {
                    if (SPVEIVPixel <= 0.001) { SPVEIVPixel = 0.001111; }
                    if (SPVEIVPixel >= 0.1) { SPVEIVPixel = 0.09999; }
                    SPVEIPixels.Add(SPVEIVPixel);
                    double remap2 = ((Math.Log10(SPVEIVPixel) / 2.0f) + 1.5f);

                    var remap = ColorGenerator.Remap(remap2, 0, 1, 0, 1);  //original ColorGenerator.Remap(val, min, max, 0, 1)
                    SPVEIPixelColor = ColorGenerator.Inferno.ReturnInfernoColor(remap);

                }
                else
                {
                    SPVEIPixels.Add(0);
                    SPVEIPixelColor = Color.Black;
                }

                Mesh SPVEIPixelPetal = new Mesh();

                if (isMeshSensor)
                {

                    Point3d vert0 = new Point3d(result.Results[i].Vert0.X, result.Results[i].Vert0.Y, result.Results[i].Vert0.Z);
                    Point3d vert1 = new Point3d(result.Results[i].Vert1.X, result.Results[i].Vert1.Y, result.Results[i].Vert1.Z);
                    Point3d vert2 = new Point3d(result.Results[i].Vert2.X, result.Results[i].Vert2.Y, result.Results[i].Vert2.Z);
                    Point3d vert3 = new Point3d(result.Results[i].Vert3.X, result.Results[i].Vert3.Y, result.Results[i].Vert3.Z);

                    SPVEIPixelPetal.Vertices.Add(vert0);
                    SPVEIPixelPetal.Vertices.Add(vert1);
                    SPVEIPixelPetal.Vertices.Add(vert2);
                    SPVEIPixelPetal.Vertices.Add(vert3);
                }
                else
                {
                    SPVEIPixelPetal.Vertices.Add(viewPoint + pUpR * scale2 * (0.3));
                    SPVEIPixelPetal.Vertices.Add(viewPoint + pUpL * scale2 * (0.3));
                    SPVEIPixelPetal.Vertices.Add(viewPoint + pUpR * scale2 * (-0.3));
                    SPVEIPixelPetal.Vertices.Add(viewPoint + pUpL * scale2 * (-0.3));
                }

                SPVEIPixelPetal.Faces.AddFace(0, 1, 2, 3);

                SPVEIPixelPetal.VertexColors.SetColor(0, SPVEIPixelColor);
                SPVEIPixelPetal.VertexColors.SetColor(1, SPVEIPixelColor);
                SPVEIPixelPetal.VertexColors.SetColor(2, SPVEIPixelColor);
                SPVEIPixelPetal.VertexColors.SetColor(3, SPVEIPixelColor);

                SPVEIPixelPetal.Normals.ComputeNormals();
                SPVEIPixelGraphs.Add(SPVEIPixelPetal);
            }


            DA.SetDataList(0, overallRatingGraphs);
            DA.SetDataList(1, viewContentGraphs);
            DA.SetDataList(2, viewContentPixelGraphs);
            DA.SetDataList(3, viewAccessGraphs);
            DA.SetDataList(4, privacyGraphs);
            DA.SetDataList(5, frameworkGraphs);
            DA.SetDataList(6, SPVEIGraphs);
            DA.SetDataList(7, SPVEIPixelGraphs);

            double o = overalls.Average();
            double c = contents.Average();
            double cp = contentPixels.Average();
            double a = accesses.Average();
            double p = privacys.Average();
            double f = frameworks.Average();
            double v = SPVEIs.Average();
            double vp = SPVEIPixels.Average();
            DA.SetData(8, o);
            DA.SetData(9, c);
            DA.SetData(10, cp);
            DA.SetData(11, a);
            DA.SetData(12, p);
            DA.SetData(13, f);
            DA.SetData(14, v);
            DA.SetData(15, vp);
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
                return Properties.Resources.visualizer;
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