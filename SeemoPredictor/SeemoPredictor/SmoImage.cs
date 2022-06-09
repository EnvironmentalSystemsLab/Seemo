using SeemoPredictor.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;



namespace SeemoPredictor
{
    public class SmoImage
    {
        public int xres { get; set; }
        public int yres { get; set; }

        public double angleStep { get; set; }  

        public Point3 Pt { get; set; }
        public Point3 Dir { get; set; }
        public double Height { get; set; }
        public Point3 TopCorner { get; set; }
        public Point3 xAxis { get; set; }
        public Point3 yAxis { get; set; }


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

        //raycast2 output
        public double[][] WindowDepthMap { get; set; }
        public double[] WindowDepthMapFlat
        {
            get
            {
                if (WindowDepthMap == null) return null;
                return WindowDepthMap.SelectMany(a => a).ToArray();
            }
        }

        public SmoFace.SmoFaceType[][] WindowLabelMap { get; set; }
        public string[] WindowLabelMapFlat
        {
            get
            {
                if (WindowLabelMap == null) return null;
                var arr = WindowLabelMap.SelectMany(a => a).ToArray();
                return arr.Select(a => a.ToString()).ToArray();
            }
        }
        
        public Point3[][] WindowHits { get; set; }
        public Point3[] WindowHitsFlat
        {
            get
            {
                if (WindowHits == null) return null;
                return WindowHits.SelectMany(a => a).ToArray();
            }
        }

        public Point3[][] WindowNormals { get; set; }
        public Point3[] WindowNormalsFlat
        {
            get
            {
                if (WindowNormals == null) return null;
                return WindowNormals.SelectMany(a => a).ToArray();
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


            xres = (Resolution / 2) * 2;
            angleStep = horizontalViewAngle / xres;
            yres = ((int)(verticalViewAngle / (double)angleStep) / 2) * 2;

          

            ImageRays = new Point3[xres][];


            Hits = new Point3[xres][];
            DepthMap = new double[xres][];
            LabelMap = new SmoFace.SmoFaceType[xres][];

            WindowHits = new Point3[xres][];
            WindowNormals = new Point3[xres][];
            WindowDepthMap = new double[xres][];
            WindowLabelMap = new SmoFace.SmoFaceType[xres][];

            for (int i = 0; i < xres; i++) {
                ImageRays[i] = new Point3[yres];

                Hits[i] = new Point3[yres];
                DepthMap[i] = new double[yres];
                LabelMap[i] = new SmoFace.SmoFaceType[yres];

                WindowHits[i] = new Point3[yres];
                WindowNormals[i] = new Point3[yres];
                WindowDepthMap[i] = new double[yres];
                WindowLabelMap[i] = new SmoFace.SmoFaceType[yres];
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

        }
        

        //split sphere image to multiple directions after compute image
        public static SmoImage FrameImages(SmoImage sphereImage, Point3 dir, double horizontalViewAngle, double verticalViewAngle)
        {
            SmoImage image = new SmoImage(sphereImage.Pt, dir, (int)Math.Ceiling(horizontalViewAngle / sphereImage.angleStep), horizontalViewAngle, verticalViewAngle);
            Point3 nvd = dir;
            nvd.Normalize();


            //calculate direction's [][]

            Point3 projectedDir = new Point3(dir.X, dir.Y, 0);

            
            double hAngle = Point3.AngleDegree(new Point3(0, -1, 0), projectedDir);
            double vAngle = Point3.AngleDegree(dir, projectedDir);
            

            
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

                    image.WindowHits[x][y] = sphereImage.WindowHits[sphereX][y];
                    image.WindowNormals[x][y] = sphereImage.WindowNormals[sphereX][y];
                    image.WindowDepthMap[x][y] = sphereImage.WindowDepthMap[sphereX][y];
                    image.WindowLabelMap[x][y] = sphereImage.WindowLabelMap[sphereX][y];

                }
            }

            return image;

        }


