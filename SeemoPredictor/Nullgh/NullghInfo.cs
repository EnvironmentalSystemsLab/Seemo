using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace Nullgh
{
    public class NullghInfo : GH_AssemblyInfo
    {
        public override string Name => "Nullgh";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("F3D4E4E1-C89C-4DDB-A5C6-EBD5785D52A0");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}