using System;
using ILGPU.Algorithms;
using ILGPU;
using System.Collections.Generic;
using NullEngine.Rendering;
using NullEngine.Rendering.DataStructures;
using System.Windows;
using Grasshopper.Kernel;
using Rhino.Geometry;
using SeemoPredictor.Geometry;
using SeemoPredictor;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace Nullgh
{
    public class NullghComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public NullghComponent()
          : base("GPURayTracing", "GPURayTracing",
            "GPURayTracing",
            "SeEmo", "3|Analyzer")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Run", "Run", "Run", GH_ParamAccess.item);
            pManager.AddBooleanParameter("GPU", "GPU", "GPU raytracing", GH_ParamAccess.item);
            pManager.AddGenericParameter("Sensors", "S", "View Sensors", GH_ParamAccess.list);
            pManager.AddGenericParameter("Faces", "F", "Seemo Faces", GH_ParamAccess.list);
            pManager.AddBooleanParameter("BitmapSave", "BitmapSave", "BitmapSave", GH_ParamAccess.item);
            //pManager.AddTextParameter("Obj file", "Obj", "Obj FilePath", GH_ParamAccess.item);
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
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<SmoSensor> sensors = new List<SmoSensor>();
            List<SmoFace> envFaces = new List<SmoFace>();
            Boolean run = false;
            Boolean forceCPU = true;
            string objFilePath = "";
            Boolean bitmapSave = false;


            if (!DA.GetData(0, ref run)) return;
            if (run == false) return;


            if (!DA.GetData(1, ref forceCPU)) return;
            DA.GetDataList(2, sensors);
            DA.GetDataList(3, envFaces);
            DA.GetData(4, ref bitmapSave);
            //if (!DA.GetData(4, ref objFilePath)) return;

            List<float> vertices = new List<float>();
            List<int> triangles = new List<int> ();
            List<int> mats = new List<int> ();

            SmoFace.ConvertToOBJdata(envFaces, out vertices, out triangles, out mats); //when 3dm imported to nullgh (obj) pay attention to yz flip


            Renderer renderer = new Renderer(!forceCPU, objFilePath, vertices, triangles, mats);  //go in
            renderer.savebitmap = bitmapSave;
            List<Camera> cameras = new List<Camera>();
            
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

            bool customSetDirections = true; //every sensors have the same viewdirections.
            if (sensors[0].ViewDirections.Length == 8 && Point3.AngleDegree(sensors[0].ViewDirections[0], sensors[0].ViewDirections[4]) == 180)
            {
                customSetDirections = false;
            }

            List<SmoImage> sphericalImages = new List<SmoImage>();

            for (int i = 0; i < sensors.Count; i++)
            {
                if (customSetDirections)
                {
                    for (int j = 0; j < sensors[i].ViewDirections.Length; j++)
                    {
                        var image = new SmoImage(sensors[i].Pt, sensors[i].ViewDirections[j], sensors[i].Resolution, sensors[i].HorizontalViewAngle, sensors[i].VerticalViewAngle);
                        images.Add(image);
                        Camera cam = new Camera(Vec3.ToVec3(sensors[i].Pt), Vec3.ToVec3(sensors[i].Pt + sensors[i].ViewDirections[j]), new Vec3(0, 1, 0), image.xres, image.yres, (float)sensors[i].VerticalViewAngle, new Vec3(0, 0, 0), (float)image.angleStep);
                        cameras.Add(cam);
                    }
                }
                else
                {
                    //calculate spherical image
                    var sphereImage = new SmoImage(sensors[i].Pt, new Point3(0, 1, 0), sensors[i].Resolution, 360, sensors[i].VerticalViewAngle);
                    sphericalImages.Add(sphereImage);
                    Camera cam = new Camera(Vec3.ToVec3(sensors[i].Pt), Vec3.ToVec3(sensors[i].Pt + new Point3(0,1,0)), new Vec3(0, 1, 0), sphereImage.xres, sphereImage.yres, 180.0f, new Vec3(0, 0, 0), (float)sphereImage.angleStep);
                    cameras.Add(cam);
                }

            }

            report.AppendLine("Setup raycasting worklist: " + sp.ElapsedMilliseconds + "[ms]");
            sp.Restart();

            // -------------------------
            // raycasting process
            // -------------------------

            SmoImage[] imageArray;
            if (customSetDirections)
            {
                imageArray = images.ToArray();

                for(int i = 0; i < imageArray.Length; i++)
                {
                    renderer.CameraUpdateAndRender(cameras[i]);
                    renderer.Start();
                    imageArray[i] = renderer.RayTracingToImage(imageArray[i]);
                }

                ////    for (int i = 0; i < imageArray.Length; i++)
                //Parallel.For(0, imageArray.Length, i =>
                //{
                //    imageArray[i].ComputeImage(octree0, maxNodeSize);
                //}); // Parallel.For
            }
            else
            {
                SmoImage[] sphericalImageArray = sphericalImages.ToArray();
                for (int i = 0; i < sphericalImageArray.Length; i++)
                {
                    renderer.CameraUpdateAndRender(cameras[i]);
                    renderer.Start();
                    sphericalImageArray[i] = renderer.RayTracingToImage(sphericalImageArray[i]);
                }

                ////    for (int i = 0; i < imageArray.Length; i++)
                //Parallel.For(0, sphericalImageArray.Length, i =>
                //{
                //    sphericalImageArray[i].ComputeImage(octree0, maxNodeSize);
                //}); // Parallel.For



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

                node.Vert0 = sensors[i].QuadMeshVertices[0];
                node.Vert1 = sensors[i].QuadMeshVertices[1];
                node.Vert2 = sensors[i].QuadMeshVertices[2];
                node.Vert3 = sensors[i].QuadMeshVertices[3];


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

                    /*
                    // save rendering into bmp ////////// SOMETHING IS NOT WORKING WHEN J>=4
                    string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    string dir = (path + @"\NullEngine");

                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    long time = DateTime.Now.ToFileTime();
                    string filename1 = dir + @"\Mat_Sensor" + i + "_dir" + j + time + ".bmp";
                    string filename2 = dir + @"\Depth_Sensor" + i + "_dir" + j + time + ".bmp";

                    var MatBitmap = directionResult.Image.GetDepthBitmap();
                    MatBitmap.Save(filename1);

                    var DepthBitmap = directionResult.Image.GetLabelBitmap();
                    DepthBitmap.Save(filename2);
                    */

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
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon => null;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("7CE0BCE6-74EE-4A3B-97D0-C40393FE6C9A");

    }
}