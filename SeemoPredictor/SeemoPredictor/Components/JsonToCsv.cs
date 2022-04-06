using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using GroupDocs.Conversion;
using GroupDocs.Conversion.Options.Convert;
using GroupDocs.Conversion.FileTypes;
using System.IO;

namespace SeemoPredictor.Components
{
    public class JsonToCsv : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the JsonToCsv class.
        /// </summary>
        public JsonToCsv()
          : base("JsonToCsv", "J2C",
                "JsonToCsv", "SeEmo", "6|Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("JSON File Path", "JSON", "JSON File Path", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("CSV File Path", "CSV", "CSV File Path", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string jsonpath = "";
            if (!DA.GetData(0, ref jsonpath)) { return; }

            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string dir = (path + @"\Seemo");

            if (!System.IO.Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }


            jsonpath = Path.GetFullPath(jsonpath);
            string csvPath = System.IO.Path.ChangeExtension(jsonpath, ".csv");

            using (Converter converter = new Converter(@jsonpath))
            {
                SpreadsheetConvertOptions options = new SpreadsheetConvertOptions()
                {
                    Format = SpreadsheetFileType.Csv
                };

                DA.SetData(0, csvPath);

                converter.Convert(csvPath, options);
            }

            
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
            get { return new Guid("4F9A53BD-A96B-4FA1-8CB8-0BA972D676D4"); }
        }
    }
}