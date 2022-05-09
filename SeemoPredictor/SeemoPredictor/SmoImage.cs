using SeemoPredictor.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;
using SeemoPredictor.GPU;


namespace SeemoPredictor
{
    public class SmoImage
    {
        public int xres { get; set; }
        public int yres { get; set; }

        double angleStep { get; set; }  

        public Point3 Pt { get; set; }
        public Point3 Dir { get; set; }
        public double Height { get; set; }
        public Point3 TopCorner { get; set; }
        public Point3 xAxis { get; set; }
        public Point3 yAxis { get; set; }

        //Imported from GPU tracer

        public float aspectRatio { get; set; }
        public float cameraPlaneDist { get; set; }
        public float reciprocalHeight { get; set; }
        public float reciprocalWidth { get; set; }
        public OrthoNormalBasis axis { get; set; }



        //raycast input
        public Point3[][] ImageRays { get; set; }
        public Point3[] ImageRaysFlat
        {
            get { 
            
                if(ImageRays == null)return null;
                return ImageRays.SelectMany(a => a).ToArray();

            }
        }

        //raycast output
        public Point3[][] Hits { get; set; }
        public Point3[] HitsFlat
        {
            get
            {
                if (Hits == null) return null;
                return Hits.SelectMany(a => a).ToArray();
            }
        }

        //raycast output
        public double[][] DepthMap { get; set; }
        public double[] DepthMapFlat
        {
            get
            {
                if (DepthMap == null) return null;
                return DepthMap.SelectMany(a => a).ToArray();
            }
        }

        //raycast output
        public SmoFace.SmoFaceType[][] LabelMap { get; set; }
        public string[] LabelMapFlat
        {
            get
            {
                if (LabelMap == null) return null;
                var arr = LabelMap.SelectMany(a => a).ToArray();
                return arr.Select(a=>a.ToString()).ToArray();
            }
        }

        public SmoImage(Point3 pt, Point3 dir, int Resolution, double horizontalViewAngle, double verticalViewAngle) {

            this.Pt = pt;
            this.Dir = dir;

            //Define Left, right, up, down vectors to measure room dimension
            Point3 nvd = dir;
            nvd.Normalize();
            Dir = nvd;

            Point3 vup = new Point3(0, 0, 1);

            xAxis = Point3.Cross(nvd, vup);
            xAxis.Normalize();

            yAxis = Point3.Cross(nvd, -xAxis);
            yAxis.Normalize();


            axis = OrthoNormalBasis.fromZY(nvd, vup);


            xres = (Resolution / 2) * 2;
            angleStep = horizontalViewAngle / xres;
            yres = ((int)(verticalViewAngle / (double)angleStep) / 2) * 2;

          

            ImageRays = new Point3[xres][];


            Hits = new Point3[xres][];
            DepthMap = new double[xres][];
            LabelMap = new SmoFace.SmoFaceType[xres][];

            for (int i = 0; i < xres; i++) {
                ImageRays[i] = new Point3[yres];

                Hits[i] = new Point3[yres];
                DepthMap[i] = new double[yres];
                LabelMap[i] = new SmoFace.SmoFaceType[yres];
            }



            //generate view rays

            //        rotate to the left edge     
            var _xrot = (angleStep * xres / 2.0);
            //        rotate to the top edge     
            var _yrot = (angleStep * yres / 2.0);

            var _vdy = Point3.Rotate(nvd, xAxis, (float)(_yrot * Math.PI / 180));
            var _vdx = Point3.Rotate(_vdy, yAxis, (float)(_xrot * Math.PI / 180));

            TopCorner = _vdx;

            Point3 xAxisTemp = Point3.Cross(nvd, yAxis);
            xAxisTemp.Normalize();


            for (int x = 0; x < xres ; x++)
            {
                var xrot = -(xres - x) * angleStep;
                var vdx = Point3.Rotate(TopCorner, yAxis, (float)(xrot * Math.PI / 180));

                xAxisTemp = Point3.Cross(yAxis, vdx);

                for (int y = 0; y < yres; y++)
                {
                     
                     
                    var yrot =  - ( yres - y ) * angleStep;

                    var vdy = Point3.Rotate(vdx, -xAxisTemp, (float)(yrot * Math.PI / 180));

                    ImageRays[x][y] = vdy;

                }
            }




            aspectRatio = xres / yres;
            cameraPlaneDist = 1.0f / XMath.Tan((float)51.282 * XMath.PI / 360.0f); //51.282 come from vertical scene angle (smoSensor setting)
            reciprocalHeight = 1.0f / yres;
            reciprocalWidth = 1.0f / xres;


        }
        /*

        public SmoImage(Point3 pt, int Resolution)
        {

            this.Pt = pt;
            this.Dir = new Point3(0,1,0);

            double horizontalViewAngle = 360;
            double verticalViewAngle = 180;

            //Define Left, right, up, down vectors to measure room dimension
            Point3 nvd = new Point3(0, 1, 0);
            nvd.Normalize();
            Dir = nvd;

            Point3 vup = new Point3(0, 0, 1);

            xAxis = Point3.Cross(nvd, vup);
            xAxis.Normalize();

            yAxis = Point3.Cross(nvd, -xAxis);
            yAxis.Normalize();


            xres = (Resolution / 2) * 2;
            angleStep = horizontalViewAngle / xres;
            yres = ((int)(verticalViewAngle / (double)angleStep) / 2) * 2;



            ImageRays = new Point3[xres][];


            Hits = new Point3[xres][];
            DepthMap = new double[xres][];
            LabelMap = new SmoFace.SmoFaceType[xres][];

            for (int i = 0; i < xres; i++)
            {
                ImageRays[i] = new Point3[yres];

                Hits[i] = new Point3[yres];
                DepthMap[i] = new double[yres];
                LabelMap[i] = new SmoFace.SmoFaceType[yres];
            }



            //generate view rays

            //        rotate to the left edge     
            var _xrot = (angleStep * xres / 2.0);
            //        rotate to the top edge     
            var _yrot = (angleStep * yres / 2.0);

            var _vdy = Point3.Rotate(nvd, xAxis, (float)(_yrot * Math.PI / 180));
            var _vdx = Point3.Rotate(_vdy, yAxis, (float)(_xrot * Math.PI / 180));

            TopCorner = _vdx;


            for (int x = 0; x < xres; x++)
            {
                for (int y = 0; y < yres; y++)
                {

                    var xrot = -(xres - x) * angleStep;

                    var yrot = -(yres - y) * angleStep;


                    var vdx = Point3.Rotate(TopCorner, yAxis, (float)(xrot * Math.PI / 180));

                    var vdy = Point3.Rotate(vdx, xAxis, (float)(yrot * Math.PI / 180));

                    ImageRays[x][y] = vdy;

                }
            }

        }
        */

