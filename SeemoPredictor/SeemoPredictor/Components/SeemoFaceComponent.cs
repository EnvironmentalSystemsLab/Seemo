using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino.DocObjects;
using Rhino.Geometry;
using SeemoPredictor.Geometry;
using Rhino.Render;

namespace SeemoPredictor
{
    public class SeemoFaceComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public SeemoFaceComponent()
          : base("SeemoFace", "SF",
              "SeemoFace",
              "SeEmo", "2|Environment")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Geometry", "G", "Geometry as Meshes", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Type", "T", "View Content Geometry Type", GH_ParamAccess.item, 0);
            var types = Enum.GetNames(typeof(SmoFace.SmoFaceType));
            Param_Integer param = pManager[1] as Param_Integer;
            for (int i = 0; i < types.Length; i++)
            {
                param.AddNamedValue(types[i], i);
            }

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Faces", "F", "Seemo Faces", GH_ParamAccess.list);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Mesh> rhinogeo = new List<Mesh>();
            List<SmoFace> faces = new List<SmoFace>();

            DA.GetDataList(0, rhinogeo);

            int ftype = 0;
            if (!DA.GetData(1, ref ftype)) return;
            SmoFace.SmoFaceType facetype = (SmoFace.SmoFaceType)ftype;

            this.Message = facetype.ToString();

            foreach (var m in rhinogeo)
            {
                faces.AddRange(Mesh2SmoFaces.MeshToSmoFaces(m, facetype));
            }

            DA.SetDataList(0, faces);





            ////////////////for gpu
            //generate material with material.ID
            Material matType = new Material();
            matType.Name = facetype.ToString();


            //combine List<mesh>into one mesh
            Mesh oneMesh = new Mesh();
            foreach(Mesh m in rhinogeo)
            {
                oneMesh.Append(m);
            }

            //define mesh.material 
            ObjectAttributes attr = new ObjectAttributes();
            attr.Name = facetype.ToString();
            attr.


            //da.writeData 

            RhinoObject obj;
            obj = oneMesh;

                //new ObjectAttributes(Material(matType));


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
                return Properties.Resources.seemoFace;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{78718120-0F12-4925-85B0-C26A18871A2C}"); }
        }
    }
}