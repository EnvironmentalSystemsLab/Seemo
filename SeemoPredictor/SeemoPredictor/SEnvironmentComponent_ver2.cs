using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using SeemoPredictor.SeemoGeo;

namespace SeemoPredictor
{
    public class SEnvironmentComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public SEnvironmentComponent()
          : base("Environment", "Environment",
              "Environment obects : Buildings, Nature, Pavement, Green, Dynamics, Sky Sphere",
              "SeEmo", "2|Environment")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Analyzing Building", "Analyzing Building", "Analyzing Building Meshes", GH_ParamAccess.list);
            pManager.AddMeshParameter("Neighbor Buildings", "Neighbor Buildings", "Artificial Object Meshes", GH_ParamAccess.list);
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
            pManager[8].Optional = true;

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
            List<Mesh> analyzingBuildings = new List<Mesh>();
            List<Mesh> buildings = new List<Mesh>();
            List<Mesh> equipments = new List<Mesh>();
            List<Mesh> trees = new List<Mesh>();
            List<Mesh> pavements = new List<Mesh>();
            List<Mesh> grass = new List<Mesh>();
            List<Mesh> water = new List<Mesh>();
            List<Mesh> dynamics = new List<Mesh>();
            List<Mesh> sky = new List<Mesh>();
            Mesh msky = new Mesh();

            SeemoInput envInput = new SeemoInput();

            DA.GetDataList(0, analyzingBuildings);
            DA.GetDataList(1, buildings);
            DA.GetDataList(2, equipments);
            DA.GetDataList(3, trees);
            DA.GetDataList(4, pavements);
            DA.GetDataList(5, grass);
            DA.GetDataList(6, water);
            DA.GetDataList(7, dynamics);
            DA.GetData(8, ref msky);

            sky.Add(msky);
            List<List<Mesh>> env = new List<List<Mesh>>();
            
            env.Add(analyzingBuildings);
            env.Add(buildings);
            env.Add(equipments);
            env.Add(trees);
            env.Add(pavements);
            env.Add(grass);
            env.Add(water);
            env.Add(dynamics);
            env.Add(sky);

            
            //Convert Mesh to SmoFace
            for(int i = 0; i < env.Count; i++)
            {
                SmoFace.SmoFaceType p = SmoFace.SmoFaceType._UNSET_;
                if (i == 0)
                {
                    p = SmoFace.SmoFaceType.AnalyzingBuilding;
                    for (int j = 0; j < env[i].Count; j++)
                    {
                        Mesh m = env[i][j];
                        envInput.AnalyzingBuilding.AddRange(Mesh2SmoFaces.MeshToSmoFaces(m, p));
                    }
                }
                else if (i == 1)
                {
                    p = SmoFace.SmoFaceType.Building;
                    for (int j = 0; j < env[i].Count; j++)
                    {
                        Mesh m = env[i][j];
                        envInput.Building.AddRange(Mesh2SmoFaces.MeshToSmoFaces(m, p));
                    }
                }
                else if (i == 2)
                {
                    p = SmoFace.SmoFaceType.Equipment;
                    for (int j = 0; j < env[i].Count; j++)
                    {
                        Mesh m = env[i][j];
                        envInput.Equipment.AddRange(Mesh2SmoFaces.MeshToSmoFaces(m, p));
                    }
                }
                else if (i == 3)
                {
                    p = SmoFace.SmoFaceType.Tree;
                    for (int j = 0; j < env[i].Count; j++)
                    {
                        Mesh m = env[i][j];
                        envInput.Tree.AddRange(Mesh2SmoFaces.MeshToSmoFaces(m, p));
                    }
                }
                else if (i == 4)
                {
                    p = SmoFace.SmoFaceType.Pavement;
                    for (int j = 0; j < env[i].Count; j++)
                    {
                        Mesh m = env[i][j];
                        envInput.Pavement.AddRange(Mesh2SmoFaces.MeshToSmoFaces(m, p));
                    }
                }
                else if (i == 5)
                {
                    p = SmoFace.SmoFaceType.Grass;
                    for (int j = 0; j < env[i].Count; j++)
                    {
                        Mesh m = env[i][j];
                        envInput.Grass.AddRange(Mesh2SmoFaces.MeshToSmoFaces(m, p));
                    }
                }
                else if (i == 6)
                {
                    p = SmoFace.SmoFaceType.Water;
                    for (int j = 0; j < env[i].Count; j++)
                    {
                        Mesh m = env[i][j];
                        envInput.Water.AddRange(Mesh2SmoFaces.MeshToSmoFaces(m, p));
                    }
                }
                else if (i == 7)
                {
                    p = SmoFace.SmoFaceType.Dynamics;
                    for (int j = 0; j < env[i].Count; j++)
                    {
                        Mesh m = env[i][j];
                        envInput.Dynamics.AddRange(Mesh2SmoFaces.MeshToSmoFaces(m, p));
                    }
                }
                else if (i == 8)
                {
                    p = SmoFace.SmoFaceType.Sky;
                    for (int j = 0; j < env[i].Count; j++)
                    {
                        Mesh m = env[i][j];
                        envInput.Sky.AddRange(Mesh2SmoFaces.MeshToSmoFaces(m, p));
                    }
                }
                else
                {
                    break;
                }

            }

            DA.SetData(0, envInput);
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
}