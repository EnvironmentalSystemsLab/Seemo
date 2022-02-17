using System;
using System.Collections.Generic;


using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SeemoPredictor
{
    public class RoomWindowComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public RoomWindowComponent()
          : base("Room Snesor", "Room Sensor",
              "Analyzing View Setting : Windows, viewpoints, viewvectors in a room",
              "SeEmo", "1|Sensor")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Room", "Room", "Room box", GH_ParamAccess.item);
            pManager.AddBrepParameter("Windows", "Windows", "Window surfaces", GH_ParamAccess.list);
            pManager.AddPointParameter("View Points", "View Points", "View Points", GH_ParamAccess.list);
            pManager.AddVectorParameter("Option_View vectors", "Option_View Vectors", "Option_Normalized view vectors for each view point", GH_ParamAccess.list);
            //pManager.AddIntegerParameter("Option_Analyzing Resolution", "Option_Analyzing Resolution", "Option_Analyzing Resolution", GH_ParamAccess.item);
            //pManager.AddNumberParameter("Option_HorizontalSceneAngle", "Option_HorizontalSceneAngle", "Option_HorizontalSceneAngle", GH_ParamAccess.item);
            //pManager.AddNumberParameter("Option_VerticalSceneAngle", "Option_VerticalSceneAngle", "Option_VerticalSceneAngle", GH_ParamAccess.item);

            pManager[3].Optional = true;
            //pManager[4].Optional = true;
            //pManager[5].Optional = true;
            //pManager[6].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Room Sensor", "Room Sensor", "Room Sensor", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Mesh Room = new Mesh();
            Mesh Room = new Mesh();
            List<Brep> WindowBreps = new List<Brep>();
            List<Point3d> ViewPoints = new List<Point3d>();
            List<Vector3d> ViewVectors = new List<Vector3d>();
            int Resolution = 300;
            double HorizontalSceneAngle = (35.754 * 2);
            double VerticalSceneAngle = (25.641 * 2);
            Point3d vvp = new Point3d(0, 0, 0);

            
            //rotate vector to clockwise from 12 
            
            

            if (!DA.GetData(0, ref Room)) return;
            if (!DA.GetDataList(1, WindowBreps)) return;
            if (!DA.GetDataList(2, ViewPoints)) return;
            if (!DA.GetDataList(3, ViewVectors)) {
                
                for (int i = 0; i < 8; i++)
                {
                    Vector3d v1 = new Vector3d(0, 1, 0);
                    Transform rocw45 = Transform.Rotation(-0.25* i * (Math.PI), vvp);
                    v1.Transform(rocw45);
                    ViewVectors.Add(v1);
                }
            }
            
            //DA.GetData(4, ref Resolution);
            //DA.GetData(5, ref HorizontalSceneAngle);
            //DA.GetData(6, ref VerticalSceneAngle);

            RoomSensor roomSensor = new RoomSensor(Room, WindowBreps, ViewPoints, ViewVectors, Resolution, HorizontalSceneAngle, VerticalSceneAngle);

            //RoomSensor(Mesh _room, List < Brep > _windows, List < Point3d > _pts, List < Vector3d > _vecs)

            DA.SetData(0, roomSensor);


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
            get { return new Guid("434386FE-9416-414B-BA92-8C149EA75AC1"); }
        }
    }
}