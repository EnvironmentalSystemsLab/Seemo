using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using SeemoPredictor.Geometry;
using Rhino.DocObjects;

namespace SeemoPredictor
{
    public class SmoSensorComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public SmoSensorComponent()
          : base("Sensor", "Sensor",
              "Analyzing View Setting : viewpoints, viewvectors",
              "SeEmo", "1|Sensor")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            
            pManager.AddPointParameter("Point", "Pt", "View Point", GH_ParamAccess.item);
            pManager.AddVectorParameter("Vectors", "Vec", "Optional view vectors", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Resolution", "Res", "View Resolution in pixels along z", GH_ParamAccess.item);
            //pManager.AddNumberParameter("Option_HorizontalSceneAngle", "Option_HorizontalSceneAngle", "Option_HorizontalSceneAngle", GH_ParamAccess.item);
            //pManager.AddNumberParameter("Option_VerticalSceneAngle", "Option_VerticalSceneAngle", "Option_VerticalSceneAngle", GH_ParamAccess.item);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            //pManager[5].Optional = true;
            //pManager[6].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Sensor", "Sensor", "Sensor", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            
           Point3d ViewPoint = Point3d.Origin;
            List<Vector3d> ViewVectors = new List<Vector3d>();
            List<Point3> SViewPoints = new List<Point3>();
            List<Point3> SViewVectors = new List<Point3>();
            
           
            Point3d vvp = new Point3d(0, 0, 0);

            
            //rotate vector to clockwise from 12 
            
            if (!DA.GetData(0, ref ViewPoint)) return;
            if (!DA.GetDataList(1, ViewVectors)) {
                
                for (int i = 0; i < 8; i++)
                {
                    Vector3d v1 = new Vector3d(0, 1, 0);
                    Transform rocw45 = Transform.Rotation(-0.25* i * (Math.PI), vvp);
                    v1.Transform(rocw45);
                    ViewVectors.Add(v1);
                    
                }
            }

            //DA.GetData(5, ref HorizontalSceneAngle);
            //DA.GetData(6, ref VerticalSceneAngle);


 
                Point3 sp = new Point3(ViewPoint.X, ViewPoint.Y, ViewPoint.Z);
             

            //Convert Vector3d to SmoPoint3
            foreach (Vector3d vt in ViewVectors)
            {
                Point3 sv = new Point3(vt.X, vt.Y, vt.Z);
                SViewVectors.Add(sv);
            }


            int xResolution = 1440;

            if (!DA.GetData(2, ref xResolution)) return;

            double HorizontalSceneAngle = (35.754 * 2);
            double VerticalSceneAngle = (25.641 * 2);

            SmoSensor smoSensor = new SmoSensor(sp, SViewVectors, xResolution, HorizontalSceneAngle, VerticalSceneAngle);

            DA.SetData(0, smoSensor);




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
                return Properties.Resources.sensor;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("434386FE-9416-414B-BA92-8C149EA75AC1"); }
        }
    }
}