        public void ComputeImage(PointOctree<SmoFace> octreeEnv, PointOctree<SmoFace> octreeWindow, double max) //compute using gpu
        {

            for (int x = 0; x < this.xres; x++)
            {
                for (int y = 0; y < this.yres; y++)
                {
                    var pt = this.Pt;
                    var ray = this.ImageRays[x][y];

                    Point3 hit;
                    var face = SmoIntersect.IsVisible(octreeEnv, pt, ray, max, out hit);

                    if (face == null)
                    {
                        this.LabelMap[x][y] = SmoFace.SmoFaceType._UNSET_;
                        continue;
                    }
                    double dist = Point3.Distance(hit, pt);
                    this.Hits[x][y] = hit;
                    this.DepthMap[x][y] = (hit - pt).Length;
                    this.LabelMap[x][y] = face.ViewContentType;

                    //check intersection with windows
                    if (face.ViewContentType != SmoFace.SmoFaceType.Interior)
                    {
                        Point3 hit2;
                        var face2 = SmoIntersect.IsVisible(octreeWindow, pt, ray, max, out hit2);

                        if (face2 == null)
                        {
                            this.WindowLabelMap[x][y] = SmoFace.SmoFaceType._UNSET_;
                            continue;
                        }

                        double dist2 = Point3.Distance(hit2, pt);
                        this.WindowHits[x][y] = hit2;
                        this.WindowDepthMap[x][y] = (hit2 - pt).Length;
                        this.WindowLabelMap[x][y] = face2.ViewContentType;
                        this.WindowNormals[x][y] = face2.Normal;
                    }
                }
            }
        }

        /// <summary>
        /// convert gpu result to seemo image class for feature computing
        /// </summary>
        /// <param name="distances"></param>
        /// <param name="lables"></param>
        

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
                    var remap = ColorGenerator.Remap(val, 0, max, 0, 1);  //original ColorGenerator.Remap(val, min, max, 0, 1)
                    var pixColor = ColorGenerator.Turbo.ReturnTurboColor(remap);

                    bitmap.SetPixel(this.xres - x - 1 , this.yres - y - 1, pixColor);    

                }
            }

            return bitmap;
        }

        public Bitmap GetWindowDepthBitmap()
        {
            double min = double.MaxValue;
            double max = double.MinValue;
            for (int x = 0; x < this.xres; x++)
            {
                for (int y = 0; y < this.yres; y++)
                {
                    var val = this.WindowDepthMap[x][y];
                    min = Math.Min(min, val);
                    max = Math.Max(max, val);
                }
            }


            var bitmap = new Bitmap(this.xres, this.yres);

            for (int x = 0; x < this.xres; x++)
            {
                for (int y = 0; y < this.yres; y++)
                {
                    var val = this.WindowDepthMap[x][y];
                    var remap = ColorGenerator.Remap(val, 0, max, 0, 1);  //original ColorGenerator.Remap(val, min, max, 0, 1)
                    var pixColor = ColorGenerator.Turbo.ReturnTurboColor(remap);

                    bitmap.SetPixel(this.xres - x - 1, this.yres - y - 1, pixColor);

                }
            }

            return bitmap;
        }

        public Bitmap GetWindowNormalBitmap()
        {
            double min = double.MaxValue;
            double max = double.MinValue;
            for (int x = 0; x < this.xres; x++)
            {
                for (int y = 0; y < this.yres; y++)
                {
                    var val = this.WindowNormals[x][y].Length;
                    min = Math.Min(min, val);
                    max = Math.Max(max, val);
                }
            }


            var bitmap = new Bitmap(this.xres, this.yres);

            for (int x = 0; x < this.xres; x++)
            {
                for (int y = 0; y < this.yres; y++)
                {
                    var val = this.WindowNormals[x][y].Length;
                    var remap = ColorGenerator.Remap(val, 0, max, 0, 1);  //original ColorGenerator.Remap(val, min, max, 0, 1)
                    var pixColor = ColorGenerator.Turbo.ReturnTurboColor(remap);

                    bitmap.SetPixel(this.xres - x - 1, this.yres - y - 1, pixColor);

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

                    bitmap.SetPixel(this.xres - x - 1, this.yres - y - 1, pixColor);

                }
            }

            return bitmap;
        }

        public Bitmap GetWindowLabelBitmap()
        {

            double min = 0;
            double max = Enum.GetNames(typeof(SmoFace.SmoFaceType)).Length;

            var bitmap = new Bitmap(this.xres, this.yres);

            for (int x = 0; x < this.xres; x++)
            {
                for (int y = 0; y < this.yres; y++)
                {

                    double val = (double)((int)this.WindowLabelMap[x][y]);
                    var remap = ColorGenerator.Remap(val, min, max, 0, 1);
                    var pixColor = ColorGenerator.Inferno.ReturnInfernoColor(remap);

                    bitmap.SetPixel(this.xres - x - 1, this.yres - y - 1, pixColor);

                }
            }

            return bitmap;
        }

    }
}