        //split sphere image to multiple directions after compute image
        public static SmoImage FrameImages(SmoImage sphereImage, Point3 dir, double horizontalViewAngle, double verticalViewAngle)
        {
            SmoImage image = new SmoImage(sphereImage.Pt, dir, (int)Math.Ceiling(horizontalViewAngle / sphereImage.angleStep), horizontalViewAngle, verticalViewAngle);
            Point3 nvd = dir;
            nvd.Normalize();


            //calculate direction's [][]

            Point3 projectedDir = new Point3(dir.X, dir.Y, 0);

            
            double hAngle = Point3.AngleDegree(projectedDir, new Point3(0, -1, 0));
            double vAngle = Point3.AngleDegree(projectedDir, dir);
            

            
            for (int x = 0; x < image.xres; x++)
            {
                for (int y = 0; y < image.yres; y++)
                {
                    int offsetX = (int)Math.Floor(hAngle / sphereImage.angleStep) - (int)Math.Floor(horizontalViewAngle / sphereImage.angleStep / 2);
                    //int offsetY = (int)Math.Floor(vAngle / sphereImage.angleStep) - (int)Math.Floor(verticalViewAngle / sphereImage.angleStep / 2);


                    int sphereX = ((x + offsetX) + sphereImage.xres) % sphereImage.xres;
                    //int sphereY = ((y + offsetY) + (sphereImage.yres)/2 * 3) % sphereImage.yres;

                    image.ImageRays[x][y] = sphereImage.ImageRays[sphereX][y];
                    image.Hits[x][y] = sphereImage.Hits[sphereX][y];
                    image.DepthMap[x][y] = sphereImage.DepthMap[sphereX][y];
                    image.LabelMap[x][y] = sphereImage.LabelMap[sphereX][y];

                }
            }

            return image;

        }

