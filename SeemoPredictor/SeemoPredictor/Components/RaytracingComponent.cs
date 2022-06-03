using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using SeemoPredictor.Geometry;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Text;
using System.IO;

namespace SeemoPredictor.Components
{
    public class RaytracingComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the RaytracingComponent class.
        /// </summary>
        public RaytracingComponent()
          : base("RayTracing", "RayTracing",
              "RayTracing",
              "SeEmo", "3|Analyzer")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Run", "Run", "Run", GH_ParamAccess.item);
            pManager.AddGenericParameter("Sensors", "S", "View Sensors", GH_ParamAccess.list);
            pManager.AddGenericParameter("Faces", "F", "Seemo Faces", GH_ParamAccess.list);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Report", "Out", "Console log of simulation", GH_ParamAccess.item);
            pManager.AddGenericParameter("RT_Result", "RT_Res", "RT_Result", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //input objects
            List<SmoSensor> sensors = new List<SmoSensor>();
            List<SmoFace> faces = new List<SmoFace>();
            Boolean run = false;

            if (!DA.GetData(0, ref run)) return;
            if (run == false) return;

            DA.GetDataList(1, sensors);
            DA.GetDataList(2, faces);


            //calculate min, max mode size and generate octree
            double avNodeSize = 0;
            double minNodeSize = double.MaxValue;
            double maxNodeSize = double.MinValue;
            BBox worldbounds = new BBox();

            for (int i = 0; i < faces.Count; i++)
            {
                var f = faces[i];
                f.Id = i;
                var size = f.BoundingBox.Size.Length;
                if (minNodeSize > size) minNodeSize = size;
                if (maxNodeSize < size) maxNodeSize = size;
                avNodeSize += size;
                worldbounds.Encapsulate(f.BoundingBox);
            }
            avNodeSize /= faces.Count;
            // make octree for visibility
            float worldSize = (float)worldbounds.Size.Length * 0.8f;
            PointOctree<SmoFace> octree0 = new PointOctree<SmoFace>(worldSize, worldbounds.Center, (float)(avNodeSize));
            foreach (SmoFace f in faces)
            {
                octree0.Add(f, f.Center);
            }



            //output objects
            var seemoResult = new SeemoResult();
            List<SmoSensorWithResults> resultNodes = new List<SmoSensorWithResults>();

            StringBuilder report = new StringBuilder();
            Stopwatch sp = new Stopwatch();
            sp.Start();

            // -------------------------
            // setup raycasting worklist
            // -------------------------

            List<SmoImage> images = new List<SmoImage>();
            List<SmoImage> splitImages = new List<SmoImage>();
            //

            bool singleDirection = false; //every sensors have the same viewdirections.
            if (sensors[0].ViewDirections.Length == 1 && sensors[0].ViewDirections[0].Z == 0)
            {
                singleDirection = true;
            }

            List<SmoImage> sphericalImages = new List<SmoImage>();

            for (int i = 0; i < sensors.Count; i++)
            {
                if (singleDirection)
                {
                    for (int j = 0; j < sensors[i].ViewDirections.Length; j++)
                    {
                        var image = new SmoImage(sensors[i].Pt, sensors[i].ViewDirections[j], sensors[i].Resolution, sensors[i].HorizontalViewAngle, sensors[i].VerticalViewAngle);
                        images.Add(image);
                    }
                }
                else
                {
                    //calculate spherical image
                    var sphereImage = new SmoImage(sensors[i].Pt, new Point3(0, 1, 0), sensors[i].Resolution, 360, sensors[i].VerticalViewAngle);
                    sphericalImages.Add(sphereImage);
                }

            }

            report.AppendLine("Setup raycasting worklist: " + sp.ElapsedMilliseconds + "[ms]");
            sp.Restart();

            // -------------------------
            // raycasting process
            // -------------------------

            SmoImage[] imageArray;
            if (singleDirection)
            {
                imageArray = images.ToArray();

                //    for (int i = 0; i < imageArray.Length; i++)
                Parallel.For(0, imageArray.Length, i =>
                {
                    imageArray[i].ComputeImage(octree0, maxNodeSize);
                }); // Parallel.For
            }
            else
            {
                SmoImage[] sphericalImageArray = sphericalImages.ToArray();

                //    for (int i = 0; i < imageArray.Length; i++)
                Parallel.For(0, sphericalImageArray.Length, i =>
                {
                    sphericalImageArray[i].ComputeImage(octree0, maxNodeSize);
                }); // Parallel.For

                //spliting images
                for (int i = 0; i < sensors.Count; i++)
                {
                    for (int j = 0; j < sensors[i].ViewDirections.Length; j++)
                    {
                        Point3 p = sensors[i].ViewDirections[j];
                        //divide image for each direction
                        var splitImage = SmoImage.FrameImages(sphericalImageArray[i], sensors[i].ViewDirections[j], sensors[i].HorizontalViewAngle, sensors[i].VerticalViewAngle);
                        //!!!!!!!!!check when sphere image view angle is not 360 180.....
                        splitImages.Add(splitImage);
                    }

                }

                imageArray = splitImages.ToArray();

            }


            report.AppendLine("Computing view images: " + sp.ElapsedMilliseconds + "[ms]");
            sp.Restart();




            // -------------------------
            //  create result output classes
            // -------------------------
            int imgIndex = 0;
            for (int i = 0; i < sensors.Count; i++)
            {
                List<DirectionResult> nodeResult = new List<DirectionResult>();
                SmoSensorWithResults node = new SmoSensorWithResults();
                node.NodeID = i;
                node.Pt = sensors[i].Pt;
                node.Dirs = sensors[i].ViewDirections;


                for (int j = 0; j < sensors[i].ViewDirections.Length; j++)
                {
                    DirectionResult directionResult = new DirectionResult();
                    directionResult.ID = ("Point" + i.ToString() + ":" + "Dir" + j.ToString());
                    directionResult.Dir = sensors[i].ViewDirections[j];

                    directionResult.ViewPointX = sensors[i].Pt.X;
                    directionResult.ViewPointY = sensors[i].Pt.Y;
                    directionResult.ViewPointZ = sensors[i].Pt.Z;
                    directionResult.ViewVectorX = sensors[i].ViewDirections[j].X;
                    directionResult.ViewVectorY = sensors[i].ViewDirections[j].Y;
                    directionResult.ViewVectorZ = sensors[i].ViewDirections[j].Z;



                    directionResult.Image = imageArray[imgIndex];
                    imgIndex++;


                    //Save direction result
                    nodeResult.Add(directionResult);
                }

                node.DirectionsResults = nodeResult;
                //Save node result
                resultNodes.Add(node);
            }
            seemoResult.Results = resultNodes;



           
            report.AppendLine("Saving result: " + sp.ElapsedMilliseconds + "[ms]");
            sp.Restart();


            DA.SetData(0, report.ToString());
            DA.SetData(1, seemoResult);


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
            get { return new Guid("D26E7C3C-0417-494A-8C87-0511BDCAD2F9"); }
        }
    }
}