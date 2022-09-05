using NullEngine.Rendering.DataStructures;
using System.Threading;
using ILGPU.Runtime;
using NullEngine.Rendering.Implementation;
using System.Windows;
using System;
using System.Drawing;
using System.IO;
using ILGPU.Algorithms;
using System.Collections.Generic;
using ILGPU;
using SeemoPredictor;
using SeemoPredictor.Geometry;

namespace NullEngine.Rendering
{
    public class Renderer
    {
        public int width = 36;
        public int height = 10;
        
        private bool run = true;
        public bool savebitmap = false;

        private ByteFrameBuffer deviceFrameBuffer;
        private FloatFrameBuffer deviceFrameDistanceBuffer;
        private ByteFrameBuffer deviceFrameDistance2Buffer;

        //where data is stored in cpu
        private byte[] frameBuffer = new byte[0]; //this for depthmap
        private byte[] frameMaterialID2Buffer = new byte[0]; //this is the real Material for map // for saving bmp
        private byte[] frameMaterialIDBuffer = new byte[0]; //important //this is the real Material for map
        private float[] frameDistanceBuffer = new float[0]; //important //this is the real distance for depthmap
        private byte[] frameDistance2Buffer = new byte[0]; // for what??

        private GPU gpu;
        private Camera camera;
        private Scene scene;
        private FrameData frameData;
        private Thread renderThread;  // main component to run. renderer.start() -> renderThread.start()

        
        public Renderer(bool forceCPU, string objFilePath, List<float> vertices, List<int> triangles, List<int> mats)
        {

            gpu = new GPU(forceCPU);
            
            this.scene = new Scene(objFilePath, gpu, vertices, triangles, mats);

        }


        public Renderer(bool forceCPU, string objFilePath)
        {
            
            gpu = new GPU(forceCPU);
            
            this.scene = new Scene(objFilePath, gpu);

        }

        public void CameraUpdateAndRender(Camera cameraInput)
        {
            //Panoramic: verticalFov = 180
            //camera origin and lookAt coordinates should be changed, (x, y, z) -> (x, -z, -y)
            camera = cameraInput;

            this.width = camera.width;
            this.height = camera.height;

            //main rendereing setup
            //renderThread = new Thread(RenderThread);  //thread.cs?? is not here?
            //renderThread.IsBackground = true;

        }

        public void Start()
        {
            if (ReadyFrameBuffer())  //dispose previous bufferdata and reset them for new resolution
            {
                RenderToFrameBuffer();  //actual rendereing and copy the data to cpu


                var MaterialBitmap = new Bitmap(width, height);

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        byte r = frameMaterialID2Buffer[(width * y + x) * 3];
                        byte g = frameMaterialID2Buffer[(width * y + x) * 3 + 1];
                        byte b = frameMaterialID2Buffer[(width * y + x) * 3 + 2];
                        var pixColor = Color.FromArgb(r, g, b);

                        MaterialBitmap.SetPixel(width - x - 1, y, pixColor);

                    }
                }


                var DepthBitmap = new Bitmap(width, height);

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        byte r = frameBuffer[(width * y + x) * 3];
                        byte g = frameBuffer[(width * y + x) * 3 + 1];
                        byte b = frameBuffer[(width * y + x) * 3 + 2];
                        var pixColor = Color.FromArgb(r, g, b);

