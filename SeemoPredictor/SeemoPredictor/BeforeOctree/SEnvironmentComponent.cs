/*using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SeemoPredictor
{
    public class SEnvironmentComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public SEnvironmentComponent()
          : base("EnvironmentComponent", "Environment",
              "Environment obects : Buildings, Nature, Pavement, Green, Dynamics, Sky Sphere",
              "SeEmo", "2|Environment")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Buildings", "Buildings", "Artificial Object Meshes", GH_ParamAccess.list);
            pManager.AddMeshParameter("Equipment", "Equipment", "Equipment Object Meshes", GH_ParamAccess.list);
            pManager.AddMeshParameter("Nature", "Nature", "Nature Object Meshes(Trees, Bushes, Mountains)", GH_ParamAccess.list);
            pManager.AddMeshParameter("Man-made Ground", "Man-made Ground", "Artificial Ground Meshes", GH_ParamAccess.list);
            pManager.AddMeshParameter("Nature Ground", "Nature Ground", " Natural Ground Meshes", GH_ParamAccess.list);
            pManager.AddMeshParameter("Water", "Water", "Water Ground Meshes", GH_ParamAccess.list);
            pManager.AddMeshParameter("Dynamics", "Dynamics", "Moving Object Meshes(People, Vehicles)", GH_ParamAccess.list);
            pManager.AddMeshParameter("Sky", "Sky", "Sky", GH_ParamAccess.item);

            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
            pManager[7].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Environment", "Environment", "Environment", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Analyzing Objects' view areas from the number of meshray points-------------------------------------------------------
            //Calculating Distance to each Object type and find closest object type and distance
            List<Mesh> buildings = new List<Mesh>();
            List<Mesh> equipments = new List<Mesh>();
            List<Mesh> trees = new List<Mesh>();
            List<Mesh> pavements = new List<Mesh>();
            List<Mesh> grass = new List<Mesh>();
            List<Mesh> water = new List<Mesh>();
            List<Mesh> dynamics = new List<Mesh>();
            Mesh msky = new Mesh();

            DA.GetDataList(0, buildings);
            DA.GetDataList(1, equipments);
            DA.GetDataList(2, trees);
            DA.GetDataList(3, pavements);
            DA.GetDataList(4, grass);
            DA.GetDataList(5, water);
            DA.GetDataList(6, dynamics);
            DA.GetData(7, ref msky);

            //Method : Joining List<Mesh> into one Mesh
            Mesh joinMesh(List<Mesh> elementMeshList)
            {
                Mesh elementMesh = new Mesh();
                foreach (var m in elementMeshList)
                {
                    elementMesh.Append(m);
                }
                elementMesh.Vertices.CullUnused();
                return elementMesh;
            }


            Mesh buildingMesh = joinMesh(buildings);
            Mesh equipmentMesh = joinMesh(equipments);
            Mesh treeMesh = joinMesh(trees);
            Mesh pavementMesh = joinMesh(pavements);
            Mesh grassMesh = joinMesh(grass);
            Mesh waterMesh = joinMesh(water);
            Mesh dynamicMesh = joinMesh(dynamics);

            Mesh sceneMesh = new Mesh();

            sceneMesh.Append(buildingMesh);
            sceneMesh.Append(equipmentMesh);
            sceneMesh.Append(treeMesh);
            sceneMesh.Append(pavementMesh);
            sceneMesh.Append(grassMesh);
            sceneMesh.Append(waterMesh);
            sceneMesh.Append(dynamicMesh);
            sceneMesh.Append(msky);

            List<int> faceCnts = new List<int>();

            faceCnts.Add(buildingMesh.Faces.Count);
            faceCnts.Add(equipmentMesh.Faces.Count);
            faceCnts.Add(treeMesh.Faces.Count);
            faceCnts.Add(pavementMesh.Faces.Count);
            faceCnts.Add(grassMesh.Faces.Count);
            faceCnts.Add(waterMesh.Faces.Count);
            faceCnts.Add(dynamicMesh.Faces.Count);
            faceCnts.Add(msky.Faces.Count);


            SEnvironment env = new SEnvironment(sceneMesh, faceCnts, pavements, grass);


            DA.SetData(0, env);

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
            get { return new Guid("C01ACADD-685F-4CBC-BEE9-6054A440CA2A"); }
        }
    }
}*/