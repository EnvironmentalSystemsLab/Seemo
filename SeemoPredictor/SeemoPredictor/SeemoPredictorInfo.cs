using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace SeemoPredictor
{
    public class SeemoPredictorInfo : GH_AssemblyInfo
    {
        public override string Name => "SeemoPredictor";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("C2D1440F-3C74-4487-B6C7-747CE1F40932");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}