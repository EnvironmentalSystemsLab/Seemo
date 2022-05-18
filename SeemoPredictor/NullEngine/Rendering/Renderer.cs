using NullEngine.Utils;
using NullEngine.Rendering.DataStructures;
using System.Threading;
using ILGPU.Runtime;
using NullEngine.Rendering.Implementation;
using System.Windows;
using System;
using ILGPU.Algorithms;
using System.Collections.Generic;
//using SeemoPredictor;
//using SeemoPredictor.Geometry;
using System.IO;

namespace NullEngine.Rendering
{
    public class Renderer
    {
        public int width;
        public int height;
        
        private bool run = true;
        private int targetFramerate;
        private double frameTime;

        private ByteFrameBuffer deviceFrameBuffer;
        private FloatFrameBuffer deviceFrameDistanceBuffer;

        //where data is stored in cpu
        private byte[] frameBuffer = new byte[0];
        private byte[] materialIDBuffer = new byte[0];
        private float[] frameDistanceBuffer = new float[0];

        private GPU gpu;
        public Camera camera; //originally it was private
        private Scene scene;
        private FrameData frameData;
        private UI.RenderFrame renderFrame;
        private Thread renderThread;
        private FrameTimer frameTimer;


        //for Seemo output
        public byte[] materials;
        public float[] distances;

        public Renderer(UI.RenderFrame renderFrame, int targetFramerate, bool forceCPU)
        {
            this.renderFrame = renderFrame;
            this.targetFramerate = targetFramerate;
            gpu = new GPU(forceCPU);
            //this.scene = new Scene(gpu, "../../../Assets/CubeTest/Scene.json");
            //this.scene = new Scene(gpu, "../../../Assets/Sponza/Scene.json");
            //this.scene = new Scene(gpu, "../../../Assets/Suzannes/Scene.json");
            this.scene = new Scene(gpu, "../../../Assets/Viewbackground/Scene.json");
            camera = new Camera(new Vec3(0, 0, 10), new Vec3(0, 0, 0), new Vec3(0, -1, 0), 1500, 1000, 40, new Vec3(0, 0, 0));
            frameTimer = new FrameTimer();

            renderFrame.onResolutionChanged = OnResChanged;

            renderThread = new Thread(RenderThread);
            renderThread.IsBackground = true;


            
        }

        public void Start()
        {
            renderThread.Start();
        }

        public void Stop()
        {
            run = false;
            renderThread.Join();
        }

        private void OnResChanged(int width, int height)
        {
            this.width = width;
            this.height = height;

            camera = new Camera(camera, this.width, this.height);
        }

