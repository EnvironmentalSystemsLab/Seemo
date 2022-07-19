using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using SeemoPredictor.Geometry;
using Rhino.DocObjects;

namespace SeemoPredictor.Components
{
    public class SmoMeshsensorComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SmoMeshsensroComponent class.
        /// </summary>
        public SmoMeshsensorComponent()
          : base("Mesh Sensor", "Mesh Sensor",
              "Analyzing View Setting from Mesh : viewpoints, viewvectors",
              "SeEmo", "1|Sensor")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>  
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {

            pManager.AddMeshParameter("Sensor Mesh", "Mesh", "Sensor Mesh", GH_ParamAccess.item);
            pManager.AddVectorParameter("Vectors", "Vec", "Optional view vectors", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Resolution", "Res", "View Resolution in pixels along z", GH_ParamAccess.item);

            pManager[1].Optional = true;
            pManager[2].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Sensor", "Sensor", "Sensor", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh sensorPlane = new Mesh();
            List<Vector3d> ViewVectors = new List<Vector3d>();
            List<Point3> SViewVectors = new List<Point3>();
            List<SmoSensor> sensors = new List<SmoSensor>();

            Point3d vvp = new Point3d(0, 0, 0);


            //rotate vector to clockwise from 12 

            if (!DA.GetData(0, ref sensorPlane)) return;
            if (!DA.GetDataList(1, ViewVectors))
            {

                for (int i = 0; i < 8; i++)
                {
                    Vector3d v1 = new Vector3d(0, 1, 0);
                    Transform rocw45 = Transform.Rotation(-0.25 * i * (Math.PI), vvp);
                    v1.Transform(rocw45);
                    ViewVectors.Add(v1);
                }
            }


            int xResolution = 1440;

            if (!DA.GetData(2, ref xResolution)) return;

            double HorizontalSceneAngle = (35.754 * 2);
            double VerticalSceneAngle = (25.641 * 2);


            for (int i = 0; i < sensorPlane.Faces.Count; i++)
            {
                Point3f vert0f = sensorPlane.Vertices[(int)sensorPlane.Faces[i].A];
                Point3f vert1f = sensorPlane.Vertices[(int)sensorPlane.Faces[i].B];
                Point3f vert2f = sensorPlane.Vertices[(int)sensorPlane.Faces[i].C];
                Point3f vert3f = sensorPlane.Vertices[(int)sensorPlane.Faces[i].D];

                
                Point3 vert0 = new Point3(vert0f.X, vert0f.Y, vert0f.Z);
                Point3 vert1 = new Point3(vert1f.X, vert1f.Y, vert1f.Z);
                Point3 vert2 = new Point3(vert2f.X, vert2f.Y, vert2f.Z);
                Point3 vert3 = new Point3(vert3f.X, vert3f.Y, vert3f.Z);

                Point3 sp = (vert0 + vert1 + vert2 + vert3) / 4;

                //Convert Vector3d to SmoPoint3
                foreach (Vector3d vt in ViewVectors)
                {
                    Point3 sv = new Point3(vt.X, vt.Y, vt.Z);
                    SViewVectors.Add(sv);
                }

                SmoSensor smoSensor = new SmoSensor(sp, SViewVectors, vert0, vert1, vert2, vert3, xResolution, HorizontalSceneAngle, VerticalSceneAngle);
                sensors.Add(smoSensor);

            }

            DA.SetDataList(0, sensors);

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
            get { return new Guid("72C32FEF-0499-4C50-9123-F0852610C2CE"); }
        }
    }
}