                        DepthBitmap.SetPixel(width - x - 1, y, pixColor);
                    }
                }

                if (savebitmap)
                {
                    //save rendering into bmp
                    string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    string dir = (path + @"\NullEngine");

                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    long time = DateTime.Now.ToFileTime();
                    string filename1 = dir + @"\MaterialMap" + time + ".bmp";
                    string filename2 = dir + @"\DepthMap" + time + ".bmp";

                    MaterialBitmap.Save(filename1);
                    DepthBitmap.Save(filename2);
                }
                
                
            }
            //renderThread.Start();
        }

        

        public void Stop()
        {
            run = false;
            renderThread.Join();
        }


        public SmoImage RayTracingToImage(SmoImage image)
        {
            
            for (int x = 0; x < image.xres; x++)
            {
                for (int y = 0; y < image.yres; y++)
                {
                    byte m = frameMaterialIDBuffer[y * image.xres + x];
                    double dist = (double)frameDistanceBuffer[y * image.xres + x];
                    

                    image.Hits[x][y] = Vec3.ToPoint3(camera.GetRay(x, y).a + (camera.GetRay(x, y).b) * (float)dist);
                    image.DepthMap[x][image.yres - y - 1] = dist;
                    
                    switch (m)
                    {
                        case 0:
                            image.LabelMap[x][image.yres - y - 1] = SmoFace.SmoFaceType.Interior;
                            break;
                        case 1:
                            image.LabelMap[x][image.yres - y - 1] = SmoFace.SmoFaceType.Exterior;
                            break;
                        case 2:
                            image.LabelMap[x][image.yres - y - 1] = SmoFace.SmoFaceType.Glazing; //there would be no glazing cause we excluded glazing when forming the mesh
                            break;
                        case 3:
                            image.LabelMap[x][image.yres - y - 1] = SmoFace.SmoFaceType.Context_Building;
                            break;
                        case 4:
                            image.LabelMap[x][image.yres - y - 1] = SmoFace.SmoFaceType.Context_Window;
                            break;
                        case 5:
                            image.LabelMap[x][image.yres - y - 1] = SmoFace.SmoFaceType.Equipment;
                            break;
                        case 6:
                            image.LabelMap[x][image.yres - y - 1] = SmoFace.SmoFaceType.Landmark;
                            break;
                        case 7:
                            image.LabelMap[x][image.yres - y - 1] = SmoFace.SmoFaceType.Sidewalk;
                            break;
                        case 8:
                            image.LabelMap[x][image.yres - y - 1] = SmoFace.SmoFaceType.Road;
                            break;
                        case 9:
                            image.LabelMap[x][image.yres - y - 1] = SmoFace.SmoFaceType.ParkingLot;
                            break;
                        case 10:
                            image.LabelMap[x][image.yres - y - 1] = SmoFace.SmoFaceType.Tree;
                            break;
                        case 11:
                            image.LabelMap[x][image.yres - y - 1] = SmoFace.SmoFaceType.Grass;
                            break;
                        case 12:
                            image.LabelMap[x][image.yres - y - 1] = SmoFace.SmoFaceType.Water;
                            break;
                        case 13:
                            image.LabelMap[x][image.yres - y - 1] = SmoFace.SmoFaceType._UNSET_;
                            break;
                    }
                }
            }
            return image;
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
                        deviceFrameDistance2Buffer.Dispose();

                        frameData.Dispose();
                    }

                    frameBuffer = new byte[width * height * 3];
                    frameDistanceBuffer = new float[width * height];
                    frameDistance2Buffer = new byte[width * height * 3];
                    frameMaterialIDBuffer = new byte[width * height];
                    frameMaterialID2Buffer = new byte[width * height * 3];
                    
                    deviceFrameBuffer = new ByteFrameBuffer(gpu, height, width);
                    deviceFrameDistanceBuffer = new FloatFrameBuffer(gpu, height, width);
                    deviceFrameDistance2Buffer = new ByteFrameBuffer(gpu, height, width);

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
                gpu.Render(camera, scene, deviceFrameBuffer.frameBuffer, deviceFrameDistanceBuffer.frameDistanceBuffer, frameData.deviceFrameData);  //should I update?? for distance2
                deviceFrameBuffer.memoryBuffer.CopyToCPU(frameBuffer);
                
                deviceFrameBuffer.memoryMaterialIDBuffer.CopyToCPU(frameMaterialIDBuffer);
                deviceFrameBuffer.memoryMaterialID2Buffer.CopyToCPU(frameMaterialID2Buffer);

                deviceFrameDistanceBuffer.memoryDistanceBuffer.CopyToCPU(frameDistanceBuffer);
                deviceFrameDistance2Buffer.memoryDistance2Buffer.CopyToCPU(frameDistance2Buffer);

            }
        }
    }
}
