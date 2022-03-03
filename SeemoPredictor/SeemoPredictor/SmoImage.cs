using SeemoPredictor.SeemoGeo;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeemoPredictor
{
    public class SmoImage
    {
        public int xres { get; set; }
        public int yres { get; set; }

        double angleStep { get; set; }  

        public SmoPoint3 Pt { get; set; }
        public SmoPoint3 Dir { get; set; }
        public SmoPoint3 TopCorner { get; set; }
        public SmoPoint3 xAxis { get; set; }
        public SmoPoint3 yAxis { get; set; }


        //raycast input
        public SmoPoint3[][] ImageRays { get; set; }
        public SmoPoint3[] ImageRaysFlat
        {
            get { 
            
                if(ImageRays == null)return null;
                return ImageRays.SelectMany(a => a).ToArray();

            }
        }

        //raycast output
        public SmoPoint3[][] Hits { get; set; }
        public SmoPoint3[] HitsFlat
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

        public SmoImage(SmoPoint3 Pt, SmoPoint3 dir, int Resolution, double horizontalViewAngle, double verticalViewAngle) {
             
            //Define Left, right, up, down vectors to measure room dimension
            SmoPoint3 nvd = dir;
            nvd.Normalize();
            Dir = nvd;

            SmoPoint3 vup = new SmoPoint3(0, 0, 1);

             xAxis = SmoPoint3.Cross(nvd, vup);
            xAxis.Normalize();

             yAxis = SmoPoint3.Cross(nvd, -xAxis);
            yAxis.Normalize();


            xres = (Resolution / 2) * 2;
            angleStep = horizontalViewAngle / xres;
            yres = ((int)(verticalViewAngle / (double)angleStep) / 2) * 2;

            Pt = Pt;
            Dir = Dir;

            ImageRays = new SmoPoint3[xres][];


            Hits = new SmoPoint3[xres][];
            DepthMap = new double[xres][];
            LabelMap = new SmoFace.SmoFaceType[xres][];

            for (int i = 0; i < xres; i++) {
                ImageRays[i] = new SmoPoint3[yres];

                Hits[i] = new SmoPoint3[yres];
                DepthMap[i] = new double[yres];
                LabelMap[i] = new SmoFace.SmoFaceType[yres];
            }



            //generate view rays

            //        rotate to the left edge     
            var _xrot = (angleStep * xres / 2.0);
            //        rotate to the top edge     
            var _yrot = (angleStep * yres / 2.0);

            var _vdy = SmoPoint3.Rotate(nvd, xAxis, (float)(_yrot * Math.PI / 180));
            var _vdx = SmoPoint3.Rotate(_vdy, yAxis, (float)(_xrot * Math.PI / 180));

            TopCorner = _vdx;


            for (int x = 0; x < xres ; x++)
            {
                for (int y = 0; y < yres; y++)
                {
                     
                    var xrot =  - ( xres - x ) * angleStep;
                     
                    var yrot =  - ( yres - y ) * angleStep;


                    var vdx = SmoPoint3.Rotate(TopCorner, yAxis, (float)(xrot * Math.PI / 180));

                    var vdy = SmoPoint3.Rotate(vdx, xAxis, (float)(yrot * Math.PI / 180));

                    ImageRays[x][y] = vdy;

                }
            }


             




        }



        public void ComputeImage(  SmoPointOctree<SmoFace> octree, double max)
        {

            for (int x = 0; x < this.xres; x++)
            {
                for (int y = 0; y < this.yres; y++)
                {
                    var pt = this.Pt;
                    var ray = this.ImageRays[x][y];

                    SmoPoint3 hit;
                    var face = SmoIntersect.IsVisible(octree, pt, ray, max, out hit);

                    if (face == null) continue;

                    double dist = SmoPoint3.Distance(hit, pt);
                    this.Hits[x][y] = hit;
                    this.DepthMap[x][y] = (hit - pt).Length;
                    this.LabelMap[x][y] = face.ViewContentType;

                }
            }
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

                    bitmap.SetPixel(x, this.yres-y-1, pixColor);    

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

                    bitmap.SetPixel(x, this.yres-y-1, pixColor);

                }
            }

            return bitmap;
        }

    }
}