        /// <summary>
        /// Imported from GPUTracer
        /// </summary>
        /// <param name="octree"></param>
        /// <param name="max"></param>
        public void ComputeImage( PointOctree<SmoFace> octree, double max)
        {
            /* original cpu calculation code
            for (int x = 0; x < this.xres; x++)
            {
                for (int y = 0; y < this.yres; y++)
                {
                    var pt = this.Pt;
                    var ray = this.ImageRays[x][y];

                    Point3 hit;
                    var face = SmoIntersect.IsVisible(octree, pt, ray, max, out hit);

                    if (face == null)
                    {
                        this.LabelMap[x][y] = SmoFace.SmoFaceType._UNSET_; 
                        continue;
                    }

                    double dist = Point3.Distance(hit, pt);
                    this.Hits[x][y] = hit;
                    this.DepthMap[x][y] = (hit - pt).Length;
                    this.LabelMap[x][y] = face.ViewContentType;

                }
            }

            */
            // Test GPU code (reference. ILGPU tutorial
            // 1.host and device setup
            Context context = Context.Create(builder => builder.Default().EnableAlgorithms());
            Accelerator device = context.GetPreferredDevice(preferCPU: false)
                                      .CreateAccelerator(context);

            int width = this.xres;
            int height = this.yres;

            // my GPU can handle around 10,000 when using the struct of arrays
            // 2.create cpu output storage
            double[] h_flattenDepthMap = new double[width * height];

            // 3.create device input storage (d_///: device)
            //MemoryBuffer2D<Point3, Stride2D.DenseY> canvasData = device.Allocate2DDenseY<Point3>(new Index2D(width, height));
            MemoryBuffer1D<double, Stride1D.Dense> d_flattenDepthMap = device.Allocate1D<double>(width * height);

            HpGPU.CanvasData c = new HpGPU.CanvasData(/*canvasData,*/ d_flattenDepthMap, width, height);

            HpGPU.HostParticleSystem h_particleSystem = new HpGPU.HostParticleSystem(device, particleCount, width, height);


            //loading kernels, not yet connected with data (memorybuffers), only set dataTypes and actions.
            //var frameBufferToBitmap = device.LoadAutoGroupedStreamKernel<Index2D, HpGPU.CanvasData>(HpGPU.CanvasData.CanvasToBitmap);
            var particleProcessingKernel = device.LoadAutoGroupedStreamKernel<Index1D, HpGPU.CanvasData, HpGPU.ParticleSystem>(HpGPU.ParticleSystem.particleKernel);

            var particleProcessingKernel2 = device.LoadAutoGroupedStreamKernel<>


            particleProcessingKernel(particleCount, c, h_particleSystem.deviceParticleSystem);
            device.Synchronize();


            for (int x = 0; x < this.xres; x++)
            {
                for (int y = 0; y < this.yres; y++)
                {
                    var pt = this.Pt;
                    var ray = this.ImageRays[x][y];

                    Point3 hit;
                    var face = SmoIntersect.IsVisible(octree, pt, ray, max, out hit);

                    if (face == null)
                    {
                        this.LabelMap[x][y] = SmoFace.SmoFaceType._UNSET_;
                        continue;
                    }

                    double dist = Point3.Distance(hit, pt);
                    this.Hits[x][y] = hit;
                    this.DepthMap[x][y] = (hit - pt).Length;
                    this.LabelMap[x][y] = face.ViewContentType;

                }
            }
            //frameBufferToBitmap(canvasData.Extent.ToIntIndex(), c);
            //device.Synchronize();

            d_flattenDepthMap.CopyToCPU(h_flattenDepthMap);

            //bitmap magic that ignores bitmap striding, be careful some sizes will mess up the striding
            //Bitmap b = new Bitmap(width, height, width * 3, PixelFormat.Format24bppRgb, Marshal.UnsafeAddrOfPinnedArrayElement(h_bitmapData, 0));
            //b.Save("out.bmp");

        }


        public Bitmap GetDepthBitmap()
        {
            double min= double.MaxValue;
            double max= double.MinValue;    
            for (int x = 0; x < this.xres; x++)
            {
                for (int y = 0; y < this.yres; y++)
                {
                    var val = this.DepthMap[x][y];
                    min = Math.Min(min, val);
                    max = Math.Max(max, val);
                }
            }


            var bitmap = new Bitmap(this.xres, this.yres);

            for (int x = 0; x < this.xres; x++)
            {
                for (int y = 0; y < this.yres; y++)
                {
                    var val = this.DepthMap[x][y];
                    var remap = ColorGenerator.Remap(val, min, max, 0, 1);
                    var pixColor = ColorGenerator.Turbo.ReturnTurboColor(remap);

                    bitmap.SetPixel(this.xres -x-1 , this.yres-y-1, pixColor);    

                }
            }

            return bitmap;
        }