        //eveything below this happens in the render thread
        private void RenderThread()
        {
            while (run)
            {
                frameTimer.startUpdate();

                if(ReadyFrameBuffer())
                {
                    RenderToFrameBuffer();
                    Application.Current.Dispatcher.InvokeAsync(Draw);

                    materials = materialIDBuffer;
                    distances = frameDistanceBuffer;
                }

                frameTime = frameTimer.endUpdateForTargetUpdateTime(1000.0 / targetFramerate, true);
                renderFrame.frameTime = frameTime;
            }

            if (deviceFrameBuffer != null)
            {
                deviceFrameBuffer.Dispose();
                deviceFrameDistanceBuffer.Dispose();

                frameData.Dispose();
            }
            gpu.Dispose();

            

            //add seemo
            List<SmoSensor> sensors = new List<SmoSensor>();

            List<double> overallRatings = new List<double>();
            List<double> viewContents = new List<double>();
            List<double> viewAccesses = new List<double>();
            List<double> privacys = new List<double>();

            double glevel = 0; //later connect with input

            //output objects
            var seemoResult = new SeemoResult();
            List<SmoSensorWithResults> resultNodes = new List<SmoSensorWithResults>();

            //StringBuilder report = new StringBuilder();
            //Stopwatch sp = new Stopwatch();
            //sp.Start();

            // -------------------------
            // setup raycasting worklist
            // -------------------------
            

            // -------------------------
            // setup raycasting for seemo
            // -------------------------

            List<SmoImage> images = new List<SmoImage>();
            List<SmoImage> splitImages = new List<SmoImage>();
            //
            var image = new SmoImage(camera.origin.ToPoint3(), camera.lookAt.ToPoint3(), camera.width, 70, 52);
            images.Add(image);

            //report.AppendLine("Setup raycasting worklist: " + sp.ElapsedMilliseconds + "[ms]");
            //sp.Restart();

            // -------------------------
            // converting raycasting result from nullengine to seemo
            // -------------------------
            SmoImage[] imageArray;
            imageArray = images.ToArray();

            imageArray[0].ConvertFromGpuResultToImage(distances, materials) ;


            //report.AppendLine("Computing view images: " + sp.ElapsedMilliseconds + "[ms]");
            //sp.Restart();

            // -------------------------
            // execute ML model and create result output classes
            // -------------------------
            int imgIndex = 0;
            for (int i = 0; i < 1; i++) //sensors.Count
            {
                List<DirectionResult> nodeResult = new List<DirectionResult>();
                SmoSensorWithResults node = new SmoSensorWithResults();
                node.NodeID = i;
                node.Pt = sensors[i].Pt;
                node.Dirs = sensors[i].ViewDirections;


                for (int j = 0; j < 1; j++) // sensors[i].ViewDirections.Length
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


                    //directionResult.Image = new SmoImage(sensors[i].Pt, sensors[i].ViewDirections[j], sensors[i].Resolution, sensors[i].HorizontalViewAngle, sensors[i].VerticalViewAngle);
                    //directionResult.Image.ComputeImage(octree0, maxNodeSize);




                    // compute the ML model inputs from the SmoImage class here
                    directionResult.ComputeFeatures();

                    directionResult.FloorHeights = (directionResult.ViewPointZ - glevel);


                    //Generate Model input for prediction
                    ModelInput sampleDataOverallRating = new ModelInput()
                    {

                        WindowNumber = (float)2.0,
                        WindowAreaSum = (float)directionResult.WindowAreaSum,
                        Z1PtsCountRatio = (float)directionResult.Z1PtsCountRatio,
                        Z2PtCountRatio = (float)directionResult.Z2PtsCountRatio,
                        Z3PtsCountRatio = (float)directionResult.Z3PtsCountRatio,
                        Z4PtsCountRatio = (float)directionResult.Z4PtsCountRatio,
                        BuildingPtsCountRatio = (float)directionResult.BuildingPtsCountRatio,
                        EquipmentPtsCountRatio = (float)directionResult.EquipmentPtsCountRatio,
                        TreePtsCountRatio = (float)directionResult.TreePtsCountRatio,
                        PavementPtsCountRatio = (float)directionResult.PavementPtsCountRatio,
                        GrassPtsCountRatio = (float)directionResult.GrassPtsCountRatio,
                        WaterPtsCountRatio = (float)directionResult.WaterPtsCountRatio,
                        DynamicPtsRatio = (float)directionResult.DynamicPtsCountRatio,
                        SkyPtsCountRatio = (float)directionResult.SkyPtsCountRatio,
                        ElementNumber = (float)directionResult.ElementNumber,
                        FloorHeights = (float)directionResult.FloorHeights,
                        BuildingClosestDist = (float)directionResult.BuildingClosestDist,
                        EquipmentClosestDist = (float)directionResult.EquipmentClosestDist,
                        TreeClosestDist = (float)directionResult.TreeClosestDist,
                        GrassClosestDist = (float)directionResult.GrassClosestDist,
                        WaterClosestDist = (float)directionResult.WaterClosestDist,
                        DynamicClosestDist = (float)directionResult.DynamicClosestDist,
                        SkyCondition = (float)directionResult.SkyCondition,

                    };
                    ModelInputViewContent sampleDataViewContent = new ModelInputViewContent()
                    {

                        WindowNumber = (float)2.0,
                        WindowAreaSum = (float)directionResult.WindowAreaSum,
                        Z1PtsCountRatio = (float)directionResult.Z1PtsCountRatio,
                        Z2PtCountRatio = (float)directionResult.Z2PtsCountRatio,
                        Z3PtsCountRatio = (float)directionResult.Z3PtsCountRatio,
                        Z4PtsCountRatio = (float)directionResult.Z4PtsCountRatio,
                        BuildingPtsCountRatio = (float)directionResult.BuildingPtsCountRatio,
                        EquipmentPtsCountRatio = (float)directionResult.EquipmentPtsCountRatio,
                        TreePtsCountRatio = (float)directionResult.TreePtsCountRatio,
                        PavementPtsCountRatio = (float)directionResult.PavementPtsCountRatio,
                        GrassPtsCountRatio = (float)directionResult.GrassPtsCountRatio,
                        WaterPtsCountRatio = (float)directionResult.WaterPtsCountRatio,
                        DynamicPtsRatio = (float)directionResult.DynamicPtsCountRatio,
                        SkyPtsCountRatio = (float)directionResult.SkyPtsCountRatio,
                        ElementNumber = (float)directionResult.ElementNumber,
                        FloorHeights = (float)directionResult.FloorHeights,
                        BuildingClosestDist = (float)directionResult.BuildingClosestDist,
                        EquipmentClosestDist = (float)directionResult.EquipmentClosestDist,
                        TreeClosestDist = (float)directionResult.TreeClosestDist,
                        GrassClosestDist = (float)directionResult.GrassClosestDist,
                        WaterClosestDist = (float)directionResult.WaterClosestDist,
                        DynamicClosestDist = (float)directionResult.DynamicClosestDist,
                        SkyCondition = (float)directionResult.SkyCondition,

                    };
                    ModelInputViewAccess sampleDataViewAccess = new ModelInputViewAccess()
                    {

                        WindowNumber = (float)2.0,
                        WindowAreaSum = (float)directionResult.WindowAreaSum,
                        Z1PtsCountRatio = (float)directionResult.Z1PtsCountRatio,
                        Z2PtCountRatio = (float)directionResult.Z2PtsCountRatio,
                        Z3PtsCountRatio = (float)directionResult.Z3PtsCountRatio,
                        Z4PtsCountRatio = (float)directionResult.Z4PtsCountRatio,
                        BuildingPtsCountRatio = (float)directionResult.BuildingPtsCountRatio,
                        EquipmentPtsCountRatio = (float)directionResult.EquipmentPtsCountRatio,
                        TreePtsCountRatio = (float)directionResult.TreePtsCountRatio,
                        PavementPtsCountRatio = (float)directionResult.PavementPtsCountRatio,
                        GrassPtsCountRatio = (float)directionResult.GrassPtsCountRatio,
                        WaterPtsCountRatio = (float)directionResult.WaterPtsCountRatio,
                        DynamicPtsRatio = (float)directionResult.DynamicPtsCountRatio,
                        SkyPtsCountRatio = (float)directionResult.SkyPtsCountRatio,
                        ElementNumber = (float)directionResult.ElementNumber,
                        FloorHeights = (float)directionResult.FloorHeights,
                        BuildingClosestDist = (float)directionResult.BuildingClosestDist,
                        EquipmentClosestDist = (float)directionResult.EquipmentClosestDist,
                        TreeClosestDist = (float)directionResult.TreeClosestDist,
                        GrassClosestDist = (float)directionResult.GrassClosestDist,
                        WaterClosestDist = (float)directionResult.WaterClosestDist,
                        DynamicClosestDist = (float)directionResult.DynamicClosestDist,
                        SkyCondition = (float)directionResult.SkyCondition,

                    };
                    ModelInputPrivacy sampleDataPrivacy = new ModelInputPrivacy()
                    {

                        WindowNumber = (float)2.0,
                        WindowAreaSum = (float)directionResult.WindowAreaSum,
                        Z1PtsCountRatio = (float)directionResult.Z1PtsCountRatio,
                        Z2PtCountRatio = (float)directionResult.Z2PtsCountRatio,
                        Z3PtsCountRatio = (float)directionResult.Z3PtsCountRatio,
                        Z4PtsCountRatio = (float)directionResult.Z4PtsCountRatio,
                        BuildingPtsCountRatio = (float)directionResult.BuildingPtsCountRatio,
                        EquipmentPtsCountRatio = (float)directionResult.EquipmentPtsCountRatio,
                        TreePtsCountRatio = (float)directionResult.TreePtsCountRatio,
                        PavementPtsCountRatio = (float)directionResult.PavementPtsCountRatio,
                        GrassPtsCountRatio = (float)directionResult.GrassPtsCountRatio,
                        WaterPtsCountRatio = (float)directionResult.WaterPtsCountRatio,
                        DynamicPtsRatio = (float)directionResult.DynamicPtsCountRatio,
                        SkyPtsCountRatio = (float)directionResult.SkyPtsCountRatio,
                        ElementNumber = (float)directionResult.ElementNumber,
                        FloorHeights = (float)directionResult.FloorHeights,
                        BuildingClosestDist = (float)directionResult.BuildingClosestDist,
                        EquipmentClosestDist = (float)directionResult.EquipmentClosestDist,
                        TreeClosestDist = (float)directionResult.TreeClosestDist,
                        GrassClosestDist = (float)directionResult.GrassClosestDist,
                        WaterClosestDist = (float)directionResult.WaterClosestDist,
                        DynamicClosestDist = (float)directionResult.DynamicClosestDist,
                        SkyCondition = (float)directionResult.SkyCondition,

                    };



                    //max:43259, min: 17892
                    //(directionResult.WindowAreaSum * 5288.02083158) > 17892) && ((directionResult.WindowAreaSum * 5288.02083158) < 43259)
                    if (directionResult.WindowAreaSum > 0)
                    {
                        // Make a single prediction on the sample data and print results
                        var overallRating = ConsumeOverallRating.Predict(sampleDataOverallRating);
                        var viewContent = ConsumeViewContent.Predict(sampleDataViewContent);
                        var viewAccess = ConsumeViewAccess.Predict(sampleDataViewAccess);
                        var privacy = ConsumePrivacy.Predict(sampleDataPrivacy);

                        overallRatings.Add(overallRating.OverallRatingB);
                        viewContents.Add(viewContent.ViewContentB);
                        viewAccesses.Add(viewAccess.ViewAccessB);
                        privacys.Add(privacy.PrivacyB);

                        directionResult.PredictedOverallRating = overallRatings[overallRatings.Count - 1];
                        directionResult.PredictedViewContent = viewContents[viewContents.Count - 1];
                        directionResult.PredictedViewAccess = viewAccesses[viewAccesses.Count - 1];
                        directionResult.PredictedPrivacy = privacys[privacys.Count - 1];

                    }
                    else
                    {
                        overallRatings.Add(double.NaN);
                        viewContents.Add(double.NaN);
                        viewAccesses.Add(double.NaN);
                        privacys.Add(double.NaN);

                        directionResult.PredictedOverallRating = double.NaN;
                        directionResult.PredictedViewContent = double.NaN;
                        directionResult.PredictedOverallRating = double.NaN;
                        directionResult.PredictedViewContent = double.NaN;
                    }



                    //Save direction result
                    nodeResult.Add(directionResult);
                }

                node.DirectionsResults = nodeResult;
                //Save node result
                resultNodes.Add(node);
            }
            seemoResult.Results = resultNodes;



