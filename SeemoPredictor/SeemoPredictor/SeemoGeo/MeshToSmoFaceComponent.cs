using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SeemoPredictor.SeemoGeo
{
    public class MeshToSmoFaceComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the BrepToSmoFaceComponent class.
        /// </summary>
        public MeshToSmoFaceComponent()
          : base("MeshToSmoFace", "SmoFace",
              "Change Mesh to SmoFace",
              "SeEmo", "0|Setting")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "Mesh", "Mesh", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("SmoFace", "SmoFace", "SmoFace", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            Mesh m = new Mesh();
            List<SmoFace> smoFaces = new List<SmoFace>();
            DA.GetData(0, ref m);


            for(int i = 0; i < m.Faces.Count; i++)
            {
                
                SmoPoint3[] pts;

                var a = m.Vertices[m.Faces[i].A];
                var b = m.Vertices[m.Faces[i].B];
                var c = m.Vertices[m.Faces[i].C];
                SmoPoint3 p0 = new SmoPoint3(a.X, a.Y, a.Z);
                SmoPoint3 p1 = new SmoPoint3(b.X, b.Y, b.Z);
                SmoPoint3 p2 = new SmoPoint3(c.X, c.Y, c.Z);

                if (m.Faces[i].IsQuad)
                {
                    pts = new SmoPoint3[4];
                    var d = m.Vertices[m.Faces[i].D];
                    SmoPoint3 p3 = new SmoPoint3(d.X, d.Y, d.Z);
                    pts[0] = p0;
                    pts[1] = p1;
                    pts[2] = p2;
                    pts[3] = p3;
                }
                else
                {
                    pts = new SmoPoint3[3];
                    pts[0] = p0;
                    pts[1] = p1;
                    pts[2] = p2;
                }

                SmoFace smoFace = new SmoFace(pts);
                smoFaces.Add(smoFace);
                
            }



            //make SmoOctree
            //SmoPointOctree<SmoFace> octree = new SmoPointOctree<SmoFace>((float)maxNodeSize, smoFaces[0].Center, (float)minNodeSize);
            SmoPointOctree<SmoFace> octree1 = new SmoPointOctree<SmoFace>(0.5f, smoFaces[0].Center, 0.01f);

            for (int i = 0; i < smoFaces.Count; i++)
            {
                var f = smoFaces[i];
                octree1.Add(f, f.Center);
            }


            //DA.SetDataList(0, smoFacess);

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
            get { return new Guid("24C923D0-9F98-43A1-B436-5471E0007212"); }
        }
    }
}


/*
 * List<Mesh> meshes = new List<Mesh>();
    List<List<SmoFace>> smoFacess = new List<List<SmoFace>>();
    DA.GetDataList(0, meshes);


    for(int i = 0; i < meshes.Count; i++)
    {
        Mesh m = meshes[i];
        List<SmoFace> smoFaces = new List<SmoFace>();


        //convert meshfaces into smoFaces
        for(int j = 0; j < m.Faces.Count; j++)
        {
            SmoPoint3[] pts;

            var a = m.Vertices[m.Faces[j].A];
            var b = m.Vertices[m.Faces[j].B];
            var c = m.Vertices[m.Faces[j].C];
            SmoPoint3 p0 = new SmoPoint3(a.X, a.Y, a.Z);
            SmoPoint3 p1 = new SmoPoint3(b.X, b.Y, b.Z);
            SmoPoint3 p2 = new SmoPoint3(c.X, c.Y, c.Z);

            if (m.Faces[j].IsQuad)
            {
                pts = new SmoPoint3[4];
                var d = m.Vertices[m.Faces[j].D];
                SmoPoint3 p3 = new SmoPoint3(d.X, d.Y, d.Z);
                pts[0] = p0;
                pts[1] = p1;
                pts[2] = p2;
                pts[3] = p3;
            }
            else
            {
                pts = new SmoPoint3[3];
                pts[0] = p0;
                pts[1] = p1;
                pts[2] = p2;
            }

            SmoFace smoFace = new SmoFace(pts);
            smoFaces.Add(smoFace);
        }

        smoFacess.Add(smoFaces);
    }
*/