        public Bitmap GetLabelBitmap()
        {

            double min = 0;
            double max = Enum.GetNames(typeof(SmoFace.SmoFaceType)).Length;    
            
            var bitmap = new Bitmap(this.xres, this.yres);

            for (int x = 0; x < this.xres; x++)
            {
                for (int y = 0; y < this.yres; y++)
                {

                    double val = (double)((int) this.LabelMap[x][y]);
                    var remap = ColorGenerator.Remap(val, min, max, 0, 1);
                    var pixColor = ColorGenerator.Inferno.ReturnInfernoColor(remap);

                    bitmap.SetPixel(this.xres - x - 1, this.yres-y-1, pixColor);

                }
            }

            return bitmap;
        }


        private Ray rayFromUnit(float x, float y)
        {
            Point3 xContrib = axis.x * -x * aspectRatio;
            Point3 yContrib = axis.y * -y;
            Point3 zContrib = axis.z * cameraPlaneDist;
            
            Point3 direction = (xContrib + yContrib + zContrib);
            direction.Normalize();

            return new Ray(Pt, direction);
        }


        public Ray GetRay(float x, float y)
        {
            return rayFromUnit(2f * (x * (1/xres)) - 1f, 2f * (y * (1/yres)) - 1f);
        }
    }

    public readonly struct OrthoNormalBasis
    {
        public readonly Point3 x;
        public readonly Point3 y;
        public readonly Point3 z;

        public OrthoNormalBasis(Point3 x, Point3 y, Point3 z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }


        public Point3 transform(Point3 pos)
        {
            return x * pos.X + y * pos.Y + z * pos.Z;
        }


        public static OrthoNormalBasis fromXY(Point3 x, Point3 y)
        {
            Point3 zz = (Point3.Cross(x, y)).Normalized;
            Point3 yy = (Point3.Cross(zz, x)).Normalized;
            return new OrthoNormalBasis(x, yy, zz);
        }


        public static OrthoNormalBasis fromYX(Point3 y, Point3 x)
        {
            Point3 zz = (Point3.Cross(x, y)).Normalized;
            Point3 xx = (Point3.Cross(y, zz)).Normalized;
            return new OrthoNormalBasis(xx, y, zz);
        }


        public static OrthoNormalBasis fromXZ(Point3 x, Point3 z)
        {
            Point3 yy = (Point3.Cross(z, x)).Normalized;
            Point3 zz = (Point3.Cross(x, yy)).Normalized;
            return new OrthoNormalBasis(x, yy, zz);
        }


        public static OrthoNormalBasis fromZX(Point3 z, Point3 x)
        {
            Point3 yy = (Point3.Cross(z, x)).Normalized;
            Point3 xx = (Point3.Cross(yy, z)).Normalized;
            return new OrthoNormalBasis(xx, yy, z);
        }


        public static OrthoNormalBasis fromYZ(Point3 y, Point3 z)
        {
            Point3 xx = (Point3.Cross(y, z)).Normalized;
            Point3 zz = (Point3.Cross(xx, y)).Normalized;
            return new OrthoNormalBasis(xx, y, zz);
        }


        public static OrthoNormalBasis fromZY(Point3 z, Point3 y)
        {
            Point3 xx = (Point3.Cross(y, z)).Normalized;
            Point3 yy = (Point3.Cross(z, xx)).Normalized;
            return new OrthoNormalBasis(xx, yy, z);
        }


        public static OrthoNormalBasis fromZ(Point3 z)
        {
            Point3 xx;
            if (XMath.Abs(Point3.Dot(z, new Point3(1, 0, 0))) > 0.99999f)
            {
                xx = (Point3.Cross(new Point3(0, 1, 0), z)).Normalized;
                
            }
            else
            {
                xx = (Point3.Cross(new Point3(1, 0, 0), z)).Normalized;
            }
            Point3 yy = (Point3.Cross(z, xx)).Normalized;
            return new OrthoNormalBasis(xx, yy, z);
        }
    }
}
