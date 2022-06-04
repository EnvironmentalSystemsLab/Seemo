using System;
using System.Collections.Generic;
using SeemoPredictor.Geometry;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.FileIO;
using System.IO;


namespace SeemoPredictor.Components
{
    public class ObjExportComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ObjExportComponent class.
        /// </summary>
        public ObjExportComponent()
          : base("ObjExportComponent", "Nickname",
              "Description",
              "Category", "Subcategory")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Run", "Run", "Run", GH_ParamAccess.item);
            pManager.AddMeshParameter("SmoMesh", "M", "Seemo Meshes", GH_ParamAccess.list);
            pManager.AddTextParameter("Environment", "Env", "Environment Name", GH_ParamAccess.item);

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

            List<Mesh> meshes = new List<Mesh>();

            Boolean run = false;
            string envName = "default";

            if (!DA.GetData(0, ref run)) return;
            if (run == false) return;
            DA.GetDataList(1, meshes);
            if (!DA.GetData(2, ref envName)) return;


            //save rendering into bmp
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string dir = (path + @"\Seemo\");

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string filename = dir + envName + ".obj";

            var options = new Rhino.FileIO.FileObjWriteOptions(new FileWriteOptions { IncludeHistory = true, WriteUserData = true })
            {
                MapZtoY = true,
                ExportAsTriangles = true,
                ExportNormals = true,
                ExportOpenMeshes = true,
                ExportMaterialDefinitions = true,
                UseSimpleDialog = true,
                MeshParameters = MeshingParameters.Default
            };

            var result = FileObj.Write(filename, meshes.ToArray(), options);

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
            get { return new Guid("0C4C918F-ABE8-4A7E-BBF4-B09AD19E9592"); }
        }
    }
}