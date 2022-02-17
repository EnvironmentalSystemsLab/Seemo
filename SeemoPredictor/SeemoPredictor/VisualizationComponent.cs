using System;
using System.Collections.Generic;
using System.Drawing;

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
            pManager.AddGenericParameter("ViewResult", "ViewResult", "ViewResult", GH_ParamAccess.tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("View Graph", "View Graph", "View Graph", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<Grasshopper.Kernel.Types.IGH_Goo> dataTree1;
            DataTree<Mesh> graphs = new DataTree<Mesh>();
            //DataTree<ResultDataSet> dataTree2 = new DataTree<ResultDataSet>();
            //List<double> overallRatings = new List<double>();


            if (!DA.GetDataTree(0, out dataTree1)) { return; }


            //Wind Rose Mesh Generation

            foreach (GH_Path p in dataTree1.Paths)
            {
                foreach (Grasshopper.Kernel.Types.IGH_Goo o in dataTree1.get_Branch(p))
                {
                    if (o != null)
                    {
                        ResultDataSet r1;

                        //if (item.CastTo(out r1)) { continue; }
                        o.CastTo(out r1);

                        Point3d viewPoint = new Point3d(r1.ViewPointX, r1.ViewPointY, r1.ViewPointZ);
                        Point3d viewVector = new Point3d(r1.ViewVectorX, r1.ViewVectorY, r1.ViewVectorZ);
                        Transform rocw25 = Transform.Rotation(-0.125 * (Math.PI), viewPoint);
                        Transform roccw25 = Transform.Rotation(0.125 * (Math.PI), viewPoint);
                        Point3d p1 = new Point3d(viewPoint + 0.3 * viewVector);
                        Point3d p2 = p1;
                        p1.Transform(rocw25);
                        p2.Transform(roccw25);

                        //int prediction = (int)Math.Ceiling((r1.PredictedOverallRating + 5) / 10);
                        Color overallRatingColor;
                        double overallRating = r1.PredictedOverallRating;
                        if((overallRating >= -5) && (overallRating <= 5))
                        {
                            double overallRatingP = ColorGenerator.Remap(overallRating, -5, 5, 0, 1);
                            overallRatingColor = ColorGenerator.GetTriColour(overallRatingP, Color.Gray, Color.Orange, Color.Green);
                        }
                        else
                        {
                            overallRatingColor = Color.DarkGray;
                        }
                        

                        Mesh petal = new Mesh();
                        petal.Vertices.Add(viewPoint);
                        petal.Vertices.Add(p1);
                        petal.Vertices.Add(p2);

                        petal.Faces.AddFace(0, 1, 2);

                        petal.VertexColors.SetColor(0, overallRatingColor);
                        petal.VertexColors.SetColor(1, overallRatingColor);
                        petal.VertexColors.SetColor(2, overallRatingColor);

                        petal.Normals.ComputeNormals();
                        //petal.FaceNormals.ComputeFaceNormals();


                        GH_Path r1path = p;
                        graphs.Add(petal, r1path);

                    }
                }
            }
            
            DA.SetDataTree(0, graphs);

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