            // -------------------------
            //save all results to json file
            // -------------------------

            //report.AppendLine("Computing predictions: " + sp.ElapsedMilliseconds + "[ms]");
            //sp.Restart();

            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            string dir = (path + @"\Seemo");

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            long time = DateTime.Now.ToFileTime();
            seemoResult.ToFile(dir + @"\Result" + time + ".json");

            */
        }

        private void ConvertToSeemoJSON()
        {

        }

        private bool ReadyFrameBuffer()
        {
            if((width != 0 && height != 0))
            {
                if(deviceFrameBuffer == null || deviceFrameBuffer.frameBuffer.width != width || deviceFrameBuffer.frameBuffer.height != height)
                {
                    if (deviceFrameBuffer != null)
                    {
                        deviceFrameBuffer.Dispose();
                        deviceFrameDistanceBuffer.Dispose();

                        frameData.Dispose();
                    }

                    frameBuffer = new byte[width * height * 3];
                    materialIDBuffer = new byte[width * height];
                    frameDistanceBuffer = new float[width * height];

                    deviceFrameBuffer = new ByteFrameBuffer(gpu, height, width);
                    deviceFrameDistanceBuffer = new FloatFrameBuffer(gpu, height, width);

                    frameData = new FrameData(gpu.device, width, height);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private void RenderToFrameBuffer()
        {
            if (deviceFrameBuffer != null && !deviceFrameBuffer.isDisposed)
            {
                gpu.Render(camera, scene, deviceFrameBuffer.frameBuffer, deviceFrameDistanceBuffer.frameDistanceBuffer, frameData.deviceFrameData);
                deviceFrameBuffer.memoryBuffer.CopyToCPU(frameBuffer);
                deviceFrameBuffer.memoryMaterialIDBuffer.CopyToCPU(materialIDBuffer);

                deviceFrameDistanceBuffer.memoryDistanceBuffer.CopyToCPU(frameDistanceBuffer);


                //cpu side everything is stored in frameBuffer
            }
        }

        private void Draw()
        {
            renderFrame.update(ref frameBuffer);
            renderFrame.updateMaterialID(ref materialIDBuffer);
            //renderFrame.updateDistance(ref frameDistanceBuffer);
            renderFrame.frameRate = frameTimer.lastFrameTimeMS;
        }
    }
}
